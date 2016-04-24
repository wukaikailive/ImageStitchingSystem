using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageStitchingSystem.UI.Weight
{
    /// <summary>
    /// TabControlWithBackground.xaml 的交互逻辑
    /// </summary>
    public partial class TabControlWithBackground : TabControl
    {

        public static readonly DependencyProperty SelectedBgClrProperty = DependencyProperty.Register("SelectedBgClr",
        typeof(Brush), typeof(TabControlWithBackground), new UIPropertyMetadata(null));

        [Category("Appearance")]
        public Brush SelectedBgClr
        {
            get
            {
                return (Brush)GetValue(SelectedBgClrProperty);
            }
            set
            {
                SetValue(SelectedBgClrProperty, value);
            }
        }

        public TabControlWithBackground()
        {
            InitializeComponent();
        }
    }
}
