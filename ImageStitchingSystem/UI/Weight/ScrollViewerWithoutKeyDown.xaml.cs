using System;
using System.Collections.Generic;
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
    /// ScrollViewerWithoutKeyDown.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollViewerWithoutKeyDown : ScrollViewer
    {
        public ScrollViewerWithoutKeyDown()
        {
            InitializeComponent();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
           // base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
           // base.OnKeyUp(e);
        }

    }
}
