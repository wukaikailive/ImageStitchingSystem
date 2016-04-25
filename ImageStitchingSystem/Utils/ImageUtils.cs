using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ImageStitchingSystem.Utils
{
    public class ImageUtils
    {
        public static void DowheelZoom(TransformGroup group, Point point, double delta)
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

        public static System.Drawing.Point GetLeftPoint(System.Drawing.Point point,int rectWith)
        {
            int h = rectWith / 2;
            return new System.Drawing.Point(point.X - h, point.Y - h);
        }
        public static Point GetLeftPoint(System.Windows.Point point, int rectWith)
        {
            int h = rectWith / 2;
            return new Point(point.X - h, point.Y - h);
        }

        /// <summary>
        /// 在中心绘制一个十字
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void DrawCenterCross(DrawingContext dc,int width,int height)
        {
            Point top = new Point(width / 2, 0);
            Point bottom = new Point(width / 2, height);
            Point left = new Point(0, height / 2);
            Point right = new Point(width, height / 2);
            var pen=new Pen(Brushes.Black,1);
            dc.DrawLine(pen, top, bottom);
            dc.DrawLine(pen, left, right);
        }
    }
}
