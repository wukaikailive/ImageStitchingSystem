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

namespace ImageStitchingSystem
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
            pointColletion =(FeaturePointCollection) (Application.Current.Resources["featurePointCollection"] as ObjectDataProvider).Data;
        }

        private void comboBoxL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = comboBoxL.SelectedItem as Photo;
            Binding binding = new Binding("Image");
            binding.Source = item;
            binding.Mode = BindingMode.TwoWay;
            imgL.SetBinding(Image.SourceProperty, binding);
        }

        private void comboBoxR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Photo item = comboBoxR.SelectedItem as Photo;
            Binding binding = new Binding("Image");
            binding.Source = item;
            binding.Mode = BindingMode.TwoWay;
            imgR.SetBinding(Image.SourceProperty, binding);
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
            Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Bitmap);
            Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Bitmap);

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

                const float minRatio = 1f / 2f;

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

            Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Bitmap);
            Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Bitmap);
       
            Image<Bgr, byte> ll = CVUtils.Draw(l, pointColletion.Select(o => o.TrainPoint).ToList());
            Image<Bgr, byte> rr = CVUtils.Draw(r, pointColletion.Select(o => o.QueryPoint).ToList());

            Photo item = comboBoxL.SelectedItem as Photo;
            item.Bitmap = l.Bitmap;


        }

        private void comboBoxRImg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            String sel = box.SelectedItem as string;
            switch (sel)
            {
                case "自动适应":
                    break;
                case "25%":

                    break;
                case "50%":
                    break;
                case "75%":
                    break;
                case "100%":
                    break;
                case "125%":
                    break;
                case "150%":
                    break;
                case "175%":
                    break;
                case "200%":
                    break;
            }
        }

        private void rectangleL_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup group = this.FindResource("TransformL") as TransformGroup;

            Debug.Assert(group != null);
            Point point = Mouse.GetPosition(sender as Rectangle);

            var delta = e.Delta * 0.002;
            ImageUtils.DowheelZoom(group, point, delta);
        }

        private void rectangleR_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup group = this.FindResource("TransformR") as TransformGroup;

            Debug.Assert(group != null);
            Point point = Mouse.GetPosition(sender as Rectangle);

            var delta = e.Delta * 0.002;
            ImageUtils.DowheelZoom(group, point, delta);
        }

        private void rectangleR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void rectangleR_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void rectangleR_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void rectangleL_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void rectangleL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void rectangleL_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (Image<Bgr, byte> l = new Image<Bgr, byte>((comboBoxL.SelectedItem as Photo).Bitmap))
                using (Image<Bgr, byte> r = new Image<Bgr, byte>((comboBoxR.SelectedItem as Photo).Bitmap))
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
        }

    }
}
