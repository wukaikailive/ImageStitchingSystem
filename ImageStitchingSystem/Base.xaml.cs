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

        public PhotoCollection Photos;

        public Base()
        {
            InitializeComponent();
            this.Photos = (PhotoCollection)(this.Resources["Photos"] as ObjectDataProvider).Data;
        }

        private void OnPhotoClick(object sender, RoutedEventArgs e)
        {
            //PhotoView pvWindow = new PhotoView();
            //pvWindow.SelectedPhoto = (Photo)photosListBox.SelectedItem;
            //pvWindow.Show();
        }

        private void editPhoto(object sender, RoutedEventArgs e)
        {
            PhotoView pvWindow = new PhotoView();
            pvWindow.SelectedPhoto = (Photo)photosListBox.SelectedItem;
            pvWindow.Show();
        }

        private void OnImagesDirChangeClick(object sender, RoutedEventArgs e)
        {
           // this.Photos.Path = ImagesDir.Text;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //ImagesDir.Text = Environment.CurrentDirectory + "\\images";
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
            for(int i = 0; i < imgs.Length; i++)
            {
                this.Photos.Add(new Photo(imgs[i]));
            }
            
            photosListBox.ItemsSource = this.Photos;
        }

        private void insertImg(String img)
        {
           
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
