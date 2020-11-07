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

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(CollapsableButton), new PropertyMetadata(null));

        protected string LabelSymbol
        {
            get { return (string)GetValue(LabelSymbolProperty); }
            set { SetValue(LabelSymbolProperty, value); }
        }

        public static readonly DependencyProperty LabelSymbolProperty =
            DependencyProperty.Register("LabelSymbol", typeof(string), typeof(CollapsableButton), new PropertyMetadata(null));


        public string LabelLength
        {
            get { return (string)GetValue(LabelLengthProperty); }
            set { SetValue(LabelLengthProperty, value);}
        }

        public static readonly DependencyProperty LabelLengthProperty =
            DependencyProperty.Register("LabelLength", typeof(string), typeof(CollapsableButton), new PropertyMetadata(null));


        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set
            {
                SetValue(IsExpandedProperty, value);
                Expand(value);
            }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(CollapsableButton), new PropertyMetadata(null));
        #endregion

        public event RoutedEventHandler OpenStateChanged;

        public CollapsableButton()
        {
            LabelSymbol = "🡆";
            InitializeComponent();
        }

        void Expand(bool value)
        {
            if (value)
            {
                LabelSymbol = "🡇";
            }
            else
            {
                LabelSymbol = "🡆";
            }

            OpenStateChanged?.Invoke(this, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
        }
    }
}
