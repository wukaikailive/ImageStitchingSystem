using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ImageStitchingSystem.Models
{
    public class FeaturePoint : INotifyPropertyChanged
    {
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
                this.Changed("Index");
            }
        }

        public FeaturePoint()
        {
            TrainPoint = new MKeyPoint();
            QueryPoint = new MKeyPoint();
        }

        private void Changed(String propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private MKeyPoint _trainPoint;

        private MKeyPoint _queryPoint;

        public MKeyPoint TrainPoint
        {
            get
            {
                return _trainPoint;
            }

            set
            {
                _trainPoint = value;
            }
        }

        public MKeyPoint QueryPoint
        {
            get
            {
                return _queryPoint;
            }

            set
            {
                _queryPoint = value;
            }
        }
        public double LX
        {
            set { _trainPoint.Point.X = (float)value; }

            get { return _trainPoint.Point.X; }
        }
        public double LY { get { return TrainPoint.Point.Y; } set { _trainPoint.Point.Y = (float)value; } }
        public double RX { get { return QueryPoint.Point.X; } set { _queryPoint.Point.X = (float)value; } }
        public double RY { get { return QueryPoint.Point.Y; } set { _queryPoint.Point.Y = (float)value; } }
        public double Distance { get; set; }

        public FeaturePoint(int index, MKeyPoint t, MKeyPoint q, Double distance)
        {
            Index = index;
            TrainPoint = t;
            QueryPoint = q;
            Distance = distance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class FeaturePointCollection : ObservableCollection<FeaturePoint>
    {
        public int Index { get; set; }

        public void UpdateIndex()
        {
            int i = -1;
            foreach(var v in this)
            {
                i++;
                v.Index = i;
            }
            
        }
    }

    public class FeaturePointCollections : ObservableCollection<FeaturePointCollection>
    {
        public FeaturePointCollection this[int index]
        {
            get
            {
                foreach(var v in this)
                {
                    if (v.Index == index)
                    {
                        return v;
                    }
                }
                return null;
            }
            set
            {
                foreach (var v in this)
                {
                    FeaturePointCollection x = v;
                    if (v.Index == index)
                    {
                        x = value;
                    }
                }
            }
        }
    }
}
