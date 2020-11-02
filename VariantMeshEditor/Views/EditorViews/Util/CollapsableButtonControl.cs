using CommonDialogs.Common;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VariantMeshEditor.Views.EditorViews.Util
{

    public class CollapsableButtonViewModel : NotifyPropertyChangedImpl
    {
        public ICommand OnButtonPressed { get; set; }
        double _ContentHight;
        public double ContentHeight { get { return _ContentHight; } set { _ContentHight = value; NotifyPropertyChanged(); } }
        
        string _buttonSymbol = "🡆";
        public string ButtonSymbol { get { return _buttonSymbol; } set { _buttonSymbol = value; NotifyPropertyChanged(); } }

        string _buttonText;
        public string ButtonText { get { return _buttonText; } set { _buttonText = value; NotifyPropertyChanged(); } }
    }


    public class CollapsableButtonControl : UserControl
    {
        CollapsableButtonViewModel _viewModel;
        static CollapsableButtonControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CollapsableButtonControl), new FrameworkPropertyMetadata(typeof(CollapsableButtonControl)));
        }



        public string CustomFilterText
        {
            get { return (string)GetValue(CustomFilterTextProperty); }
            set { SetValue(CustomFilterTextProperty, value); }
        }

        public static readonly DependencyProperty CustomFilterTextProperty = DependencyProperty.Register("CustomFilterText", typeof(string), typeof(CollapsableButtonControl), new PropertyMetadata(null));


        public CollapsableButtonControl(string headerName)
        {
       
            _viewModel = new CollapsableButtonViewModel()
            {
                OnButtonPressed = new RelayCommand(OnClick),
                ButtonText = headerName,
            };

            
            CustomFilterText = " CustomFilterText";

            //DataContext = _viewModel;
        }

        bool _isClosed = true;
        public void OnClick()
        {
            if (_isClosed)
            {
                _viewModel.ButtonSymbol = "🡇";
                _viewModel.ContentHeight = double.NaN;
            }
            else
            {
                _viewModel.ButtonSymbol = "🡆";
                _viewModel.ContentHeight = 0;
            }

            CustomFilterText = " CustomFilterText_CustomFilterText";

            _isClosed = !_isClosed;
        }

    }
}
