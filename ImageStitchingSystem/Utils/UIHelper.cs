using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageStitchingSystem.Utils
{
    public class UIHelper
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

        public static void ZoomImage(Image img, ScrollViewer view, String sel)
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

        public static double ParseZoomString(String sel)
        {
            string pattern = "^(.*?)%$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
            System.Text.RegularExpressions.Match match = regex.Match(sel);
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
    }
}
