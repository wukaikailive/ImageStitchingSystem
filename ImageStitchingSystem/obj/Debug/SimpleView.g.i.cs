﻿#pragma checksum "..\..\SimpleView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BEC103993F686493E3D9AC7F50FEDDDE"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using ImageStitchingSystem;
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ImageStitchingSystem {
    
    
    /// <summary>
    /// SimpleView
    /// </summary>
    public partial class SimpleView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 71 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboBoxL;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboBoxR;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgL;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgR;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox algsComboBox;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonFindFeatures;
        
        #line default
        #line hidden
        
        
        #line 130 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button addButton;
        
        #line default
        #line hidden
        
        
        #line 131 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button deleteButton;
        
        #line default
        #line hidden
        
        
        #line 134 "..\..\SimpleView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox autoEstimateCheckBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ImageStitchingSystem;component/simpleview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\SimpleView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.comboBoxL = ((System.Windows.Controls.ComboBox)(target));
            
            #line 72 "..\..\SimpleView.xaml"
            this.comboBoxL.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.comboBoxL_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.comboBoxR = ((System.Windows.Controls.ComboBox)(target));
            
            #line 77 "..\..\SimpleView.xaml"
            this.comboBoxR.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.comboBoxR_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.imgL = ((System.Windows.Controls.Image)(target));
            return;
            case 4:
            this.imgR = ((System.Windows.Controls.Image)(target));
            return;
            case 5:
            this.algsComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 101 "..\..\SimpleView.xaml"
            this.algsComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.algsComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.buttonFindFeatures = ((System.Windows.Controls.Button)(target));
            return;
            case 7:
            this.addButton = ((System.Windows.Controls.Button)(target));
            return;
            case 8:
            this.deleteButton = ((System.Windows.Controls.Button)(target));
            return;
            case 9:
            this.autoEstimateCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

