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

namespace ImageStitchingSystem.Utils
{
    public class CVUtils
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
                    SURF surfCPU = new SURF(hessianThresh);
                    //extract features from the object image
                    UMat modelDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    UMat observedDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
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

        public static Image<Bgr,byte> DrawTextWithBackground(Image<Bgr, byte> img,String text)
        {
            return img;
        }

    }
}
