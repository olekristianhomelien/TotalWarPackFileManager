using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonDialogs
{
    /// <summary>
    /// Interaction logic for CollapsableButton.xaml
    /// </summary>
    
    [ContentProperty("InnerContent")]
    public partial class CollapsableButton : UserControl
    {

        #region InnerContent
        public FrameworkElement InnerContent
        {
            get { return (FrameworkElement)GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerContent", typeof(FrameworkElement), typeof(CollapsableButton), new UIPropertyMetadata(null));



        public Visibility CustomFilterVisibility
        {
            get { return (Visibility)GetValue(CustomFilterVisibilityProperty); }
            set { SetValue(CustomFilterVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CustomFilterVisibilityProperty =
            DependencyProperty.Register("CustomFilterVisibility", typeof(Visibility), typeof(CollapsableButton), new PropertyMetadata(null));



        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(CollapsableButton), new PropertyMetadata(null));





        public string LabelSymbol
        {
            get { return (string)GetValue(LabelSymbolProperty); }
            set { SetValue(LabelSymbolProperty, value); }
        }

        public static readonly DependencyProperty LabelSymbolProperty =
            DependencyProperty.Register("LabelSymbol", typeof(string), typeof(CollapsableButton), new PropertyMetadata(null));

        #endregion

        public CollapsableButton()
        {
            InitializeComponent();
            LabelSymbol = "🡆";
            CustomFilterVisibility = Visibility.Visible;
        }
        bool _isOpen = true;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_isOpen)
            {
                LabelSymbol = "🡇";
                ContentGrid.Height = double.NaN;
            }
            else
            {
                LabelSymbol = "🡆";
                ContentGrid.Height = 0;
            }
            _isOpen = !_isOpen;

        }
    }
}
