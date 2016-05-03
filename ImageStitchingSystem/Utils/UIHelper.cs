using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Size = System.Drawing.Size;

namespace ImageStitchingSystem.Utils
{
    public class UiHelper
    {
        public static void Text(Canvas canvas, double x, double y, string text, Color fontColor)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
            textBlock.Foreground = new SolidColorBrush(fontColor);
            textBlock.FontSize = 12;
            textBlock.FontWeight = FontWeights.Thin;
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            canvas.Children.Add(textBlock);
        }

        public static void ZoomImage(Image img, ScrollViewer view, string sel)
        {
            var width = img.Source.Width;
            var height = img.Source.Height;

            var hp = ScrollBarVisibility.Visible;
            var vp = ScrollBarVisibility.Visible;

            switch (sel)
            {
                case "自动适应":
                    hp = ScrollBarVisibility.Disabled;
                    vp = ScrollBarVisibility.Disabled;
                    width = view.Width;
                    height = view.Height;
                    break;
                default:
                    double v = ParseZoomString(sel) / 100;
                    width *= v;
                    height *= v;
                    break;
            }

            img.Width = width;
            img.Height = height;

            view.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, hp);
            view.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, vp);
        }

        public static double ParseZoomString(string sel)
        {
            const string pattern = "^(.*?)%$";
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            var match = regex.Match(sel);
            double v = 0;
            try
            {
                if (match.Success)
                {
                    v = double.Parse(match.Groups[1].Value);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return v;
        }

        public static void MoveToCenter(ScrollViewer view, System.Drawing.Point p)
        {
            double aw = view.ActualWidth;
            double ah = view.ActualHeight;

            double w = view.Width;
            double h = view.Height;

            double sv = view.VerticalOffset;
            double sh = view.HorizontalOffset;

            // double centerx = p.X - aw / 2;
            // double centery = p.Y - ah / 2;

            //double rx = 0;
            //double ry = 0;

            //double cw = w / 2;
            //double ch = w / 2;

            //if (p.X < cw && p.Y < ch)
            //{
            //    rx = p.X + (cw - p.X);

            //}

            view.ScrollToHorizontalOffset(p.X);
            view.ScrollToVerticalOffset(p.Y);

        }

        public static void SetSmallImg(Image imgSmall, Image<Bgr, byte> img, System.Drawing.Point center, double width, int mult = 2)
        {
            int r = (int)Math.Round(width) / mult;
            Image<Bgr, byte> ld = new Image<Bgr, byte>(r, r);
            Size size=new Size(r,r);
            System.Drawing.Point left = ImageUtils.GetLeftPoint(center, r);
            if (left.X < 0)
            {
                size.Width=size.Width+ left.X;
                left.X = 0;
            }
            if (left.Y < 0)
            {
                size.Height = size.Height + left.Y;
                left.Y = 0;
            }
            if (left.X > img.Width)
            {
                size.Width = size.Width - left.X;
                left.X = img.Width;
            }
            if (left.Y > img.Height)
            {
                size.Height = size.Height - left.Y;
                left.Y = img.Height;
            }
            img.ROI = new System.Drawing.Rectangle(left, size);
            img.CopyTo(ld);
            img.ROI = System.Drawing.Rectangle.Empty;

            ld = CvUtils.DrawCenterCross(ld);

            imgSmall.Source = BitmapUtils.ChangeBitmapToImageSource(ld.Bitmap);
        }

        public static void SetSmallImg(Image imgSmall, Image<Bgr, byte> img, System.Windows.Point center, double width, int mult = 2)
        {
            int r = (int)Math.Round(width) / mult;
            Image<Bgr, byte> ld = new Image<Bgr, byte>(r, r);
            img.ROI = new System.Drawing.Rectangle(ImageUtils.GetLeftPoint(new System.Drawing.Point((int)center.X,(int)center.Y),r), new System.Drawing.Size(r, r));
            img.CopyTo(ld);
            img.ROI = System.Drawing.Rectangle.Empty;

            ld = CvUtils.DrawCenterCross(ld);

            imgSmall.Source = BitmapUtils.ChangeBitmapToImageSource(ld.Bitmap);
        }

        public static void MoveToPoint(ScrollViewer view, System.Drawing.Point p)
        {
            view.ScrollToHorizontalOffset(p.X);
            view.ScrollToVerticalOffset(p.Y);
        }

        public static void DrawCross(DrawingContext dc, Point p, int l,Brush brush)
        {
            Point top = new Point(p.X, p.Y - l / 2);
            Point bottom = new Point(p.X, p.Y + l / 2);
            Point left = new Point(p.X - l / 2, p.Y);
            Point right = new Point(p.X + l / 2, p.Y);
            var pen = new Pen(brush, 1);

            dc.DrawLine(pen, top, bottom);
            dc.DrawLine(pen, left, right);
        }

        public static FormattedText GetEnTextObject(string str,int fontsize,Brush brush)
        {
            return new FormattedText(str,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        fontsize,
                        brush);
        }

        public static FormattedText GetCnTextObject(string str, int fontsize, Brush brush)
        {
            return new FormattedText(str,
                        CultureInfo.GetCultureInfo("zh-cn"),
                        FlowDirection.LeftToRight,
                        new Typeface("宋体"),
                        fontsize,
                        brush);
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }
                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }

        public static Brush GetBrush(int n)
        {
            Random r = new Random(n);
            int R = r.Next(256);
            int g = r.Next(256);
            int B = (R + g > 400) ? 0 : 400 - R - g;
            B = B > 255 ? 255 : B;
            Brush b = new SolidColorBrush(Color.FromRgb((byte)R, (byte)g,(byte)B));
            return b;
        }

        public static Color GetDarkerColor(Color color)
        {
            const int max = 255;
            int increase = new Random(Guid.NewGuid().GetHashCode()).Next(30, 255); //还可以根据需要调整此处的值


            int r = Math.Abs(Math.Min(color.R - increase, max));
            int g = Math.Abs(Math.Min(color.G - increase, max));
            int b = Math.Abs(Math.Min(color.B - increase, max));


            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }
    }
}
