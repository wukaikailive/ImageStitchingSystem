using System;
using System.Globalization;
using System.Windows.Data;
using ImageStitchingSystem.Other;
using ImageStitchingSystem.Utils;

namespace ImageStitchingSystem.Models
{
    /// <summary>
    /// Converts an exposure time from a decimal (e.g. 0.0125) into a string (e.g. 1/80)
    /// </summary>
    public class ExposureTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)

        {
            try
            {
                decimal exposure = (decimal)value;

                exposure = Decimal.Round(1 / exposure);
                return $"1/{exposure}";
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            string exposure = ((string) value).Substring(2);
            return (1 / Decimal.Parse(exposure));
        }
    }

    /// <summary>
    /// Converts an exposure mode from an enum into a human-readable string (e.g. AperturePriority
    /// becomes "Aperture Priority")
    /// </summary>
    public class ExposureModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ExposureMode exposureMode = (ExposureMode)value;

            switch (exposureMode)
            {
                case ExposureMode.AperturePriority:
                    return "Aperture Priority";
                    
                case ExposureMode.HighSpeedMode:
                    return "High Speed Mode";

                case ExposureMode.LandscapeMode:
                    return "Landscape Mode";

                case ExposureMode.LowSpeedMode:
                    return "Low Speed Mode";

                case ExposureMode.Manual:
                    return "Manual";

                case ExposureMode.NormalProgram:
                    return "Normal";

                case ExposureMode.PortraitMode:
                    return "Portrait";

                case ExposureMode.ShutterPriority:
                    return "Shutter Priority";

                default:
                    return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts a lens aperture from a decimal into a human-preferred string (e.g. 1.8 becomes F1.8)
    /// </summary>
    public class LensApertureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return string.Format("F{0:##.0}", value);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                return Decimal.Parse(((string)value).Substring(1));
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Converts a focal length from a decimal into a human-preferred string (e.g. 85 becomes 85mm)
    /// </summary>
    public class FocalLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return string.Format("{0}mm", value);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts an x,y size pair into a string value (e.g. 1600x1200)
    /// </summary>
    public class PhotoSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values[0] == null) || (values[1] == null))
            {
                return string.Empty;
            }
            else
            {
                return string.Format("{0}x{1}", values[0], values[1]);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if ((string) value == string.Empty)
            {
                return new object[2];
            }
            else
            {
                string[] sSize = new string[2];
                sSize = ((string)value).Split('x');
                
                object[] size = new object[2];
                size[0] = UInt32.Parse(sSize[0]);
                size[1] = UInt32.Parse(sSize[1]);
                return size;
            }
        }
    }

    public class PhotoFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Photo photo = (Photo)value;
                
                return "#"+photo.Index+": "+TextUtils.GetFileName(photo.Source);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PixelPrecisionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                double v = (double)value;
                return v.ToString("0.00");
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FeaturePointIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                FeaturePoint point = (FeaturePoint)value;
                // var pointColletion = (SimpleView.Resources["featurePointCollection"] as FeaturePointCollection);
               
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                double d = (double)value;
                return d.ToString("0.0");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string d = (string)value;
                return double.Parse(d);
            }
            return 0;
        }
    }

    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
