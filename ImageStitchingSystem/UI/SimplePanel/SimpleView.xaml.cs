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
                UiHelper.SetSmallImg(ImgLd, l, _leftPoint, ImgLd.Width);
                // CVUtils.DrawPointAndCursor(l, leftPoint, "new", new MCvScalar(0, 0, 255));
                ImgL.AddPoint = new Point(_leftPoint.X, _leftPoint.Y);
            }
            else
            {
                if (_selectedPoint != null && index != -1)
                {
                    System.Drawing.Point pl = new System.Drawing.Point((int)point.Lx, (int)point.Ly);
                    UiHelper.SetSmallImg(ImgLd, l, pl, ImgLd.Width);
                    ComboBoxLImg.SelectedItem = _zoomStringL;
                    UiHelper.MoveToPoint(ScrollViewerL, pl);
                }

            }


            if (_rightPoint.X > 0 && _rightPoint.Y > 0)
            {
                UiHelper.SetSmallImg(ImgRd, r, _rightPoint, ImgRd.Width);
                //CVUtils.DrawPointAndCursor(r, rightPoint, "new", new MCvScalar(0, 0, 255));
                ImgR.AddPoint = new Point(_rightPoint.X, _rightPoint.Y);
            }
            else
            {
                if (_selectedPoint != null && index != -1)
                {
                    System.Drawing.Point pr = new System.Drawing.Point((int)point.Rx, (int)point.Ry);
                    UiHelper.SetSmallImg(ImgRd, r, pr, ImgRd.Width);
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


            Matrix<double> H = new Matrix<double>(3, 3);
            //Mat H = new Mat(3, 3, DepthType.Cv32F, 1);
            Mat mask = new Mat();
            CvInvoke.FindHomography(pointsl, pointsr, H, HomographyMethod.Ransac);


            Image<Bgr, byte> l = new Image<Bgr, byte>((ComboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((ComboBoxR.SelectedItem as Photo).Source);

            //Matrix<float> objCorners = new Matrix<float>(4, 2)
            //{
            //    [0, 0] = 0,
            //    [0, 1] = 0,
            //    [1, 0] = l.Cols,
            //    [1, 1] = 0,
            //    [2, 0] = l.Cols,
            //    [2, 1] = l.Rows,
            //    [3, 0] = 0,
            //    [3, 1] = l.Rows
            //};
            //Matrix<float> sceneCorners=new Matrix<float>(4,2);
            int rows = Math.Max(l.Rows, r.Rows);

            System.Drawing.Size size = new System.Drawing.Size(l.Cols + r.Cols, rows);

            Image<Bgr, byte> result = new Image<Bgr, byte>(size);
            //Mat result = new Mat(r.Size,DepthType.Cv32F, 3);
            double[,] data = { { 1.0, 0, l.Cols }, { 0, 1.0, 0 }, { 0, 0, 1.0 } };
            Matrix<double> sh = new Matrix<double>(data);

            try
            {
                //CvInvoke.PerspectiveTransform(l.Mat, r.Mat, H);
                //CvInvoke.ProjectPoints()

                //result=l.WarpPerspective(sh*H,size.Width,size.Height, Inter.Linear, Warp.InverseMap, BorderType.Default, new Bgr());
                CvInvoke.WarpPerspective(l, result, H, size);
                //result.ROI = new System.Drawing.Rectangle(l.Cols, 0, r.Cols, r.Rows);
                //r.CopyTo(result);
                //result.ROI = Rectangle.Empty;

            }
            catch (Exception ex)
            {
                // ignored
            }

            //CvInvoke.Line(r,new System.Drawing.Point((int)sceneCorners[0,0]+l.Cols,(int)sceneCorners[0,1]), new System.Drawing.Point((int)sceneCorners[1, 0] + l.Cols, (int)sceneCorners[1, 1]),new MCvScalar(0,255,0),4);
            //CvInvoke.Line(r, new System.Drawing.Point((int)sceneCorners[1, 0] + l.Cols, (int)sceneCorners[1, 1]), new System.Drawing.Point((int)sceneCorners[2, 0] + l.Cols, (int)sceneCorners[2, 1]), new MCvScalar(0, 255, 0), 4);
            //CvInvoke.Line(r, new System.Drawing.Point((int)sceneCorners[2, 0] + l.Cols, (int)sceneCorners[2, 1]), new System.Drawing.Point((int)sceneCorners[3, 0] + l.Cols, (int)sceneCorners[3, 1]), new MCvScalar(0, 255, 0), 4);
            //CvInvoke.Line(r, new System.Drawing.Point((int)sceneCorners[3, 0] + l.Cols, (int)sceneCorners[3, 1]), new System.Drawing.Point((int)sceneCorners[0, 0] + l.Cols, (int)sceneCorners[0, 1]), new MCvScalar(0, 255, 0), 4);

            //PhotoEditWindow window = new PhotoEditWindow {Bitmap = result.Bitmap};
            //window.Show();
            
            Image<Bgr, byte> last = new Image<Bgr, byte>(size);
            try
            {
                CvUtils.CopyTo(result,last,a=> a.Blue != 0D && a.Green != 0D && a.Red != 0D);
            }
            catch (Exception ex)
            {

            }
            last.ROI = new Rectangle(0, 0, r.Cols, r.Rows);
            r.CopyTo(last);
            last.ROI = Rectangle.Empty;
            
            //last += result;
            //last += r.Resize(size.Width, size.Height, Inter.Linear);
            ImageBox box = new ImageBox { Image = result.Bitmap };
            box.Show();
            ImageBox box3 = new ImageBox { Image = last.Bitmap };
            box3.Show();
            //Mat result = new Mat();
            //Mat result1 = new Mat();
            //try
            //{
            //    CvInvoke.WarpPerspective(l, result, H, new System.Drawing.Size(l.Width * 2, l.Height * 2));
            //    CvInvoke.WarpPerspective(r, result1, H, new System.Drawing.Size(r.Width * 2, r.Height * 2));
            //}
            //catch (Exception ex)
            //{
            //    // ignored
            //}
            ////result.Save("l.jpg");
            ////result1.Save("r.jpg");
            //long time;
            //Mat rrr = CvUtils.Draw(l.Mat, r.Mat, out time);

            //ImageBox box = new ImageBox { Image = rrr.Bitmap };
            //box.Show();

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

    }
}
