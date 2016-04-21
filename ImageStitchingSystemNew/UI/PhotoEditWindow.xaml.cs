using ImageStitchingSystemNew.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace ImageStitchingSystemNew.UI
{
    /// <summary>
    /// photoEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PhotoEditWindow : Window
    {

        Photo _photo;
        private bool m_IsMouseLeftButtonDown;

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
            ViewedPhoto.Source = _photo.Image;
        }


        private void MasterImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)

        {

            Rectangle rectangle = sender as Rectangle;

            if (rectangle == null)

                return;



            rectangle.ReleaseMouseCapture();

            m_IsMouseLeftButtonDown = false;

        }



        private Point m_PreviousMousePoint;

        private void MasterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            Rectangle rectangle = sender as Rectangle;

            if (rectangle == null)

                return;



            rectangle.CaptureMouse();

            m_IsMouseLeftButtonDown = true;

            m_PreviousMousePoint = e.GetPosition(rectangle);

        }



        private void MasterImage_MouseMove(object sender, MouseEventArgs e)

        {

            Rectangle rectangle = sender as Rectangle;

            if (rectangle == null)

                return;



            if (m_IsMouseLeftButtonDown)

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

            transform.X += position.X - m_PreviousMousePoint.X;

            transform.Y += position.Y - m_PreviousMousePoint.Y;



            m_PreviousMousePoint = position;

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
