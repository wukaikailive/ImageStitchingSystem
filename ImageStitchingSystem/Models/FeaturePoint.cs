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
        private void Changed(String propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MKeyPoint TrainPoint { get; set; }
        public MKeyPoint QueryPoint { get; set; }
        public double LX
        {
            get { return TrainPoint.Point.X; }
        }
        public double LY { get { return TrainPoint.Point.Y; } }
        public double RX { get { return QueryPoint.Point.X; } }
        public double RY { get { return QueryPoint.Point.Y; } }
        public double Distance { get; set; }

        public FeaturePoint() { }
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
