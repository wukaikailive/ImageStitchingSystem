using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ImageStitchingSystem.Domain
{
    public class TextFieldsViewModel : INotifyPropertyChanged
    {
        private string _name;
        private int _selectedValueOne;

        public TextFieldsViewModel()
        {
            LongListToTestComboVirtualization = new List<int>(Enumerable.Range(0, 1000));

            SelectedValueOne = LongListToTestComboVirtualization.Skip(2).First();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                this.MutateVerbose(ref _name, value, RaisePropertyChanged());
            }
        }

        public int SelectedValueOne
        {
            get { return _selectedValueOne; }
            set
            {
                this.MutateVerbose(ref _selectedValueOne, value, RaisePropertyChanged());
            }
        }

        public IList<int> LongListToTestComboVirtualization { get; }

        public DemoItem DemoItem => new DemoItem { Name = "Mr. Test"};

        public event PropertyChangedEventHandler PropertyChanged;

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }
}
