using ImageStitchingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ImageStitchingSystem.Other
{
    public class Photo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _index;

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                Changed("Index");
            }
        }

        private void Changed(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Path { get; set; }

        private Bitmap _bitmap;
        public Bitmap Bitmap
        {
            get
            {
                if (Path == null && _bitmap == null)
                {
                    throw new ArgumentNullException("请检查路径");
                }
                if (_bitmap == null)
                {
                    _bitmap = new Bitmap(Path);
                }
                return _bitmap;
            }

            set { }
        }

        private System.Windows.Media.ImageSource _image;


        public System.Windows.Media.ImageSource Image
        {
            get
            {
                return BitmapUtils.ChangeBitmapToImageSource(Bitmap);
            }
            set
            {
                _image = value;
            }
        }

        public string FileName { get { return TextUtils.GetFileName(Path); } }

        public Photo() { }

        public Photo(string path)
        {
            Path = path;
        }

        public Photo(string path,int i)
        {
            Path = path;
            _index = i;
        }

        public Photo(Bitmap bitmap)
        {
            this._bitmap = bitmap;
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
