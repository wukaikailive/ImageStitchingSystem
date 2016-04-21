using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageStitchingSystem.UI.Weight
{
    public class MyImage : Image
    {
        List<Point> points;

        public List<Point> Points
        {
            get
            {
                return points;
            }

            set
            {
                points = value;
            }
        }

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(
            "Points", typeof(List<Point>), typeof(MyImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPointsChanged))
            );
        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            // Rect rect = new Rect(new Point(11, 10), new Size(320, 80));
            // dc.DrawRectangle(Brushes.LightBlue, (Pen)null, rect);
            
            //Random random;
            if (points != null)
            {
                int i = -1;
                double zoom = 1;


                if (Source.Width / Source.Height > ActualWidth / ActualHeight)
                {
                    zoom = ActualWidth / Source.Width;
                }
                else
                {
                    zoom = ActualHeight / Source.Height;
                }
                foreach (var v in points)
                {

                    i++;
                    FormattedText text = new FormattedText(
                        "" + i,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        13,
                        Brushes.Black);
                    dc.DrawText(text, new Point(v.X * zoom, v.Y * zoom));
                }
            }

            //if (Source != null)
            //{
            //    dc.DrawImage(Source, new Rect(0, 0, 200, 200));
            //}

        }
    }
}
