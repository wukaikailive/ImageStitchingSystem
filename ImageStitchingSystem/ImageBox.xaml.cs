using ImageStitchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageStitchingSystem
{
    /// <summary>
    /// ImageBox.xaml 的交互逻辑
    /// </summary>
    public partial class ImageBox : Window
    {
        private Bitmap image;
        public ImageBox()
        {
            InitializeComponent();
        }

        public Bitmap Image
        {
            get
            {
                return image;
            }

            set
            {
                image = value;
                imageShow.Source = BitmapUtils.ChangeBitmapToImageSource(Image);

            }
        }
    }
}
