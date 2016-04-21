using ImageStitchingSystem.Utils;
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
    public class ImageWithCenterCross : Image
    {
    

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            ImageUtils.DrawCenterCross(dc, (int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight));
        }
    }
}
