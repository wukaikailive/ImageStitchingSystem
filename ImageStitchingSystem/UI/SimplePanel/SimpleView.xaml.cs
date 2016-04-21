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

namespace ImageStitchingSystem.UI
{
    /// <summary>
    /// SimpleView.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleView : UserControl
    {

        MDMatch[] matchers;

        public FeaturePointCollection pointColletion;

        public SimpleView()
        {
            InitializeComponent();
            pointColletion = (FeaturePointCollection)(Application.Current.Resources["featurePointCollection"] as ObjectDataProvider).Data;
        }

        private void comboBoxL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = comboBoxL.SelectedItem as Photo;
            //Binding binding = new Binding("Image");
            //binding.Source = item;
            //binding.Mode = BindingMode.OneWay;
            //imgL.SetBinding(Image.SourceProperty, binding);
            imgL.Source = item.Image;
            //paint(imgL, comboBoxL.SelectedItem as Photo, pointColletion.Select(o => new PointD(o.LX, o.LY)).ToList());
        }

        private void comboBoxR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = comboBoxR.SelectedItem as Photo;
            //Binding binding = new Binding("Image");
            //binding.Source = item;
            //binding.Mode = BindingMode.OneWay;
            //imgR.SetBinding(Image.SourceProperty, binding);
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

                const float minRatio = 1f / 1.5f;

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

                // List<FeaturePoint> pointList = new List<FeaturePoint>();

                for (int i = 0; i < matchers.Length; i++)
                {

                    MKeyPoint ll = modelKeyPoints[matchers[i].TrainIdx];

                    MKeyPoint rr = observedKeyPoints[matchers[i].QueryIdx];

                    FeaturePoint p = new FeaturePoint(i, ll, rr, matchers[i].Distance);

                    pointColletion.Add(p);

                }

                // points = pointList.ToArray();
                //todo repaint
            }

            bindPoints();


        }

        private void bindPoints()
        {
            Binding bind = new Binding();
            bind.Source = pointColletion;
            pointColletion.OrderBy(o => o.LX);
            listViewPoints.SetBinding(ListView.ItemsSourceProperty, bind);

            Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Source);
            Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Source);
            int i = -1;
            Random romdom = new Random();
            foreach (var v in pointColletion)
            {
                i++;
                var mcv = new MCvScalar(romdom.Next(255), romdom.Next(255), romdom.Next(255));
                CvInvoke.PutText(l, i + "", new System.Drawing.Point((int)v.LX, (int)v.LY), FontFace.HersheyComplex, 0.7, mcv);
                CvInvoke.PutText(r, i + "", new System.Drawing.Point((int)v.RX, (int)v.RY), FontFace.HersheyComplex, 0.7, mcv);
                l.Draw(new Cross2DF(v.TrainPoint.Point, 15, 15), new Bgr(255, 255, 255), 1);
                r.Draw(new Cross2DF(v.QueryPoint.Point, 15, 15), new Bgr(255, 255, 255), 1);
            }
            imgL.Source = BitmapUtils.ChangeBitmapToImageSource(l.Bitmap);
            imgR.Source = BitmapUtils.ChangeBitmapToImageSource(r.Bitmap);
        }

        private void DrawVisual(Image img, String pth)
        {
            BitmapImage bitmap = new BitmapImage(new Uri(pth));

        }

        private void paint(Image image, Photo photo, List<PointD> points)
        {

        }

        private void comboBoxLImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (scrollViewerL == null) return;

            var view = scrollViewerL;
            var img = imgL;

            ComboBox box = sender as ComboBox;
            String sel = box.SelectedItem as string;

            UIHelper.ZoomImage(img, view, sel);

            bindPoints();
        }

        private void comboBoxRImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (scrollViewerR == null) return;

            var view = scrollViewerR;
            var img = imgR;

            ComboBox box = sender as ComboBox;
            String sel = box.SelectedItem as string;

            UIHelper.ZoomImage(img, view, sel);

            //paint();

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
            pointColletion.UpdateItemIndex();

            bindPoints();

        }


        private void listViewPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var point = (sender as ListView).SelectedItem as FeaturePoint;
            var index = (sender as ListView).SelectedIndex;
            if (index != -1)
            {
                int R = (int)Math.Round(imgLD.Width) / 2;

                System.Drawing.Point pl = new System.Drawing.Point((int)point.LX, (int)point.LY);
                System.Drawing.Point pr = new System.Drawing.Point((int)point.RX, (int)point.RY);


                //Rect rl = new Rect(ImageUtils.GetLeftPoint(pl, R), new Size(R, R));
                //Rect rr = new Rect(ImageUtils.GetLeftPoint(pr, R), new Size(R, R));
                Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Source);
                Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Source);

                Image<Bgr, byte> ld = new Image<Bgr, byte>(R, R);
                Image<Bgr, byte> rd = new Image<Bgr, byte>(R, R);
                l.ROI = new System.Drawing.Rectangle(ImageUtils.GetLeftPoint(pl, R), new System.Drawing.Size(R, R));
                l.CopyTo(ld);
                l.ROI = System.Drawing.Rectangle.Empty;
                r.ROI = new System.Drawing.Rectangle(ImageUtils.GetLeftPoint(pr, R), new System.Drawing.Size(R, R));
                r.CopyTo(rd);
                r.ROI = System.Drawing.Rectangle.Empty;

                ld = CVUtils.DrawCenterCross(ld);
                rd = CVUtils.DrawCenterCross(rd);


                imgLD.Source = BitmapUtils.ChangeBitmapToImageSource(ld.Bitmap);
                imgRD.Source = BitmapUtils.ChangeBitmapToImageSource(rd.Bitmap);

            }
        }
    }
}
