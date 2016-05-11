using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.XFeatures2D;
using Emgu.CV;
using Emgu.CV.Util;
using ImageStitchingSystem.Utils;
using ImageStitchingSystem.Models;
using System.Globalization;
using System.IO;
using ZedGraph;
using Emgu.CV.CvEnum;
using MMath=ImageStitchingSystem.Utils.MMath;

using ImageStitchingSystem.UI.Weight;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace ImageStitchingSystem.UI
{
    /// <summary>
    /// SimpleView.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleView : UserControl
    {
        private MDMatch[] _matchers;

        #region 动态资源

        public FeaturePointCollection PointColletion;

        private FeaturePoint _selectedPoint;
        private int _selectedPointIndex = 0;

        private string _zoomStringL = "100%";
        private string _zoomStringR = "100%";


        private Point _leftPoint = new Point();
        private Point _rightPoint = new Point();

        #endregion


        public SimpleView()
        {
            InitializeComponent();
            PointColletion = (FeaturePointCollection)(Application.Current.Resources["FeaturePointCollection"] as ObjectDataProvider).Data;
            InitializeCommand();
        }

        //private RoutedCommand _findFeaturePointCommand = new RoutedCommand("findFeaturePoint",typeof(SimpleView));

        //初始化命令事件
        private void InitializeCommand()
        {
            //this.ButtonFindFeatures.Command = this._findFeaturePointCommand;
            //this._findFeaturePointCommand.InputGestures.Add(new KeyGesture(Key.F5));
            //this.ButtonFindFeatures.CommandTarget
        }


        /// <summary>
        /// 渲染
        /// </summary>
        private void BindPoints()
        {
            Binding bind = new Binding { Source = PointColletion };
            //todo the below ex is not used
            //PointColletion.OrderBy(o => o.Lx).ThenBy(o => o.Ly);
            ListViewPoints.SetBinding(ItemsControl.ItemsSourceProperty, bind);

            Image<Bgr, byte> l = new Image<Bgr, byte>((ComboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((ComboBoxR.SelectedItem as Photo).Source);
            int i = -1;
            Random romdom = new Random();

            var point = _selectedPoint;
            var index = _selectedPointIndex;

            UiHelper.ZoomImage(ImgL, ScrollViewerL, _zoomStringL);
            UiHelper.ZoomImage(ImgR, ScrollViewerR, _zoomStringR);



            //if(checkBoxIsClosePoint.IsChecked.Value==false)
            //{
            //    foreach (var v in pointColletion)
            //    {
            //        i++;
            //        var mcv = new MCvScalar(romdom.Next(255), romdom.Next(255), 0);
            //        CVUtils.DrawPointAndCursor(l, r, v, i, mcv);
            //    }
            //}

            //if (selectedPoint != null && selectedPointIndex != -1)
            //{
            //    var mcv = new MCvScalar(0, 0, 255);
            //    CVUtils.DrawPointAndCursor(l, r, selectedPoint, selectedPointIndex, mcv);
            //}

            if (_leftPoint.X > 0 && _leftPoint.Y > 0)
            {
                UiHelper.SetSmallImg(ImgLd, l, _leftPoint, ImgLd.Width, 2, 50);
                // CVUtils.DrawPointAndCursor(l, leftPoint, "new", new MCvScalar(0, 0, 255));
                ImgL.AddPoint = new Point(_leftPoint.X, _leftPoint.Y);
            }
            else
            {
                if (_selectedPoint != null && index != -1)
                {
                    System.Drawing.Point pl = new System.Drawing.Point((int)point.Lx, (int)point.Ly);
                    UiHelper.SetSmallImg(ImgLd, l, pl, ImgLd.Width, 2, 50);
                    ComboBoxLImg.SelectedItem = _zoomStringL;
                    UiHelper.MoveToPoint(ScrollViewerL, pl);
                }

            }


            if (_rightPoint.X > 0 && _rightPoint.Y > 0)
            {
                UiHelper.SetSmallImg(ImgRd, r, _rightPoint, ImgRd.Width, 2, 50);
                //CVUtils.DrawPointAndCursor(r, rightPoint, "new", new MCvScalar(0, 0, 255));
                ImgR.AddPoint = new Point(_rightPoint.X, _rightPoint.Y);
            }
            else
            {
                if (_selectedPoint != null && index != -1)
                {
                    System.Drawing.Point pr = new System.Drawing.Point((int)point.Rx, (int)point.Ry);
                    UiHelper.SetSmallImg(ImgRd, r, pr, ImgRd.Width, 2, 50);
                    ComboBoxRImg.SelectedItem = _zoomStringR;
                    UiHelper.MoveToPoint(ScrollViewerR, pr);
                }
            }


            ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
            ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);

            ImgL.Points = PointColletion.Select(o => new Point(o.Lx, o.Ly)).ToList();
            ImgR.Points = PointColletion.Select(o => new Point(o.Rx, o.Ry)).ToList();
            ImgL.AddPoint = _leftPoint;
            ImgR.AddPoint = _rightPoint;
        }

        #region 事件


        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_leftPoint.X > 0 && _leftPoint.Y > 0 && _rightPoint.X > 0 && _rightPoint.Y > 0)
            {
                FeaturePoint point = new FeaturePoint();
                point.Lx = _leftPoint.X;
                point.Ly = _leftPoint.Y;
                point.Rx = _rightPoint.X;
                point.Ry = _rightPoint.Y;
                point.Distance = 0;

                PointColletion.Add(point);
                PointColletion.UpdateIndex();
                ListViewPoints.SelectedItem = PointColletion.Last();
                _leftPoint = new Point();
                _rightPoint = new Point();
                BindPoints();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            //每次如果鼠标点击了图片意外的位置，如果此时在增加模式下把增加取消掉
            if ((!ImgL.IsMouseOver) && (!ImgR.IsMouseOver) && ((_leftPoint.X > 0 && _leftPoint.Y > 0) || (_rightPoint.X > 0 && _rightPoint.Y > 0)))
            {
                _leftPoint = new Point();
                _rightPoint = new Point();

                BindPoints();
            }

            base.OnMouseDown(e);
        }

        private void comboBoxL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = ComboBoxL.SelectedItem as Photo;
            if (item != null)
                ImgL.Source = item.Image;
        }

        private void comboBoxR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = ComboBoxR.SelectedItem as Photo;
            if (item != null)
                ImgR.Source = item.Image;
        }

        private void algsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            StichingParamsWindow wd = new StichingParamsWindow();
            wd.Show();
        }

        private void buttonFindFeatures_Click(object sender, RoutedEventArgs e)
        {
            string s = AlgsComboBox.SelectedItem as string;
            Debug.Assert(s != null);
            Stitcher stitcher = new Stitcher(false);
            FeaturesFinder finder = null;
            switch (s)
            {
                case "SURF":

                    break;
                case "Orb":
                    finder = new OrbFeaturesFinder(new System.Drawing.Size(3, 1));
                    break;
            }
            // stitcher.SetFeaturesFinder(finder);
            Image<Bgr, byte> l = new Image<Bgr, byte>((ComboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((ComboBoxR.SelectedItem as Photo).Source);

            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();

            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                long matchTime;
                CvUtils.FindMatch(l.Mat, r.Mat, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                  out mask, out homography);

                //double featurePointFinderThreshold =(double)Application.Current.Resources["FeaturePointFinderThreshold"];
                double featurePointFinderThreshold = 1.5;
                if (!double.TryParse(TextBoxThreshold.Text, out featurePointFinderThreshold))
                {
                    return;
                }

                double minRatio = 1 / featurePointFinderThreshold;

                List<MDMatch> matchList = new List<MDMatch>();

                for (int i = 0; i < matches.Size; i++)
                {
                    MDMatch bestMatch = matches[i][0];
                    MDMatch betterMatch = matches[i][1];

                    float distanceRatio = bestMatch.Distance / betterMatch.Distance;

                    if (distanceRatio < minRatio)
                        matchList.Add(bestMatch);
                }

                _matchers = matchList.ToArray();

                for (int i = 0; i < _matchers.Length; i++)
                {

                    MKeyPoint ll = modelKeyPoints[_matchers[i].TrainIdx];

                    MKeyPoint rr = observedKeyPoints[_matchers[i].QueryIdx];

                    FeaturePoint p = new FeaturePoint(i, ll, rr, _matchers[i].Distance);

                    PointColletion.Add(p);

                }
            }

            BindPoints();


        }

        private void comboBoxLImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScrollViewerL == null) return;

            ComboBox box = sender as ComboBox;
            _zoomStringL = box.SelectedItem as string;

            BindPoints();
        }

        private void comboBoxRImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ScrollViewerR == null) return;

            ComboBox box = sender as ComboBox;
            _zoomStringR = box.SelectedItem as string;

            BindPoints();
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (Image<Bgr, byte> l = new Image<Bgr, byte>((ComboBoxL.SelectedItem as Photo).Source))
                using (Image<Bgr, byte> r = new Image<Bgr, byte>((ComboBoxR.SelectedItem as Photo).Source))
                {
                    Image<Bgr, byte> result = CvUtils.Draw(l, r, PointColletion.ToList());
                    ImageBox box = new ImageBox();
                    box.Image = result.Bitmap;
                    box.Show();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            var point = ListViewPoints.SelectedItem as FeaturePoint;
            Debug.Assert(point != null);
            PointColletion.Remove(point);
            PointColletion.UpdateIndex();

            BindPoints();

        }

        private void listViewPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPoint = ((ListView)sender).SelectedItem as FeaturePoint;
            if (_selectedPoint == null) return;
            _selectedPointIndex = ((ListView)sender).SelectedIndex;

            _zoomStringL = "150%";
            _zoomStringR = "150%";

            ImgL.SelectedIndex = _selectedPointIndex;
            ImgR.SelectedIndex = _selectedPointIndex;
            UiHelper.MoveToPoint(ScrollViewerL, new System.Drawing.Point((int)_selectedPoint.Lx, (int)_selectedPoint.Ly));
            UiHelper.MoveToPoint(ScrollViewerR, new System.Drawing.Point((int)_selectedPoint.Rx, (int)_selectedPoint.Ry));

            BindPoints();
        }

        private void buttonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("你确定要全部删除吗", "提示", MessageBoxButton.OKCancel);
            if (r == MessageBoxResult.OK)
            {
                PointColletion.Clear();
                _selectedPoint = null;
                _selectedPointIndex = -1;
                BindPoints();
            }
        }

        private void imgL_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image v = sender as Image;
            var p = e.GetPosition(v);
            ImageSource imageSource = v.Source;
            double pixelMousePositionX = e.GetPosition(v).X * imageSource.Width / v.ActualWidth;
            double pixelMousePositionY = e.GetPosition(v).Y * imageSource.Height / v.ActualHeight;

            p = new Point(pixelMousePositionX, pixelMousePositionY);
            _leftPoint = p;

            BindPoints();

        }

        private void imgR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image v = sender as Image;
            var p = e.GetPosition(v);
            ImageSource imageSource = v.Source;
            double pixelMousePositionX = e.GetPosition(v).X * imageSource.Width / v.ActualWidth;
            double pixelMousePositionY = e.GetPosition(v).Y * imageSource.Height / v.ActualHeight;

            p = new Point(pixelMousePositionX, pixelMousePositionY);
            _rightPoint = p;

            BindPoints();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Point addPoint = new Point();
            MyImage img = null;
            ScrollViewer view = null;

            if (ImgL.IsMouseOver)
            {
                addPoint = _leftPoint;
                img = ImgL;
                view = ScrollViewerL;
            }
            if (ImgR.IsMouseOver)
            {
                addPoint = _rightPoint;
                img = ImgR;
                view = ScrollViewerR;
            }

            if (img == null || view == null) { base.OnKeyUp(e); return; }
            var source = img.Source;
            if (view.IsMouseOver)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        addPoint.Y--;
                        if (addPoint.Y < 0)
                            addPoint.Y = 0;
                        //view.LineUp();
                        break;
                    case Key.Down:
                        addPoint.Y++;
                        if (addPoint.Y > source.Height)
                            addPoint.Y = (int)source.Height;
                        //view.LineDown();
                        break;
                    case Key.Left:
                        addPoint.X--;
                        if (addPoint.X < 0)
                            addPoint.X = 0;
                        //view.LineLeft();
                        break;
                    case Key.Right:
                        addPoint.X++;
                        if (addPoint.X > source.Width)
                            addPoint.X = (int)source.Width;
                        //view.LineRight();
                        break;
                }
                img.AddPoint = addPoint;
                if (ImgL.IsMouseOver)
                {
                    _leftPoint = addPoint;
                }
                if (ImgR.IsMouseOver)
                {
                    _rightPoint = addPoint;
                }
                BindPoints();
            }
            // base.OnKeyUp(e);
        }

        #endregion
        //缝合
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.PointF[] pointsl = PointColletion.Select(o => new System.Drawing.PointF((float)o.Lx, (float)o.Ly)).ToArray();
            System.Drawing.PointF[] pointsr = PointColletion.Select(o => new System.Drawing.PointF((float)o.Rx, (float)o.Ry)).ToArray();


            Matrix<double> h = new Matrix<double>(3, 3);
            //求单应矩阵
            CvInvoke.FindHomography(pointsl, pointsr, h, HomographyMethod.Ransac);


            Image<Bgr, byte> l = new Image<Bgr, byte>((ComboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((ComboBoxR.SelectedItem as Photo).Source);

            int rows = 0;
            int cols = 0;
            int initX = 0;
            int initY = 0;
            int initSpanX = 1;
            int initSpanY = 1;
            //变换矩阵
            double[,] tData = { { 1.0, 0, 0 }, { 0, 1.0, 0 }, { 0, 0, 1.0 } };
            switch (ComboBoxOrientation.SelectedItem as string)
            {
                case "左->右":
                    rows = Math.Max(l.Rows, r.Rows);
                    cols = l.Cols + r.Cols;
                    tData[0, 2] = l.Cols;
                    initX = l.Cols;
                    initSpanX = l.Width/20;

                    break;
                case "右->左":
                    rows = Math.Max(l.Rows, r.Rows);
                    cols = l.Cols + r.Cols;
                    initSpanX = l.Width / 20;

                    break;
                case "上->下":
                    rows = l.Rows + r.Rows;
                    cols = Math.Max(l.Cols, r.Cols);
                    tData[1, 2] = l.Rows;
                    initY = l.Rows;
                    initSpanY = l.Rows/20;
                    
                    break;
                case "下->上":
                    rows = l.Rows + r.Rows;
                    cols = Math.Max(l.Cols, r.Cols);
                    initSpanY = l.Rows/20;

                    break;
            }


            System.Drawing.Size size = new System.Drawing.Size(cols, rows);

            Image<Bgr, byte> resultL = new Image<Bgr, byte>(size);

            Matrix<double> sh = new Matrix<double>(tData);
            //MessageBox.Show(CvUtils.MatrixToString(sh * h));
            Image<Bgr, byte> last = new Image<Bgr, byte>(size);

            try
            {
                CvInvoke.WarpPerspective(l, resultL, sh * h, size);
                CvUtils.CopyTo(resultL, last, a => Math.Abs(a.Blue) > 0 && Math.Abs(a.Green) > 0 && Math.Abs(a.Red) > 0);
            }
            catch (Exception ex)
            {
                // ignored
            }

            last.ROI = new Rectangle(initX, initY, r.Cols, r.Rows);
            r.CopyTo(last);
            last.ROI = Rectangle.Empty;

            ////寻找缝合线
            double[] Cimg1 = new double[20];
            double[] Cimg2 = new double[20];

            Image<Gray, byte> ll = new Image<Gray, byte>(l.Bitmap).ThresholdBinary(new Gray(100),new Gray(255));
            CvInvoke.MedianBlur(ll,ll,3);
            Image<Gray, byte> rr = new Image<Gray, byte>(r.Bitmap).ThresholdBinary(new Gray(100), new Gray(255));
            CvInvoke.MedianBlur(rr, rr, 3);


            for (int i = 0; i <= l.Width- initSpanX; i+=initSpanX)
            {
                int blackCount = 0;

                for (int j = 0; j < l.Height; j+=initSpanY)
                {
                    if (ll[j, i].Intensity == 255)
                    {
                        blackCount++;
                    }
                }
                Cimg1[i / initSpanX] = blackCount;

            }
            for (int i = 0; i <= r.Width - initSpanX; i += initSpanX)
            {
                int blackCount = 0;

                for (int j = 0; j <r.Height; j += initSpanY)
                {
                    if (rr[j, i].Intensity == 255)
                    {
                        blackCount++;
                    }
                }
                Cimg2[i / initSpanX] = blackCount;

            }
            int minIndex = 0;
            double min = double.MaxValue;

            for (int i = 0; i < 10;i++)
            {
                double[] c1ten = Cimg1.Skip(i).Take(10).ToArray();
                Console.WriteLine("c1ten:" + string.Join(",",c1ten));
                double[] c2ten=new double[10];
                for (int j = 0; j < 10; j++)
                {
                    c2ten = Cimg2.Skip(j).Take(10).ToArray();
                    Console.WriteLine("c2ten:"+ string.Join(",", c2ten));
                    double[] cten=c1ten.Zip(c2ten, (a, b) => Math.Abs(a - b)).ToArray();
                    Console.WriteLine("c ten:" + string.Join(",", cten));
                    double va = MMath.Variance(cten);
                    Console.WriteLine("va   :" + va);
                    if (va < min)
                    {
                        min = va;
                        minIndex = j;
                    }
                    Console.WriteLine("now j :" + minIndex);
                }
               
            }
            int aa = 1;
            Image<Bgr, byte> lll = l.Copy(new Rectangle(0, 0,r.Width-minIndex*initSpanX, l.Height));
            Image<Bgr, byte> rrr = r.Copy(new Rectangle(minIndex*initSpanX, 0,r.Width - minIndex * initSpanX, r.Height));

            //List<System.Drawing.Point> vSrcPtsl = new List<System.Drawing.Point>();
            //List<System.Drawing.Point> vSrcPtsr = new List<System.Drawing.Point>();
            //vSrcPtsl.Add(new System.Drawing.Point(0, 0));
            //vSrcPtsl.Add(new System.Drawing.Point(0, l.Rows));
            //vSrcPtsl.Add(new System.Drawing.Point(l.Cols, l.Rows));
            //vSrcPtsl.Add(new System.Drawing.Point(l.Cols, 0));

            //vSrcPtsr.Add(new System.Drawing.Point(0, 0));
            //vSrcPtsr.Add(new System.Drawing.Point(0, r.Rows));
            //vSrcPtsr.Add(new System.Drawing.Point(r.Cols, r.Rows));
            //vSrcPtsr.Add(new System.Drawing.Point(r.Cols, 0));

            ////计算图像2在图像1中对应坐标信息
            //List<System.Drawing.Point> vWarpPtsr = new List<System.Drawing.Point>();
            //for (int i = 0; i < vSrcPtsr.Count; i++)
            //{
            //    Matrix<double> srcMat = new Matrix<double>(3, 1);
            //    srcMat[0, 0] = vSrcPtsr[i].X;
            //    srcMat[1, 0] = vSrcPtsr[i].Y;
            //    srcMat[2, 0] = 1.0;

            //    Matrix<double> warpMat = h * srcMat;
            //    System.Drawing.Point warpPt = new System.Drawing.Point
            //    {
            //        X = (int)Math.Round(warpMat[0, 0] / warpMat[2, 0]),
            //        Y = (int)Math.Round(warpMat[1, 0] / warpMat[2, 0]),
            //    };

            //    vWarpPtsr.Add(warpPt);
            //}

            //var vPtsImg1 = pointsl.Select(System.Drawing.Point.Round).ToList();
            //var vPtsImg2 = pointsr.Select(System.Drawing.Point.Round).ToList();

            ////计算图像1和转换后的图像2的交点
            //if (!CvUtils.PolygonClip(vSrcPtsl, vWarpPtsr, vPtsImg1))
            //    return;

            //foreach (System.Drawing.Point t in vPtsImg1)
            //{
            //    Matrix<double> srcMat = new Matrix<double>(3, 1)
            //    {
            //        [0, 0] = t.X,
            //        [1, 0] = t.Y,
            //        [2, 0] = 1.0
            //    };
            //    Matrix<double> warpMat;
            //    using (Matrix<double> _h = new Matrix<double>(3, 3))
            //    {
            //        CvInvoke.Invert(h, _h, DecompMethod.LU);
            //        warpMat = _h*srcMat;
            //    }
            //    System.Drawing.Point warpPt = new System.Drawing.Point
            //    {
            //        X = (int)Math.Round(warpMat[0, 0] / warpMat[2, 0]),
            //        Y = (int)Math.Round(warpMat[1, 0] / warpMat[2, 0])
            //    };
            //    vPtsImg2.Add(warpPt);
            //}
            //last.DrawPolyline(vPtsImg1.ToArray(), false, new Bgr(255, 0, 0));
            //last.DrawPolyline(vPtsImg2.ToArray(), false, new Bgr(0, 255, 0));
            //l.DrawPolyline(vPtsImg1.ToArray(), false, new Bgr(255, 0, 0));
            //ImageBox box0 = new ImageBox { Image = l.Bitmap };
            //box0.Show();
            ImageBox box = new ImageBox { Image = resultL.Bitmap };
            box.Show();
            ImageBox box3 = new ImageBox { Image = last.Bitmap };
            box3.Show();
            ImageBox box4 = new ImageBox { Image = ll.Bitmap };
            box4.Show();
            ImageBox box5 = new ImageBox { Image = rr.Bitmap };
            box5.Show();
            ImageBox box6 = new ImageBox { Image = lll.Bitmap };
            box6.Show();
            ImageBox box7 = new ImageBox { Image = rrr.Bitmap };
            box7.Show();

        }

        private void buttonRe_Click(object sender, RoutedEventArgs e)
        {
            PointColletion.Clear();
            this.buttonFindFeatures_Click(sender, e);
        }

        private void listViewPoints_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    var point = ListViewPoints.SelectedItem as FeaturePoint;
                    Debug.Assert(point != null);
                    PointColletion.Remove(point);
                    PointColletion.UpdateIndex();

                    BindPoints();
                    break;
            }
        }

        private void buttonFilter_Click(object sender, RoutedEventArgs e)
        {
            //todo 过滤特征点
            //VectorOfDMatch
            //int pCount = PointColletion.Count;

            //Matrix<float> p1 = new Matrix<float>(pCount, 2);
            //Matrix<float> p2 = new Matrix<float>(pCount, 2);

            //for (int i = 0; i < pCount; i++)
            //{
            //    FeaturePoint point = PointColletion.GetByIndex(i);
            //    if (point == null) continue;
            //    p1[i, 0] = (float)point.Rx;
            //    p1[i, 1] = (float)point.Ry;
            //    p2[i, 0] = (float)point.Lx;
            //    p2[i, 1] = (float)point.Ly;
            //}

            //// Mat m_Fundamental = new Mat();
            //Matrix<float> mRansacStatus=new Matrix<float>(pCount,1);
            //try
            //{
            //    CvInvoke.FindFundamentalMat(p1, p2, mRansacStatus);

            //}
            //catch (Exception ex)
            //{
            //    // ignored
            //}

            //var outlinerCount = 0;
            //for (int i = 0; i < pCount; i++)
            //{
            //    if (mRansacStatus[i, 0] == 0)    // 状态为0表示野点  
            //    {
            //        outlinerCount++;
            //    }
            //}

            //int inlinerCount = pCount - outlinerCount;   // 计算内点  
            //Console.WriteLine(@"内点数为：" + inlinerCount);
        }

        private void ButtonAccommodate_OnClick(object sender, RoutedEventArgs e)
        {
            _zoomStringR = _zoomStringL = "自动适应";
            BindPoints();
        }
    }
}
