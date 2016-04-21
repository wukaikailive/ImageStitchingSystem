using ImageStitchingSystemNew.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageStitchingSystemNew.UI
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        PhotoCollection Photos;

        public MainWindow()
        {
            InitializeComponent();
            Photos = (Application.Current.Resources["Photos"] as ObjectDataProvider).Data as PhotoCollection;

        }

        private void button_load_img_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "图片文件|*.jpg;*.png;*.bmp";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                insertImgs(openFileDialog.FileNames);
            }
        }
        private void insertImgs(String[] imgs)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                this.Photos.Add(new Photo(imgs[i]));
            }
            photosListBox.ItemsSource = this.Photos;
        }

        private void button_clear_img_Click(object sender, RoutedEventArgs e)
        {
            this.Photos.Clear();
        }

        private void button_previre_img_Click(object sender, RoutedEventArgs e)
        {
            PhotoEditWindow pew = new PhotoEditWindow();
            pew.SelectedPhoto = photosListBox.SelectedItem as Photo;
            pew.Show();
        }

        private void button_delete_img_Click(object sender, RoutedEventArgs e)
        {
            this.Photos.Remove(photosListBox.SelectedItem as Photo);
        }
    }
}
