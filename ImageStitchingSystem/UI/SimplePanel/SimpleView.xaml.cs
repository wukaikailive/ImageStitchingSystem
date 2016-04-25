using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ImageStitchingSystem.UI
{
    /// <summary>
    /// SimpleView.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleView : UserControl
    {

        MDMatch[] matchers;

        #region 动态资源

        public FeaturePointCollection pointColletion;

        private FeaturePoint selectedPoint;
        private int selectedPointIndex = 0;

        private string zoomStringL = "100%";
        private string zoomStringR = "100%";


        private Point leftPoint = new Point();
        private Point rightPoint = new Point();

        private bool isInLeft = false;
        private bool isInRight = true;

        #endregion


        public SimpleView()
        {
            InitializeComponent();
            pointColletion = (FeaturePointCollection)(Application.Current.Resources["featurePointCollection"] as ObjectDataProvider).Data;
        }



        /// <summary>
        /// 渲染
        /// </summary>
        private void bindPoints()
        {
            Binding bind = new Binding();
            bind.Source = pointColletion;
            pointColletion.OrderBy(o => o.LX).ThenBy(o => o.LY);
            listViewPoints.SetBinding(ListView.ItemsSourceProperty, bind);

            Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Source);
            int i = -1;
            Random romdom = new Random();

            var point = selectedPoint;
            var index = selectedPointIndex;

            UIHelper.ZoomImage(imgL, scrollViewerL, zoomStringL);
            UIHelper.ZoomImage(imgR, scrollViewerR, zoomStringR);



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

            if (leftPoint.X > 0 && leftPoint.Y > 0)
            {
                UIHelper.SetSmallImg(imgLD, l, leftPoint, imgLD.Width);
                // CVUtils.DrawPointAndCursor(l, leftPoint, "new", new MCvScalar(0, 0, 255));
                imgL.AddPoint = new Point(leftPoint.X, leftPoint.Y);
            }
            else
            {
                if (selectedPoint != null && index != -1)
                {
                    System.Drawing.Point pl = new System.Drawing.Point((int)point.LX, (int)point.LY);
                    UIHelper.SetSmallImg(imgLD, l, pl, imgLD.Width);
                    comboBoxLImg.SelectedItem = zoomStringL;
                    UIHelper.MoveToPoint(scrollViewerL, pl);
                }

            }


            if (rightPoint.X > 0 && rightPoint.Y > 0)
            {
                UIHelper.SetSmallImg(imgRD, r, rightPoint, imgRD.Width);
                //CVUtils.DrawPointAndCursor(r, rightPoint, "new", new MCvScalar(0, 0, 255));
                imgR.AddPoint = new Point(rightPoint.X, rightPoint.Y);
            }
            else
            {
                if (selectedPoint != null && index != -1)
                {
                    System.Drawing.Point pr = new System.Drawing.Point((int)point.RX, (int)point.RY);
                    UIHelper.SetSmallImg(imgRD, r, pr, imgRD.Width);
                    comboBoxRImg.SelectedItem = zoomStringR;
                    UIHelper.MoveToPoint(scrollViewerR, pr);
                }
            }


            imgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
            imgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);

            imgL.Points = pointColletion.Select(o => new Point(o.LX, o.LY)).ToList();
            imgR.Points = pointColletion.Select(o => new Point(o.RX, o.RY)).ToList();
        }

        #region 事件



        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!imgL.IsMouseOver && !imgR.IsMouseOver && ((leftPoint.X > 0 && leftPoint.Y > 0) || (rightPoint.X > 0 && rightPoint.Y > 0)))
            {
                leftPoint = new Point();
                rightPoint = new Point();
                bindPoints();
            }

            base.OnMouseDown(e);
        }

        private void comboBoxL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = comboBoxL.SelectedItem as Photo;
            if (item != null)
                imgL.Source = item.Image;
        }

        private void comboBoxR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = comboBoxR.SelectedItem as Photo;
            if (item != null)
                imgR.Source = item.Image;
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
            String s = algsComboBox.SelectedItem as String;
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
            Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Source);

            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();

            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                long matchTime;
                CVUtils.FindMatch(l.Mat, r.Mat, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                  out mask, out homography);

                //double featurePointFinderThreshold =(double)Application.Current.Resources["FeaturePointFinderThreshold"];
                double featurePointFinderThreshold = 1.5;
                if (!double.TryParse(textBoxThreshold.Text, out featurePointFinderThreshold))
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

                matchers = matchList.ToArray();

                for (int i = 0; i < matchers.Length; i++)
                {

                    MKeyPoint ll = modelKeyPoints[matchers[i].TrainIdx];

                    MKeyPoint rr = observedKeyPoints[matchers[i].QueryIdx];

                    FeaturePoint p = new FeaturePoint(i, ll, rr, matchers[i].Distance);

                    pointColletion.Add(p);

                }
            }

            bindPoints();


        }

        private void comboBoxLImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (scrollViewerL == null) return;

            ComboBox box = sender as ComboBox;
            zoomStringL = box.SelectedItem as string;

            bindPoints();
        }

        private void comboBoxRImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (scrollViewerR == null) return;

            ComboBox box = sender as ComboBox;
            zoomStringR = box.SelectedItem as string;

            bindPoints();
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Source))
                using (Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Source))
                {
                    Image<Bgr, byte> result = CVUtils.Draw(l, r, pointColletion.ToList());
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
            var point = listViewPoints.SelectedItem as FeaturePoint;
            Debug.Assert(point != null);
            pointColletion.Remove(point);
            pointColletion.UpdateIndex();

            bindPoints();

        }

        private void listViewPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPoint = (sender as ListView).SelectedItem as FeaturePoint;
            if (selectedPoint == null) return;
            selectedPointIndex = (sender as ListView).SelectedIndex;

            zoomStringL = "150%";
            zoomStringR = "150%";

            imgL.SelectedIndex = selectedPointIndex;
            imgR.SelectedIndex = selectedPointIndex;
            UIHelper.MoveToPoint(scrollViewerL, new System.Drawing.Point((int)selectedPoint.LX, (int)selectedPoint.LY));
            UIHelper.MoveToPoint(scrollViewerR, new System.Drawing.Point((int)selectedPoint.RX, (int)selectedPoint.RY));

            bindPoints();
        }

        private void buttonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("你确定要全部删除吗", "提示", MessageBoxButton.OKCancel);
            if (r == MessageBoxResult.OK)
            {
                pointColletion.Clear();
                selectedPoint = null;
                selectedPointIndex = -1;
                bindPoints();
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
            leftPoint = p;

            bindPoints();

        }

        private void imgR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image v = sender as Image;
            var p = e.GetPosition(v);
            ImageSource imageSource = v.Source;
            double pixelMousePositionX = e.GetPosition(v).X * imageSource.Width / v.ActualWidth;
            double pixelMousePositionY = e.GetPosition(v).Y * imageSource.Height / v.ActualHeight;

            p = new Point(pixelMousePositionX, pixelMousePositionY);
            rightPoint = p;

            bindPoints();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Point _addPoint = new Point();
            MyImage img = null;
            ScrollViewer view = null;
            ImageSource Source;

            if (imgL.IsMouseOver)
            {
                _addPoint = leftPoint;
                img = imgL;
                view = scrollViewerL;
            }
            if (imgR.IsMouseOver)
            {
                _addPoint = rightPoint;
                img = imgR;
                view = scrollViewerR;
            }

            if (img == null || view == null) { base.OnKeyUp(e); return; }
            Source = img.Source;
            if (view.IsMouseOver && _addPoint != null)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        _addPoint.Y--;
                        if (_addPoint.Y < 0)
                            _addPoint.Y = 0;
                        view.LineUp();
                        break;
                    case Key.Down:
                        _addPoint.Y++;
                        if (_addPoint.Y > Source.Height)
                            _addPoint.Y = (int)Source.Height;
                        view.LineDown();
                        break;
                    case Key.Left:
                        _addPoint.X--;
                        if (_addPoint.X < 0)
                            _addPoint.X = 0;
                        view.LineLeft();
                        break;
                    case Key.Right:
                        _addPoint.X++;
                        if (_addPoint.X > Source.Width)
                            _addPoint.X = (int)Source.Width;
                        view.LineRight();
                        break;
                }
                img.AddPoint = _addPoint;
                if (imgL.IsMouseOver)
                {
                    leftPoint = _addPoint;
                }
                if (imgR.IsMouseOver)
                {
                    rightPoint = _addPoint;
                }
                bindPoints();
            }
           // base.OnKeyUp(e);
        }


        #endregion

  
    }
}
