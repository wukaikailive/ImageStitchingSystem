using ImageStitchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageStitchingSystem.UI.Weight
{

    /// <summary>
    /// 
    /// </summary>

    class CanvasWithImage : Canvas
    {
        BitmapImage _image;

        PointCollection _collection;

        double _zoomLevel=0;

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public PointCollection PointCollection
        {
            get { return _collection; }
            set { _collection = value; }
        }
        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set { _zoomLevel = value; }
        }

        public CanvasWithImage() { }

        public CanvasWithImage(BitmapImage image, double zoom = 1f)
        {
            _image = image;
            _zoomLevel = zoom;
        }

        public CanvasWithImage(BitmapImage image, PointCollection collection, double zoom = 1f)
        {
            _image = image;
            _collection = collection;
            _zoomLevel = 1f;
        }

        protected override void OnRender(DrawingContext dc)
        {

            if (_image != null)
            {
                dc.DrawImage(_image, new System.Windows.Rect(0, 0, _image.PixelWidth * _zoomLevel, _image.PixelHeight * _zoomLevel));
            }
            else return;

            if (_zoomLevel == 0)
            {
                _zoomLevel = this.Height / _image.PixelWidth;
            }
            else return;

            if (_collection != null)
            {
                int i = -1;
                foreach (var v in _collection)
                {
                    i++;
                    double lx = v.X * _zoomLevel;
                    double ly = v.Y * _zoomLevel;

                    UIHelper.Text(this, lx, ly, i + "", Color.FromRgb(255, 0, 0));
                }
            }
            else return;

            //base.OnRender(dc);
        }

    }
}
