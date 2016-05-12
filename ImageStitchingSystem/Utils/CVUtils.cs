using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using ImageStitchingSystem.Exceptions;
using ImageStitchingSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using ZedGraph;

namespace ImageStitchingSystem.Utils
{
    public static class CvUtils
    {
        /// <summary>
        /// 寻找特征点
        /// </summary>
        /// <param name="modelImage"></param>
        /// <param name="observedImage"></param>
        /// <param name="matchTime"></param>
        /// <param name="modelKeyPoints"></param>
        /// <param name="observedKeyPoints"></param>
        /// <param name="matches"></param>
        /// <param name="mask"></param>
        /// <param name="homography"></param>
        public static void FindMatch(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.8;
            double hessianThresh = 300;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

#if !__IOS__
            if (CudaInvoke.HasCuda)
            {
                CudaSURF surfCuda = new CudaSURF((float)hessianThresh);
                using (GpuMat gpuModelImage = new GpuMat(modelImage))
                //extract features from the object image
                using (GpuMat gpuModelKeyPoints = surfCuda.DetectKeyPointsRaw(gpuModelImage, null))
                using (GpuMat gpuModelDescriptors = surfCuda.ComputeDescriptorsRaw(gpuModelImage, null, gpuModelKeyPoints))
                using (CudaBFMatcher matcher = new CudaBFMatcher(DistanceType.L2))
                {
                    surfCuda.DownloadKeypoints(gpuModelKeyPoints, modelKeyPoints);
                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    using (GpuMat gpuObservedImage = new GpuMat(observedImage))
                    using (GpuMat gpuObservedKeyPoints = surfCuda.DetectKeyPointsRaw(gpuObservedImage, null))
                    using (GpuMat gpuObservedDescriptors = surfCuda.ComputeDescriptorsRaw(gpuObservedImage, null, gpuObservedKeyPoints))
                    //using (GpuMat tmp = new GpuMat())
                    //using (Stream stream = new Stream())
                    {
                        matcher.KnnMatch(gpuObservedDescriptors, gpuModelDescriptors, matches, k);

                        surfCuda.DownloadKeypoints(gpuObservedKeyPoints, observedKeyPoints);

                        mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                        mask.SetTo(new MCvScalar(255));
                        Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                        int nonZeroCount = CvInvoke.CountNonZero(mask);
                        if (nonZeroCount >= 4)
                        {
                            nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                               matches, mask, 1.5, 20);
                            if (nonZeroCount >= 4)
                                homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                                   observedKeyPoints, matches, mask, 2);
                        }
                    }
                    watch.Stop();
                }
            }
            else
#endif
            {
                using (UMat uModelImage = modelImage.ToUMat(AccessType.Read))
                using (UMat uObservedImage = observedImage.ToUMat(AccessType.Read))
                {
                    SURF surfCpu = new SURF(hessianThresh);
                    //extract features from the object image
                    UMat modelDescriptors = new UMat();
                    surfCpu.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    UMat observedDescriptors = new UMat();
                    surfCpu.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                    BFMatcher matcher = new BFMatcher(DistanceType.L2);
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                           matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                               observedKeyPoints, matches, mask, 2);
                    }

                    watch.Stop();
                }
            }
            matchTime = watch.ElapsedMilliseconds;
        }
        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Mat Draw(Mat modelImage, Mat observedImage, out long matchTime)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography);

                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                   matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);

                #region draw the projected region on the image

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);

                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }

                }

                #endregion

                return result;

            }
        }

        public static Image<Bgr, byte> Draw(Image<Bgr, byte> trainImage, Image<Bgr, byte> queryImage, List<FeaturePoint> matchers)
        {
            Image<Bgr, byte> result;

            result = Merge(new Image<Bgr, byte>[] { trainImage, queryImage });
            var width = trainImage.Width;
            result = DrawMatchs(result, width, matchers);
            return result;
        }

        public static Image<Bgr, byte> Draw(Image<Bgr, byte> img, List<MKeyPoint> points)
        {
            foreach (var v in points)
            {
                int index = points.IndexOf(v);
                CvInvoke.Circle(img, Point.Round(v.Point), 5, new MCvScalar(0, 0, 255));
                CvInvoke.PutText(img, index.ToString(), Point.Round(v.Point), new FontFace(), 0.7, new MCvScalar(0, 0, 255), 2);
            }
            return img;
        }

        public static Image<Bgr, byte> DrawMatchs(Image<Bgr, byte> img, int widths, List<FeaturePoint> matchers)
        {
            Random r = new Random();
            foreach (var v in matchers)
            {
                int index = matchers.IndexOf(v);
                Point p1 = Point.Round(v.TrainPoint.Point);
                Point p2 = Point.Round(v.QueryPoint.Point);
                Point p = new Point(p2.X + widths, p2.Y);
                CvInvoke.Line(img, p1, p, new MCvScalar(r.Next(255), r.Next(255), r.Next(255)));
                CvInvoke.Circle(img, p1, 5, new MCvScalar(0, 0, 255));
                CvInvoke.Circle(img, p, 5, new MCvScalar(255, 0, 0));
                CvInvoke.PutText(img, v.Index.ToString(), p1, new FontFace(), 0.7, new MCvScalar(0, 0, 255), 2);
                CvInvoke.PutText(img, v.Index.ToString(), p, new FontFace(), 0.7, new MCvScalar(255, 0, 0), 2);
            }

            return img;
        }

        public static Image<Bgr, byte> Merge(Image<Bgr, byte>[] args, Orientation o = Orientation.Horizontal)
        {
            switch (o)
            {
                case Orientation.Horizontal:
                    return MergeByHorizontal(args);

                case Orientation.Vertical:
                    throw new UnSupportException();

                default:
                    throw new UnSupportException();
            }
        }

        public enum Orientation
        {
            Horizontal, Vertical
        }

        private static Image<Bgr, byte> MergeByHorizontal(Image<Bgr, byte>[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException();
            }
            if (args.Length == 0)
            {
                throw new IndexOutOfRangeException();
            }
            int h = 0;
            int height = args.Max(o => o.Height);
            int width = args.Sum(o => o.Width);
            Size size = new Size(width + 10, height + 10);
            Image<Bgr, byte> result = new Image<Bgr, byte>(size);

            foreach (var v in args)
            {
                Rectangle rect = new Rectangle(new Point(h, 0), v.Size);
                result.ROI = rect;
                v.CopyTo(result);
                result.ROI = Rectangle.Empty;
                h += v.Width;
            }
            return result;

        }

        public static Image<Bgr, byte> DrawCursor(Image<Bgr, byte> img, Point point)
        {
            //CvInvoke.D
            return img;
        }

        public static Image<Bgr, byte> DrawCenterCross(Image<Bgr, byte> img)
        {
            Point top = new Point(img.Width / 2, 0);
            Point bottom = new Point(img.Width / 2, img.Height);
            Point left = new Point(0, img.Height / 2);
            Point right = new Point(img.Width, img.Height / 2);
            img.Draw(new LineSegment2D(top, bottom), new Bgr(255, 255, 255), 1);
            img.Draw(new LineSegment2D(left, right), new Bgr(255, 255, 255), 1);
            return img;
        }

        public static Image<Bgr, byte> DrawTextWithBackground(Image<Bgr, byte> img, string text)
        {
            throw new NotImplementedException();
        }

        public static void DrawPointAndCursor(Image<Bgr, byte> img1, Image<Bgr, byte> img2, FeaturePoint v, int number, MCvScalar color)
        {
            CvInvoke.PutText(img1, number + "", new System.Drawing.Point((int)v.Lx, (int)v.Ly), FontFace.HersheyComplex, 0.7, color);
            CvInvoke.PutText(img2, number + "", new System.Drawing.Point((int)v.Rx, (int)v.Ry), FontFace.HersheyComplex, 0.7, color);
            img1.Draw(new Cross2DF(v.TrainPoint.Point, 15, 15), new Bgr(255, 255, 255), 1);
            img2.Draw(new Cross2DF(v.QueryPoint.Point, 15, 15), new Bgr(255, 255, 255), 1);
        }

        public static void DrawPointAndCursor(Image<Bgr, byte> img1, Image<Bgr, byte> img2, FeaturePoint v, string text, MCvScalar color)
        {
            CvInvoke.PutText(img1, text + "", new System.Drawing.Point((int)v.Lx, (int)v.Ly), FontFace.HersheyComplex, 0.7, color);
            CvInvoke.PutText(img2, text + "", new System.Drawing.Point((int)v.Rx, (int)v.Ry), FontFace.HersheyComplex, 0.7, color);
            img1.Draw(new Cross2DF(v.TrainPoint.Point, 15, 15), new Bgr(255, 255, 255), 1);
            img2.Draw(new Cross2DF(v.QueryPoint.Point, 15, 15), new Bgr(255, 255, 255), 1);
        }

        public static void DrawPointAndCursor(Image<Bgr, byte> img, System.Drawing.Point p, string text, MCvScalar color)
        {
            CvInvoke.PutText(img, text + "", p, FontFace.HersheyComplex, 0.7, color);
            img.Draw(new Cross2DF(p, 15, 15), new Bgr(255, 255, 255), 1);
        }

        public static void DrawPointAndCursorAndImage(Image<Bgr, byte> img1, Image<Bgr, byte> img2, FeaturePoint v, int number, MCvScalar color)
        {
            DrawPointAndCursor(img1, img2, v, number, color);
        }

        public static MCvScalar GetColor(int n)
        {
            Random r = new Random(n);
            int R = r.Next(255);
            int g = r.Next(255);
            int b = r.Next(255);
            return new MCvScalar(b, g, R);
        }

        public static Mat GetPerspectiveTransform(PointF[] img1, PointF[] img2)
        {
            img1 = img1.Take(4).ToArray();
            img2 = img2.Take(4).ToArray();
            Mat result = null;
            try
            {
                result = CvInvoke.GetPerspectiveTransform(img1, img2);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public static IntPtr CreatePointListPointer(IList<PointF> points, int pointCount)
        {
            IntPtr result = CvInvoke.cvCreateMat(pointCount, 2, DepthType.Cv32F);

            for (int i = 0; i < pointCount; i++)
            {
                double currentX = points[i].X;
                double currentY = points[i].Y;
                CvInvoke.cvSet2D(result, i, 0, new MCvScalar(currentX));
                CvInvoke.cvSet2D(result, i, 1, new MCvScalar(currentY));
            }

            return result;
        }

        public static void CopyTo(Image<Bgr, byte> img1, Image<Bgr, byte> img2, Func<Bgr, bool> predicate)
        {
            for (int i = 0; i < img1.Rows; i++)
            {
                for (int j = 0; j < img1.Cols; j++)
                {
                    Bgr a = img1[i, j];
                    if (predicate.Invoke(a))
                    {
                        img2[i, j] = a;
                    }
                }
            }
        }

        public static void CopyTo<TColor, TDepth>(Image<TColor,TDepth> img1, Image<TColor, TDepth> img2,Func<TColor, bool> predicate,Func<TColor, TColor,TColor> fun) where TColor : struct ,IColor where TDepth:new()
        {
            for (int i = 0; i < img1.Rows; i++)
            {
                for (int j = 0; j < img1.Cols; j++)
                {
                    TColor a = img1[i, j];
                    if (predicate.Invoke(a))
                    {
                        img2[i,j]=fun.Invoke(a, img2[i, j]);
                    }
                }
            }
        }

        public static bool PolygonClip(List<Point> poly1, List<Point> poly2, List<Point> interPoly)
        {
            if (poly1.Count < 3 || poly2.Count < 3)
            {
                return false;
            }

            long x = 0, y = 0;
            //计算多边形交点
            for (int i = 0; i < poly1.Count; i++)
            {
                int poly1_next_idx = (i + 1) % poly1.Count;
                for (int j = 0; j < poly2.Count; j++)
                {
                    int poly2_next_idx = (j + 1) % poly2.Count;
                    if (GetCrossPoint(poly1[i], poly1[poly1_next_idx],
                        poly2[j], poly2[poly2_next_idx],
                        x, y))
                    {
                        interPoly.Add(new Point((int)x, (int)y));
                    }
                }
            }

            //计算多边形内部点
            for (int i = 0; i < poly1.Count; i++)
            {
                if (IsPointInPolygon(poly2, poly1[i]))
                {
                    interPoly.Add(poly1[i]);
                }
            }
            for (int i = 0; i < poly2.Count; i++)
            {
                if (IsPointInPolygon(poly1, poly2[i]))
                {
                    interPoly.Add(poly2[i]);
                }
            }

            if (interPoly.Count <= 0)
                return false;

            //点集排序 
            ClockwiseSortPoints(interPoly);
            return true;
        }

        //排斥实验
        public static bool IsRectCross(Point p1, Point p2, Point q1, Point q2)
        {
            bool ret = Math.Min(p1.X, p2.X) <= Math.Max(q1.X, q2.X) &&
                         Math.Min(q1.X, q2.X) <= Math.Max(p1.X, p2.X) &&
                         Math.Min(p1.Y, p2.Y) <= Math.Max(q1.Y, q2.Y) &&
                         Math.Min(q1.Y, q2.Y) <= Math.Max(p1.Y, p2.Y);
            return ret;
        }
        //跨立判断
        public static bool IsLineSegmentCross(Point pFirst1, Point pFirst2, Point pSecond1, Point pSecond2)
        {
            long line1 = pFirst1.X * (pSecond1.Y - pFirst2.Y) +
                         pFirst2.X * (pFirst1.Y - pSecond1.Y) +
                         pSecond1.X * (pFirst2.Y - pFirst1.Y);
            long line2 = pFirst1.X * (pSecond2.Y - pFirst2.Y) +
                         pFirst2.X * (pFirst1.Y - pSecond2.Y) +
                         pSecond2.X * (pFirst2.Y - pFirst1.Y);
            if (((line1 ^ line2) >= 0) && !(line1 == 0 && line2 == 0))
                return false;

            line1 = pSecond1.X * (pFirst1.Y - pSecond2.Y) +
                pSecond2.X * (pSecond1.Y - pFirst1.Y) +
                pFirst1.X * (pSecond2.Y - pSecond1.Y);
            line2 = pSecond1.X * (pFirst2.Y - pSecond2.Y) +
                pSecond2.X * (pSecond1.Y - pFirst2.Y) +
                pFirst2.X * (pSecond2.Y - pSecond1.Y);
            if (((line1 ^ line2) >= 0) && !(line1 == 0 && line2 == 0))
                return false;
            return true;
        }

        public static bool GetCrossPoint(Point p1, Point p2, Point q1, Point q2, long x, long y)
        {
            if (IsRectCross(p1, p2, q1, q2))
            {
                if (IsLineSegmentCross(p1, p2, q1, q2))
                {
                    //求交点
                    long tmpLeft = (q2.X - q1.X) * (p1.Y - p2.Y) - (p2.X - p1.X) * (q1.Y - q2.Y);
                    long tmpRight = (p1.Y - q1.Y) * (p2.X - p1.X) * (q2.X - q1.X) + q1.X * (q2.Y - q1.Y) * (p2.X - p1.X) - p1.X * (p2.Y - p1.Y) * (q2.X - q1.X);

                    x = (int)((double)tmpRight / (double)tmpLeft);

                    tmpLeft = (p1.X - p2.X) * (q2.Y - q1.Y) - (p2.Y - p1.Y) * (q1.X - q2.X);
                    tmpRight = p2.Y * (p1.X - p2.X) * (q2.Y - q1.Y) + (q2.X - p2.X) * (q2.Y - q1.Y) * (p1.Y - p2.Y) - q2.Y * (q1.X - q2.X) * (p2.Y - p1.Y);
                    y = (int)((double)tmpRight / (double)tmpLeft);
                    return true;
                }
            }
            return false;
        }

        public static bool IsPointInPolygon(List<Point> poly, Point pt)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((((poly[i].Y <= pt.Y) && (pt.Y < poly[j].Y)) ||
                    ((poly[j].Y <= pt.Y) && (pt.Y < poly[i].Y)))
                    && (pt.X < (poly[j].X - poly[i].X) * (pt.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
                {
                    c = !c;
                }
            }
            return c;
        }

        //若点a大于点b,即点a在点b顺时针方向,返回true,否则返回false
        public static bool PointCmp(Point a, Point b, Point center)
        {
            if (a.X >= 0 && b.X < 0)
                return true;
            if (a.X == 0 && b.X == 0)
                return a.Y > b.Y;
            //向量OA和向量OB的叉积
            int det = (a.X - center.X) * (b.Y - center.Y) - (b.X - center.X) * (a.Y - center.Y);
            if (det < 0)
                return true;
            if (det > 0)
                return false;
            //向量OA和向量OB共线，以距离判断大小
            int d1 = (a.X - center.X) * (a.X - center.X) + (a.Y - center.Y) * (a.Y - center.Y);
            int d2 = (b.X - center.X) * (b.X - center.Y) + (b.Y - center.Y) * (b.Y - center.Y);
            return d1 > d2;
        }

        public static void ClockwiseSortPoints(List<Point> vPoints)
        {
            //计算重心
            Point center=new Point();
            double x = 0, y = 0;
            for (int i = 0; i < vPoints.Count; i++)
            {
                x += vPoints[i].X;
                y += vPoints[i].Y;
            }
            center.X = (int)x / vPoints.Count;
            center.Y = (int)y / vPoints.Count;

            //冒泡排序
            for (int i = 0; i < vPoints.Count - 1; i++)
            {
                for (int j = 0; j < vPoints.Count - i - 1; j++)
                {
                    if (PointCmp(vPoints[j], vPoints[j + 1], center))
                    {
                        Point tmp = vPoints[j];
                        vPoints[j] = vPoints[j + 1];
                        vPoints[j + 1] = tmp;
                    }
                }
            }
        }

        public static string MatrixToString<T>(Matrix<T> data,string perfix="[",string suffix="]") where T : new() 
        {
            StringBuilder sBuilder=new StringBuilder();
            for (int i = 0; i < data.Rows;i++)
            {
                sBuilder.Append(perfix);
                for (int j = 0; j < data.Cols; j++)
                {
                    T t= data[i, j];
                    sBuilder.Append(perfix);
                    sBuilder.Append(t);
                    sBuilder.Append(suffix);
                }
                sBuilder.Append(suffix);
                sBuilder.Append("\n");
            }
            return sBuilder.ToString();
        }

        public static Image<Bgr, byte> CopyAndBlend(Image<Bgr, byte> img1, Image<Bgr, byte> img2) 
        {
            Image<Bgr, byte> result =new Image<Bgr, byte>(img1.Size);
            for (int i = 0; i < img2.Rows; i++)
            {
                for (int j = 0; j < img2.Cols; j++)
                {
                    Bgr a = img1[i, j];
                    Bgr b = img2[i, j];
                    if (a.IsBlack() && b.IsBlack()==false)
                    {
                        result[i, j] = b;
                    }
                    else if (a.IsBlack() == false && b.IsBlack())
                    {
                        result[i, j] = a;
                    }
                    else if (a.IsBlack() == false && b.IsBlack() == false)
                    {
                       result[i,j] = new Bgr(a.Red*0.5+b.Red*0.5, a.Green * 0.5 + b.Green * 0.5, a.Blue * 0.5 + b.Blue * 0.5);
                    }
                }
            }
            return result;
        }

        private static bool IsBlack<TColor>(this TColor tc) where TColor : struct,IColor
        {
            var m = tc.MCvScalar;
            if (m.V0 == 0 && m.V1 == 0 && m.V2 == 0)
            {
                return true;
            }
            return false;
        }
    }
}
