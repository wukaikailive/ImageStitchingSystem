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
using MMath = ImageStitchingSystem.Utils.MMath;

using ImageStitchingSystem.UI.Weight;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;
using static System.Math;

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

        private bool _isShowOtherPoint = true;

        private bool isLChange = false;
        private bool isRChange = false;

        private string lSaveName = Guid.NewGuid() + ".jpg";
        private string rSaveName = Guid.NewGuid() + ".jpg";

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
            try
            {
                Binding bind = new Binding { Source = PointColletion };
                //todo the below ex is not used
                //PointColletion.OrderBy(o => o.Lx).ThenBy(o => o.Ly);
                ListViewPoints.SetBinding(ItemsControl.ItemsSourceProperty, bind);

                Image<Bgr, byte> l = new Image<Bgr, byte>(isLChange ? (ComboBoxL.SelectedItem as Photo).Source : lSaveName);
                Image<Bgr, byte> r = new Image<Bgr, byte>(isRChange ? (ComboBoxR.SelectedItem as Photo).Source : rSaveName);
                int i = -1;
                Random romdom = new Random();

                var point = _selectedPoint;
                var index = _selectedPointIndex;

                UiHelper.ZoomImage(ImgL, ScrollViewerL, _zoomStringL);
                UiHelper.ZoomImage(ImgR, ScrollViewerR, _zoomStringR);

                ImgL.IsShowOtherPoint = _isShowOtherPoint;
                ImgR.IsShowOtherPoint = _isShowOtherPoint;

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
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                MessageBox.Show("程序运行错误,请尝试提高阈值：" + e.Message);
            }

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
            isLChange = true;
        }

        private void comboBoxR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = ComboBoxR.SelectedItem as Photo;
            if (item != null)
                ImgR.Source = item.Image;
            isRChange = true;
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
            switch (s)
            {
                case "SURF":

                    break;
                case "Orb":
                    break;
            }
            // stitcher.SetFeaturesFinder(finder);

            Image<Bgr, byte> l = new Image<Bgr, byte>(isLChange ? (ComboBoxL.SelectedItem as Photo).Source : lSaveName);
            Image<Bgr, byte> r = new Image<Bgr, byte>(isRChange ? (ComboBoxR.SelectedItem as Photo).Source : rSaveName);

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
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                for (int i = 0; i < _matchers.Length; i++)
                {

                    MKeyPoint ll = modelKeyPoints[_matchers[i].TrainIdx];

                    MKeyPoint rr = observedKeyPoints[_matchers[i].QueryIdx];

                    FeaturePoint p = new FeaturePoint(i, ll, rr, _matchers[i].Distance);

                    Bgr la = l[(int)p.Ly, (int)p.Lx];
                    Bgr ra = r[(int)p.Ry, (int)p.Rx];
                    if (la.IsBlack() || ra.IsBlack() || la.IsWhite() || ra.IsWhite())
                        continue;

                    PointColletion.Add(p);

                }
                // stopwatch.Stop();
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
                using (Image<Bgr, byte> l = new Image<Bgr, byte>(isLChange ? (ComboBoxL.SelectedItem as Photo).Source : lSaveName))
                using (Image<Bgr, byte> r = new Image<Bgr, byte>(isRChange ? (ComboBoxR.SelectedItem as Photo).Source : rSaveName))
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

        System.Drawing.Point _leftTop;
        System.Drawing.Point _leftBottom;
        System.Drawing.Point _rightTop;
        System.Drawing.Point _rightBottom;


        //计算变换图像四个角的坐标
        //计算图2的四个角经矩阵H变换后的坐标
        private void CalcFourCorner(Matrix<double> H, Image<Bgr, byte> img2)
        {
            //计算图2的四个角经矩阵H变换后的坐标
            double[,] v2 = new double[3, 1] { { 0 }, { 0 }, { 1 } };//左上角
            double[,] v1 = new double[3, 1];//变换后的坐标值
            Matrix<double> V2 = new Matrix<double>(3, 1) { Data = v2 };
            Matrix<double> V1 = new Matrix<double>(3, 1) { Data = v1 };
            CvInvoke.Gemm(H, V2, 1, null, 1, V1);
            _leftTop.X = (int)Math.Round(v1[0, 0] / v1[2, 0]);
            _leftTop.Y = (int)Math.Round(v1[1, 0] / v1[2, 0]);
            //cvCircle(xformed,leftTop,7,CV_RGB(255,0,0),2);

            //将v2中数据设为左下角坐标
            v2[0, 0] = 0;
            v2[1, 0] = img2.Height;
            V2 = new Matrix<double>(3, 1) { Data = v2 };
            V1 = new Matrix<double>(3, 1) { Data = v1 };
            CvInvoke.Gemm(H, V2, 1, null, 1, V1);
            _leftBottom.X = (int)Math.Round(v1[0, 0] / v1[2, 0]);
            _leftBottom.Y = (int)Math.Round(v1[1, 0] / v1[2, 0]);
            //cvCircle(xformed,leftBottom,7,CV_RGB(255,0,0),2);

            //将v2中数据设为右上角坐标
            v2[0, 0] = img2.Width;
            v2[1, 0] = 0;
            V2 = new Matrix<double>(3, 1) { Data = v2 };
            V1 = new Matrix<double>(3, 1) { Data = v1 };
            CvInvoke.Gemm(H, V2, 1, null, 1, V1);
            _rightTop.X = (int)Math.Round(v1[0, 0] / v1[2, 0]);
            _rightTop.Y = (int)Math.Round(v1[1, 0] / v1[2, 0]);
            //cvCircle(xformed,rightTop,7,CV_RGB(255,0,0),2);

            //将v2中数据设为右下角坐标
            v2[0, 0] = img2.Width;
            v2[1, 0] = img2.Height;
            V2 = new Matrix<double>(3, 1) { Data = v2 };
            V1 = new Matrix<double>(3, 1) { Data = v1 };
            CvInvoke.Gemm(H, V2, 1, null, 1, V1);
            _rightBottom.X = (int)Math.Round(v1[0, 0] / v1[2, 0]);
            _rightBottom.Y = (int)Math.Round(v1[1, 0] / v1[2, 0]);
            //cvCircle(xformed,rightBottom,7,CV_RGB(255,0,0),2);

        }


        //缝合
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Drawing.PointF[] pointsl =
                    PointColletion.Select(o => new System.Drawing.PointF((float)o.Lx, (float)o.Ly)).ToArray();
                System.Drawing.PointF[] pointsr =
                    PointColletion.Select(o => new System.Drawing.PointF((float)o.Rx, (float)o.Ry)).ToArray();

                if (pointsl.Length < 4)
                {
                    MessageBox.Show("特征点小于4对，不能匹配");
                    return;
                }

                Matrix<double> h = new Matrix<double>(3, 3);

                Image<Bgr, byte> l = new Image<Bgr, byte>(isLChange ? (ComboBoxL.SelectedItem as Photo)?.Source : lSaveName);
                Image<Bgr, byte> r = new Image<Bgr, byte>(isRChange ? (ComboBoxR.SelectedItem as Photo)?.Source : rSaveName);

                int rows = 0;
                int cols = 0;
                int start = 0;
                int processWidthOrHeight = 0; //重叠区域的宽度或高度

                //变换矩阵
                double[,] tData = { { 1.0, 0, 0 }, { 0, 1.0, 0 }, { 0, 0, 1.0 } };

                bool horizontal = true;
                HomographyMethod method = (HomographyMethod) Enum.Parse(typeof(HomographyMethod),
                    ComboBoxStitcheArithmetic.SelectedItem as string);
                //switch (ComboBoxStitcheArithmetic.SelectedItem as string)
                //{
                //    case "Ransac":
                //        method = HomographyMethod.Ransac;
                //        break;
                //    case "LMEDS":
                //        method = HomographyMethod.LMEDS;
                //        break;
                //    case "Default":
                //        method = HomographyMethod.Default;
                //        break;
                //}

                switch (ComboBoxOrientation.SelectedItem as string)
                {
                    case "左->右":
                        rows = Math.Max(l.Rows, r.Rows);
                        cols = l.Cols + r.Cols;
                        //求单应矩阵
                        CvInvoke.FindHomography(pointsr, pointsl, h, method);
                        CalcFourCorner(h, r);

                        start = Math.Min(_leftTop.X, _leftBottom.X); //开始位置，即重叠区域的左边界
                        processWidthOrHeight = l.Width - start;
                        break;
                    case "右->左":
                        var t = l; //交换图片
                        l = r;
                        r = t;
                        rows = Math.Max(l.Rows, r.Rows);
                        cols = l.Cols + r.Cols;
                        CvInvoke.FindHomography(pointsl, pointsr, h, method);
                        CalcFourCorner(h, r);

                        start = Math.Min(_leftTop.X, _leftBottom.X); //开始位置，即重叠区域的左边界
                        processWidthOrHeight = l.Width - start;
                        break;
                    case "上->下":
                        horizontal = false;
                        rows = l.Rows + r.Rows;
                        cols = Math.Max(l.Cols, r.Cols);

                        CvInvoke.FindHomography(pointsr, pointsl, h, method);
                        CalcFourCorner(h, r);

                        start = Math.Min(_leftTop.Y, _leftBottom.Y); //开始位置，即重叠区域的上边界
                        processWidthOrHeight = l.Height - start;
                        break;
                    case "下->上":
                        horizontal = false;
                        var t1 = l;
                        l = r;
                        r = t1;
                        rows = l.Rows + r.Rows;
                        cols = Math.Max(l.Cols, r.Cols);
                        CvInvoke.FindHomography(pointsl, pointsr, h, method);

                        CalcFourCorner(h, r);

                        start = Math.Min(_leftTop.Y, _leftBottom.Y); //开始位置，即重叠区域的上边界
                        processWidthOrHeight = l.Height - start;
                        break;
                }

                //MessageBox.Show(CvUtils.MatrixToString(h));

                Matrix<double> sh = new Matrix<double>(tData);

                System.Drawing.Size size = new System.Drawing.Size(cols, rows);

                Image<Bgr, byte> result = new Image<Bgr, byte>(size);

                Image<Bgr, byte> last = new Image<Bgr, byte>(size);

                BorderType borderType = (BorderType) Enum.Parse(typeof(BorderType), ComboBoxBorderType.SelectedItem as string);
                //switch (ComboBoxBorderType.SelectedItem as string)
                //{
                //    case "NegativeOne":
                        
                //        break;
                //    case "Constant":
                //        break;
                //    case "Replicate":
                //        break;
                //    case "Reflect":
                //        break;
                //    case "Wrap":
                //        break;
                //    case "Default":
                //        break;
                //    case "Reflect101":
                //        break;
                //    case "Transparent":
                //        break;
                //    case "Isolated":
                //        break;
                //}

                CvInvoke.WarpPerspective(r, result, sh * h, size,Inter.Linear,Warp.Default,borderType);

                if (CheckBoxIsShowWarpResult.IsChecked != null && CheckBoxIsShowWarpResult.IsChecked.Value)
                {
                    ImageBox box0 = new ImageBox { Image = result.Bitmap };
                    box0.Show();
                }


                last = result.Clone();

                if (CheckBoxWeightedAverage.IsChecked != null && CheckBoxWeightedAverage.IsChecked.Value)
                {
                    #region 重叠区域加权平均

                    if (horizontal)
                    {
                        l.ROI = new Rectangle(new System.Drawing.Point(0, 0),
                            new System.Drawing.Size(Math.Min(_leftTop.X, _leftBottom.X), l.Height));
                        last.ROI = new Rectangle(new System.Drawing.Point(0, 0),
                            new System.Drawing.Size(Math.Min(_leftTop.X, _leftBottom.X), l.Height));

                    }
                    else
                    {
                        l.ROI = new Rectangle(new System.Drawing.Point(0, 0),
                            new System.Drawing.Size(l.Width, Math.Min(_leftTop.Y, _leftBottom.Y)));
                        last.ROI = new Rectangle(new System.Drawing.Point(0, 0),
                            new System.Drawing.Size(l.Width, Math.Min(_leftTop.Y, _leftBottom.Y)));
                    }

                    l.CopyTo(last); //先用左图片填充重叠区域左边或上边

                    l.ROI = Rectangle.Empty;
                    last.ROI = Rectangle.Empty;

                    ImageBox box00 = new ImageBox { Image = last.Bitmap };
                    box00.Show();

                    double alpha; //左图中像素的权重
                    if (horizontal)
                    {
                        for (int i = 0; i < last.Height; i++)
                        {
                            for (int j = start; j < l.Width; j++)
                            {
                                if (last[i, j].IsBlack())
                                {
                                    alpha = 1;
                                }
                                else
                                {
                                    alpha = (processWidthOrHeight - (j - start)) * 1.0 / processWidthOrHeight;
                                }
                                Bgr b = l[i, j];
                                Bgr c = result[i, j];
                                double blue = b.Blue * alpha + c.Blue * (1 - alpha);
                                double green = b.Green * alpha + c.Green * (1 - alpha);
                                double red = b.Red * alpha + c.Red * (1 - alpha);
                                last[i, j] = new Bgr(blue, green, red);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < last.Width; i++)
                        {
                            for (int j = start; j < l.Height; j++)
                            {
                                //如果是黑色像素 则全由左图填充
                                if (last[j, i].IsBlack())
                                {
                                    alpha = 1;
                                }
                                else
                                {
                                    alpha = (processWidthOrHeight - (j - start)) * 1.0 / processWidthOrHeight;
                                }
                                Bgr b = l[j, i];
                                Bgr c = result[j, i];
                                double blue = b.Blue * alpha + c.Blue * (1 - alpha);
                                double green = b.Green * alpha + c.Green * (1 - alpha);
                                double red = b.Red * alpha + c.Red * (1 - alpha);
                                last[j, i] = new Bgr(blue, green, red);
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    last.ROI = new Rectangle(new System.Drawing.Point(0, 0),
                        new System.Drawing.Size(l.Width, l.Height));
                    l.CopyTo(last);
                    last.ROI=Rectangle.Empty;

                    for (int i = 0; i < result.Rows; i++)
                    {
                        for (int j = 0; j < result.Cols; j++)
                        {
                            if (!result[i, j].IsBlack())
                            {
                                last[i,j] = result[i, j];
                            }
                        }
                    }
                }

                //if (horizontal)
                //{
                //    last.ROI = new Rectangle(l.Width - 10, 0, l.Width + 10, last.Height);
                //    last.SmoothMedian(5);
                //    last.ROI=Rectangle.Empty;
                //}
                //else
                //{
                //    last.ROI = new Rectangle(0, l.Height - 10, last.Width, l.Height + 10);
                //    last.SmoothMedian(5);
                //    last.ROI = Rectangle.Empty;
                //}

                #region 裁剪
                if (CheckBoxAutoResize.IsChecked != null && CheckBoxAutoResize.IsChecked.Value)
                    try
                    {
                        if (horizontal)
                        {
                            int w = Math.Max(_rightTop.X, _rightBottom.X);
                            last.ROI = new Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(w, last.Height));
                        }
                        else
                        {
                            int hi = Math.Max(_leftBottom.Y, _rightBottom.Y);
                            last.ROI = new Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(last.Width, hi));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        MessageBox.Show("裁剪时遇到一点问题..." + ex.Message);
                    }
                #endregion

                if (!TextBoxFileSaveName.Text.Trim().Equals(""))
                {
                    last.Save(TextBoxFileSaveName.Text.Trim() + ".jpg");
                }
                else
                {
                    ImageBox box3 = new ImageBox { Image = last.Bitmap };
                    box3.Show();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                MessageBox.Show("操作错误:" + ex.Message);
            }

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

            //int pCount = PointColletion.Count;


            //PointF[] ppp1 = new PointF[pCount];
            //PointF[] ppp2 = new PointF[pCount];

            //Matrix<float> p1 = new Matrix<float>(2, pCount, 1);
            //Matrix<float> p2 = new Matrix<float>(2, pCount, 1);

            //for (int i = 0; i < pCount; i++)
            //{
            //    FeaturePoint point = PointColletion.GetByIndex(i);
            //    if (point == null) continue;
            //    //p1[0, i] = (float)point.Rx;
            //    //p1[1, i] = (float)point.Ry;
            //    //p2[0, i] = (float)point.Lx;
            //    //p2[1, i] = (float)point.Ly;

            //    ppp1[i] = point.TrainPoint.Point;
            //    ppp2[i] = point.QueryPoint.Point;
            //}

            //VectorOfPointF pp1 = new VectorOfPointF(ppp1);
            //VectorOfPointF pp2 = new VectorOfPointF(ppp2);
            //// Mat m_Fundamental = new Mat();
            //Matrix<float> f = new Matrix<float>(1, pCount, 1);

            //try
            //{
            //    CvInvoke.FindFundamentalMat(pp1, pp2, f, FmType.Ransac, 1);


            //}
            //catch (Exception ex)
            //{
            //    // ignored
            //}

            //var outlinerCount = 0;
            //for (int i = 0; i < pCount; i++)
            //{
            //    if (f[0, i] == 0)    // 状态为0表示野点  
            //    {
            //        outlinerCount++;
            //    }
            //}

            //int inlinerCount = pCount - outlinerCount;   // 计算内点  
            //Console.WriteLine(@"内点数为：" + inlinerCount);
            MessageBox.Show("不支持的功能");
        }

        private void ButtonAccommodate_OnClick(object sender, RoutedEventArgs e)
        {
            _zoomStringR = _zoomStringL = "自动适应";
            BindPoints();
        }

        private void CheckBoxIsClosePoint_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.IsChecked != null) _isShowOtherPoint = !cb.IsChecked.Value;
            BindPoints();
        }

        private void CheckBoxIsClosePoint_Unchecked(object sender, RoutedEventArgs e)
        {
            this.CheckBoxIsClosePoint_Checked(sender, e);
        }

        private void ComboBoxProjectionL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxL == null) return;
            Image<Bgr, byte> l = new Image<Bgr, byte>((ComboBoxL.SelectedItem as Photo).Source);

            string str = (sender as ComboBox).SelectedItem as string;
            switch (str)
            {
                case "默认":
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    isLChange = true;
                    break;
                case "平面->柱面":
                    l = l.Plane2CyLinder(l.Width);
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    l.Save(lSaveName);
                    isLChange = false;
                    break;
                case "柱面->平面":
                    l = l.CyLinder2Plane(l.Width);
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    l.Save(lSaveName);
                    isLChange = false;
                    break;
                case "平面->球面":
                    l = l.Plane2Sphere(l.Width);
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    l.Save(lSaveName);
                    isLChange = false;
                    break;
                case "球面->平面":
                    l = l.Sphere2Plane(l.Width);
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    l.Save(lSaveName);
                    isLChange = false;
                    break;
                case "鱼眼":
                    l = l.FishEye(l.Width);
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    l.Save(lSaveName);
                    isLChange = false;
                    break;
                case "鱼眼修正":
                    l = l.FishEyeRectify(l.Width);
                    ImgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
                    l.Save(lSaveName);
                    isLChange = false;
                    break;

            }
        }

        private void ComboBoxProjectionR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxR == null) return;
            Image<Bgr, byte> r = new Image<Bgr, byte>((ComboBoxR.SelectedItem as Photo)?.Source);
            string str = (sender as ComboBox)?.SelectedItem as string;
            switch (str)
            {
                case "默认":
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    isRChange = true;
                    break;
                case "平面->柱面":
                    r = r.Plane2CyLinder(r.Width);
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    r.Save(rSaveName);
                    isRChange = false;
                    break;
                case "柱面->平面":
                    r = r.CyLinder2Plane(r.Width);
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    r.Save(rSaveName);
                    isRChange = false;
                    break;
                case "平面->球面":
                    r = r.Plane2Sphere(r.Width);
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    r.Save(rSaveName);
                    isRChange = false;
                    break;
                case "球面->平面":
                    r = r.Sphere2Plane(r.Width);
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    r.Save(rSaveName);
                    isRChange = false;
                    break;
                case "鱼眼":
                    r = r.FishEye(r.Width);
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    r.Save(rSaveName);
                    isRChange = false;
                    break;
                case "鱼眼修正":
                    r = r.FishEyeRectify(r.Width);
                    ImgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
                    r.Save(rSaveName);
                    isRChange = false;
                    break;
            }

        }
    }
}
