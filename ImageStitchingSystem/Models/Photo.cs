using ImageStitchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ImageStitchingSystem.Models
{
    public class Photo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int index;

        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                Changed("Index");
            }
        }

        private void Changed(String propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public String Path { get; set; }

        private Bitmap bitmap;
        public Bitmap Bitmap
        {
            get
            {
                if (Path == null && bitmap == null)
                {
                    throw new ArgumentNullException("请检查路径");
                }
                if (bitmap == null)
                {
                    bitmap = new Bitmap(Path);
                }
                return bitmap;
            }

            set { }
        }

        private System.Windows.Media.ImageSource image;


        public System.Windows.Media.ImageSource Image
        {
            get { return BitmapUtils.ChangeBitmapToImageSource(Bitmap); }
            set
            {
                image = value;
            }
        }

        public String FileName { get { return TextUtils.getFileName(Path); } }

        public Photo() { }

        public Photo(String path)
        {
            Path = path;
        }

        public Photo(String path,int i)
        {
            Path = path;
            index = i;
        }

        public Photo(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }
    }

    public class PhotoCollection : ObservableCollection<Photo>
    {
         public int Index{ get; set; }

        public void UpdateItemIndex()
        {
            int i = -1;
            foreach (var v in this)
            {
                i++;
                v.Index = i;
            }
        }
    }

}
