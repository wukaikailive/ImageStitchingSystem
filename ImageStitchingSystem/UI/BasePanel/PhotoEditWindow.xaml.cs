using ImageStitchingSystem.Models;
using ImageStitchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ImageStitchingSystem.UI
{
    /// <summary>
    /// photoEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PhotoEditWindow : Window
    {
        private Photo _photo;
        private bool _mIsMouseLeftButtonDown;

        private Bitmap _bitmap;

        public Bitmap Bitmap { get; set; }

        public Photo SelectedPhoto
        {
            get { return _photo; }
            set { _photo = value; }
        }

        public PhotoEditWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ViewedPhoto.Source = _photo != null ? _photo.Image : BitmapUtils.ChangeBitmapToImageSource(_bitmap);
        }


        private void MasterImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)

        {

            Rectangle rectangle = sender as Rectangle;

            if (rectangle == null)

                return;



            rectangle.ReleaseMouseCapture();

            _mIsMouseLeftButtonDown = false;

        }



        private Point _mPreviousMousePoint;

        private void MasterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            Rectangle rectangle = sender as Rectangle;

            if (rectangle == null)

                return;



            rectangle.CaptureMouse();

            _mIsMouseLeftButtonDown = true;

            _mPreviousMousePoint = e.GetPosition(rectangle);

        }



        private void MasterImage_MouseMove(object sender, MouseEventArgs e)

        {

            Rectangle rectangle = sender as Rectangle;

            if (rectangle == null)

                return;



            if (_mIsMouseLeftButtonDown)

                DoImageMove(rectangle, e);

        }



        private void DoImageMove(Rectangle rectangle, MouseEventArgs e)

        {

            //Debug.Assert(e.LeftButton == MouseButtonState.Pressed);

            if (e.LeftButton != MouseButtonState.Pressed)

                return;



            TransformGroup group = MainPanel.FindResource("ImageTransformResource") as TransformGroup;

            Debug.Assert(group != null);

            TranslateTransform transform = group.Children[1] as TranslateTransform;

            Point position = e.GetPosition(rectangle);

            transform.X += position.X - _mPreviousMousePoint.X;

            transform.Y += position.Y - _mPreviousMousePoint.Y;



            _mPreviousMousePoint = position;

        }



        private void MasterImage_MouseWheel(object sender, MouseWheelEventArgs e)

        {

            TransformGroup group = MainPanel.FindResource("ImageTransformResource") as TransformGroup;

            Debug.Assert(group != null);
            Point point = Mouse.GetPosition(sender as Rectangle);

            var delta = e.Delta * 0.002;
            DowheelZoom(group, point, delta);

            //var pointToContent = group.Inverse.Transform(point);

            //ScaleTransform transform = group.Children[0] as ScaleTransform;

            // transform.CenterX = pointToContent.X;
            //transform.CenterY = pointToContent.Y;

            // transform.ScaleX += e.Delta * 0.002;

            //transform.ScaleY += e.Delta * 0.002;

        }


        private void DowheelZoom(TransformGroup group, Point point, double delta)
        {
            var pointToContent = group.Inverse.Transform(point);
            var transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX + delta < 0.1) return;
            transform.ScaleX += delta;
            transform.ScaleY += delta;
            var transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }

        /// <summary>   
        /// 获取鼠标的坐标   
        /// </summary>   
        /// <param name="lpPoint">传址参数，坐标point类型</param>   
        /// <returns>获取成功返回真</returns>   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Point pt);
    }
}
