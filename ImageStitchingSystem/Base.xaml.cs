using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageStitchingSystem
{
    /// <summary>
    /// Base.xaml 的交互逻辑
    /// </summary>
    public partial class Base : UserControl
    {
        public Base()
        {
            InitializeComponent();
            baseGrid.ShowGridLines = true;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "图片文件|*.jpg;*.png;*.bmp";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PContext.imgs = openFileDialog.FileNames;
                insertImgs(PContext.imgs);
            }
        }

        private void insertImgs(String[] imgs)
        {
            baseGrid.Children.Clear();
            int col = (int)baseGrid.GetValue(Grid.ColumnProperty);
            for(int i = 0; i < imgs.Length/col+1; i++)
            {
                RowDefinition rowD = new RowDefinition();
                rowD.Height = new GridLength(200);
                baseGrid.RowDefinitions.Add(new RowDefinition());
            }  
            for (int i = 0; i < imgs.Length; i++)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                SetSource(image, imgs[i]);
                
                image.SetValue(Grid.ColumnProperty, i % col);
                image.SetValue(Grid.RowProperty, (i / col));
                baseGrid.Children.Add(image);
            }
        }

        private void insertImg(String img)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            SetSource(image, img);
            int col = (int)baseGrid.GetValue(Grid.ColumnProperty);
            int count = baseGrid.Children.Count;
            image.SetValue(Grid.ColumnProperty, count % col + 1);
            image.SetValue(Grid.RowProperty, count / col +1);
            baseGrid.Children.Insert(baseGrid.Children.Count, image);
        }

        private void SetSource(System.Windows.Controls.Image image, string fileName)
        {
            System.Drawing.Image sourceImage = System.Drawing.Image.FromFile(fileName);
            int imageWidth = 0, imageHeight = 0;
            InitializeImageSize(sourceImage, image, out imageWidth, out imageHeight);

            Bitmap sourceBmp = new Bitmap(sourceImage, imageWidth, imageHeight);
            IntPtr hBitmap = sourceBmp.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            WriteableBitmap writeableBmp = new WriteableBitmap(bitmapSource);
            sourceImage.Dispose();
            sourceBmp.Dispose();
            image.Source = writeableBmp;
        }

        private static void InitializeImageSize(System.Drawing.Image sourceImage, System.Windows.Controls.Image image,
             out int imageWidth, out int imageHeight)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;
            float aspect = (float)width / (float)height;
            if (!image.Height.Equals(double.NaN))
            {
                imageHeight = Convert.ToInt32(image.Height);
                imageWidth = Convert.ToInt32(aspect * imageHeight);
            }
            else if (!image.Width.Equals(double.NaN))
            {
                imageWidth = Convert.ToInt32(image.Width);
                imageHeight = Convert.ToInt32(image.Width / aspect);
            }
            else
            {
                imageHeight = 100;
                imageWidth = Convert.ToInt32(aspect * imageHeight);
            }
        }
    }
}
