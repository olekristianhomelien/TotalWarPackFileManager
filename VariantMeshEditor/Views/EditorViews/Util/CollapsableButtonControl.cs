using CommonDialogs.Common;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VariantMeshEditor.Util;

namespace VariantMeshEditor.Views.EditorViews.Util
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:VariantMeshEditor.Views.EditorViews.Util"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:VariantMeshEditor.Views.EditorViews.Util;assembly=VariantMeshEditor.Views.EditorViews.Util"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    /// 
    public class ViwModel : NotifyPropertyChangedImpl
    {
        public ICommand MyCommand { get; set; }
        double _ContentHight;
        public double ContentHight { get { return _ContentHight; } set { _ContentHight = value; NotifyPropertyChanged(); } }
        
        string _buttonSymbol = "🡆";
        public string ButtonSymbol { get { return _buttonSymbol; } set { _buttonSymbol = value; NotifyPropertyChanged(); } }

        string _buttonText;
        public string ButtonText { get { return _buttonText; } set { _buttonText = value; NotifyPropertyChanged(); } }
    }

    


    public class CollapsableButtonControl : UserControl
    {
        public static DependencyProperty MyBane;

        ViwModel _viewModel;
        static CollapsableButtonControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CollapsableButtonControl),
           new FrameworkPropertyMetadata(typeof(CollapsableButtonControl)));
        }

        public CollapsableButtonControl(string headerName)
        {
            _viewModel = new ViwModel()
            {
                MyCommand = new RelayCommand(OnClick),
                ButtonText = headerName,
            };

            DataContext = _viewModel;
        }

        bool flag = true;
        public void OnClick()
        {
            if (flag)
            {
                _viewModel.ButtonSymbol = "🡇";
                _viewModel.ContentHight = Double.NaN;
            }
            else
            {
                _viewModel.ButtonSymbol = "🡆";
                _viewModel.ContentHight = 0;
            }
            flag = !flag;
        }

    }
}
