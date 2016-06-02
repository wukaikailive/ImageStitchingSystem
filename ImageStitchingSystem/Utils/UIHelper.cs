using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
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

        //根据缩放选项来缩放图片
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

        public static void SetSmallImg(Image imgSmall, Image<Bgr, byte> img, System.Drawing.Point center, double width, int mult = 2, double add = 0)
        {
            //int r = (int)Math.Round(width) / mult;
            //Image<Bgr, byte> ld = new Image<Bgr, byte>(r, r);
            //Size size=new Size(r,r);
            //System.Drawing.Point left = ImageUtils.GetLeftPoint(center, r);
            //if (left.X < 0)
            //{
            //    size.Width=size.Width+ left.X;
            //    left.X = 0;
            //}
            //if (left.Y < 0)
            //{
            //    size.Height = size.Height + left.Y;
            //    left.Y = 0;
            //}
            //if (left.X > img.Width)
            //{
            //    size.Width = size.Width - left.X;
            //    left.X = img.Width;
            //}
            //if (left.Y > img.Height)
            //{
            //    size.Height = size.Height - left.Y;
            //    left.Y = img.Height;
            //}
            //img.ROI = new System.Drawing.Rectangle(left, size);
            //img.CopyTo(ld);
            //img.ROI = System.Drawing.Rectangle.Empty;
            ////Image<Hsv, byte> lld = new Image<Hsv, byte>(ld.Bitmap);
            ////for (int i = 0; i < lld.Rows; i++)
            ////{
            ////    for (int j = 0; j < lld.Cols; j++)
            ////    {
            ////        Hsv hsv = lld[i, j];
            ////        hsv.Value += add;
            ////        if (hsv.Value < 0) hsv.Value = 0;
            ////        else if (hsv.Value > 255) hsv.Value = 255;
            ////        lld[i, j] = hsv;
            ////    }
            ////}
            ////ld=new Image<Bgr, byte>(lld.Bitmap);

            int r = (int)Math.Round(width) / mult;

            int sx=0, sy=0, bx=0, by=0;

            int w = img.Width;
            int h = img.Height;

            Size s = new Size();
            System.Drawing.Point left = ImageUtils.GetLeftPoint(center, r);
            int px = left.X;
            int py = left.Y;
            if (px > 0 && py > 0 && w - px > r && h - py > r)
            {
                sx = px;
                sy = py;
                s = new Size(r, r);

                bx = 0;
                by = 0;
            }else if (px < 0 && py < 0)
            {
                sx = -px;
                sy = -py;
                s = new Size(r + px, r + py);

                bx = -px;
                by = -py;
            }
            else if (px < 0 && h - py > r)
            {
                sx = 0;
                sy = py;
                s = new Size(r + px, r);

                bx = -px;
                by = 0;
            }
            else if (px < 0 && h - py < r)
            {
                sx = 0;
                sy = py;
                s = new Size(r + px, h-py);

                bx = -px;
                by = 0;
            }
            else if (px > 0 && py < 0 && w - px > r)
            {
                sx = px;
                sy = 0;
                s = new Size(r, r + py);

                bx = 0;
                by = -py;
            }
            else if (px > 0 && py < 0 && w - px < r)
            {
                sx = px;
                sy = 0;
                s = new Size(w-px, r + py);

                bx = 0;
                by = -py;
            }else if (px > 0 && py > 0 && w - px < r && h - py > r)
            {
                sx = px;
                sy = py;
                s = new Size(w-px,r);

                bx = 0;
                by = 0;
            }else if (px > 0 && py > 0 && w - px < r && h - py < r)
            {
                sx = px;
                sy = py;
                s = new Size(w-px, h-py);

                bx = 0;
                by = 0;
            }
            else if (px > 0 && py > 0 && w - px > r && h - py < r)
            {
                sx = px;
                sy = py;
                s = new Size(r, h - py);

                bx = 0;
                by = 0;
            }
            Image<Bgr, byte> ld = new Image<Bgr, byte>(r, r);
            System.Drawing.Point s_start = new System.Drawing.Point(sx,sy);
            System.Drawing.Point b_start = new System.Drawing.Point(bx, by);
            img.ROI = new System.Drawing.Rectangle(s_start, s);
            ld.ROI=new Rectangle(b_start,s);
            img.CopyTo(ld);
            img.ROI = System.Drawing.Rectangle.Empty;
            ld.ROI = System.Drawing.Rectangle.Empty;
            ld = CvUtils.DrawCenterCross(ld);

            imgSmall.Source = BitmapUtils.ChangeBitmapToImageSource(ld.Bitmap);
        }

        public static void SetSmallImg(Image imgSmall, Image<Bgr, byte> img, System.Windows.Point center, double width, int mult = 2, double add = 0)
        {
            //int r = (int)Math.Round(width) / mult;
            //Image<Bgr, byte> ld = new Image<Bgr, byte>(r, r);
            //Size size = new Size(r, r);
            //System.Windows.Point left = ImageUtils.GetLeftPoint(center, r);
            //if (left.X < 0)
            //{
            //    size.Width = (int)(size.Width + left.X);
            //    left.X = 0;
            //}
            //if (left.Y < 0)
            //{
            //    size.Height = (int)(size.Height + left.Y);
            //    left.Y = 0;
            //}
            //if (left.X > img.Width)
            //{
            //    size.Width = (int)(size.Width - left.X);
            //    left.X = img.Width;
            //}
            //if (left.Y > img.Height)
            //{
            //    size.Height = (int)(size.Height - left.Y);
            //    left.Y = img.Height;
            //}
            //img.ROI = new System.Drawing.Rectangle(new System.Drawing.Point((int)left.X, (int)left.Y), size);
            //img.CopyTo(ld);
            //img.ROI = System.Drawing.Rectangle.Empty;

            ////Image<Hsv, byte> lld = new Image<Hsv, byte>(ld.Bitmap);
            ////for (int i = 0; i < lld.Rows; i++)
            ////{
            ////    for (int j = 0; j < lld.Cols; j++)
            ////    {
            ////        Hsv hsv = lld[i, j];
            ////        hsv.Value += add;
            ////        if (hsv.Value < 0) hsv.Value = 0;
            ////        else if (hsv.Value > 255) hsv.Value = 255;
            ////        lld[i, j] = hsv;
            ////    }
            ////}
            ////ld = new Image<Bgr, byte>(lld.Bitmap);
            //ld = CvUtils.DrawCenterCross(ld);


            //imgSmall.Source = BitmapUtils.ChangeBitmapToImageSource(ld.Bitmap);
            int r = (int)Math.Round(width) / mult;

            int sx = 0, sy = 0, bx = 0, by = 0;

            int w = img.Width;
            int h = img.Height;

            Size s = new Size();
            System.Drawing.Point left = ImageUtils.GetLeftPoint(center, r);
            int px = left.X;
            int py = left.Y;
            if (px > 0 && py > 0 && w - px > r && h - py > r)
            {
                sx = px;
                sy = py;
                s = new Size(r, r);

                bx = 0;
                by = 0;
            }
            else if (px < 0 && py < 0)
            {
                sx = -px;
                sy = -py;
                s = new Size(r + px, r + py);

                bx = -px;
                by = -py;
            }
            else if (px < 0 && h - py > r)
            {
                sx = 0;
                sy = py;
                s = new Size(r + px, r);

                bx = -px;
                by = 0;
            }
            else if (px < 0 && h - py < r)
            {
                sx = 0;
                sy = py;
                s = new Size(r + px, h - py);

                bx = -px;
                by = 0;
            }
            else if (px > 0 && py < 0 && w - px > r)
            {
                sx = px;
                sy = 0;
                s = new Size(r, r + py);

                bx = 0;
                by = -py;
            }
            else if (px > 0 && py < 0 && w - px < r)
            {
                sx = px;
                sy = 0;
                s = new Size(w - px, r + py);

                bx = 0;
                by = -py;
            }
            else if (px > 0 && py > 0 && w - px < r && h - py > r)
            {
                sx = px;
                sy = py;
                s = new Size(w - px, r);

                bx = 0;
                by = 0;
            }
            else if (px > 0 && py > 0 && w - px < r && h - py < r)
            {
                sx = px;
                sy = py;
                s = new Size(w - px, h - py);

                bx = 0;
                by = 0;
            }
            else if (px > 0 && py > 0 && w - px > r && h - py < r)
            {
                sx = px;
                sy = py;
                s = new Size(r, h - py);

                bx = 0;
                by = 0;
            }
            Image<Bgr, byte> ld = new Image<Bgr, byte>(r, r);
            System.Drawing.Point s_start = new System.Drawing.Point(sx, sy);
            System.Drawing.Point b_start = new System.Drawing.Point(bx, by);
            img.ROI = new System.Drawing.Rectangle(s_start, s);
            ld.ROI = new Rectangle(b_start, s);
            img.CopyTo(ld);
            img.ROI = System.Drawing.Rectangle.Empty;
            ld.ROI = System.Drawing.Rectangle.Empty;
            ld = CvUtils.DrawCenterCross(ld);

            imgSmall.Source = BitmapUtils.ChangeBitmapToImageSource(ld.Bitmap);
        }

        public static void MoveToPoint(ScrollViewer view, Image img, System.Drawing.Point p)
        {
            double zoom = 1D;
            if (img.Source.Width / img.Source.Height > img.ActualWidth / img.ActualHeight)
            {
                zoom = img.ActualWidth / img.Source.Width;
            }
            else
            {
                zoom = img.ActualHeight / img.Source.Height;
            }

            double px = view.ActualWidth / 2;
            double py = view.ActualHeight / 2;
            view.ScrollToHorizontalOffset(p.X * zoom - px);
            view.ScrollToVerticalOffset(p.Y * zoom - py);
        }

        public static void DrawCross(DrawingContext dc, Point p, int l, Brush brush)
        {
            Point top = new Point(p.X, p.Y - (l >> 1));
            Point bottom = new Point(p.X, p.Y + (l >> 1));
            Point left = new Point(p.X - (l >> 1), p.Y);
            Point right = new Point(p.X + (l >> 1), p.Y);
            var pen = new Pen(brush, 1);

            dc.DrawLine(pen, top, bottom);
            dc.DrawLine(pen, left, right);
        }

        public static FormattedText GetEnTextObject(string str, int fontsize, Brush brush)
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
            Brush b = new SolidColorBrush(Color.FromRgb((byte)R, (byte)g, (byte)B));
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
