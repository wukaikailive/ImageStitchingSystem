using ImageStitchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace ImageStitchingSystem.UI.Weight
{
    public class MyImage : Image
    {

        #region 自定义属性

        private List<Point> _points;

        private Point _addPoint;

        private int _selectedIndex = -1;

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
            }
        }

        public List<Point> Points
        {
            get
            {
                return _points;
            }

            set
            {
                _points = value;
            }
        }

        public Point AddPoint
        {
            get
            {
                return _addPoint;
            }

            set
            {
                _addPoint = value;
            }
        }

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(
            "Points", typeof(List<Point>), typeof(MyImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPointsChanged))
            );

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
            "SelectedIndex", typeof(int), typeof(MyImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedIndexChanged))
            );

        public static readonly DependencyProperty AddPointProperty =
           DependencyProperty.Register(
           "AddPoint", typeof(Point), typeof(MyImage), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAddPointIndexChanged))
           );

        private static void OnAddPointIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MyImage u = (MyImage)d;
            u.UpdateLayout();
        }

        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        #endregion


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            // Rect rect = new Rect(new Point(11, 10), new Size(320, 80));
            // dc.DrawRectangle(Brushes.LightBlue, (Pen)null, rect);

            Random random = new Random();

            if (_points != null)
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

                Brush brush = Brushes.Blue;
                FormattedText text;

                foreach (var v in _points)
                {
                    i++;
                    if (_selectedIndex != -1 && _selectedIndex == i)
                    {
                        brush = Brushes.Red;
                    }
                    else
                    {
                        brush = UiHelper.GetBrush(i);
                    }
                    text = UiHelper.GetEnTextObject(i + "", 13, brush);

                    Point p = new Point(v.X * zoom, v.Y * zoom);
                    dc.DrawText(text, p);
                    UiHelper.DrawCross(dc, p, 15, Brushes.White);
                }

                if (_addPoint != null && _addPoint.X>0 && _addPoint.Y>0)
                {
                    Point p = new Point(_addPoint.X * zoom, _addPoint.Y * zoom);
                    text = UiHelper.GetCnTextObject("新建", 13, Brushes.Red);
                    dc.DrawText(text, p);
                    UiHelper.DrawCross(dc, p, 15, Brushes.White);
                }
            }

            //if (Source != null)
            //{
            //    dc.DrawImage(Source, new Rect(0, 0, 200, 200));
            //}

        }

    }
}
