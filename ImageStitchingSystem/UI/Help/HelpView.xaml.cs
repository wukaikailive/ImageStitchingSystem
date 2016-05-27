using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Xps.Packaging;
using Microsoft.Office.Interop.Word;

namespace ImageStitchingSystem.UI
{
    /// <summary>
    /// HelpView.xaml 的交互逻辑
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
          
            docViewer.Document =   new XpsDocument(AppDomain.CurrentDomain.BaseDirectory + "/help.xps", System.IO.FileAccess.Read).GetFixedDocumentSequence();
            docViewer.FitToWidth();
        }
    }
}
