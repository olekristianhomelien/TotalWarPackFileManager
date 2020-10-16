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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CommonDialogs.FilterUserControl;

namespace CommonDialogs.FilterDialog
{
    /// <summary>
    /// Interaction logic for CollapsableFilterControl.xaml
    /// </summary>
    public partial class CollapsableFilterControl : UserControl
    {

        public EventHandler OnItemDoubleClicked;
        public EventHandler OnItemSelected;
        public EventHandler OnOpeningFirstTime;

        public CollapsableFilterControl()
        {
            InitializeComponent();
        }



        public int LabelTotalWidth
        {
            get { return (int)GetValue(LabelTotalWidthProperty); }
            set { SetValue(LabelTotalWidthProperty, value);}
        }

        public static readonly DependencyProperty LabelTotalWidthProperty =
            DependencyProperty.Register("LabelTotalWidth", typeof(int), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public int SearchFieldOffset
        {
            get { return (int)GetValue(SearchFieldOffsetProperty); }
            set { SetValue(SearchFieldOffsetProperty, value); }
        }

        public static readonly DependencyProperty SearchFieldOffsetProperty =
            DependencyProperty.Register("SearchFieldOffset", typeof(int), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(CollapsableFilterControl), new PropertyMetadata(null));



        public string CustomFilterText
        {
            get { return (string)GetValue(CustomFilterTextProperty); }
            set { SetValue(CustomFilterTextProperty, value); }
        }

        public static readonly DependencyProperty CustomFilterTextProperty =
            DependencyProperty.Register("CustomFilterText", typeof(string), typeof(CollapsableFilterControl), new PropertyMetadata(null));



        public Visibility CustomFilterVisibility
        {
            get { return (Visibility)GetValue(CustomFilterVisibilityProperty); }
            set { SetValue(CustomFilterVisibilityProperty, value); }
        }


        public static readonly DependencyProperty CustomFilterVisibilityProperty =
            DependencyProperty.Register("CustomFilterVisibility", typeof(Visibility), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public bool ApplyCustomFilterAsDefault
        {
            get { return (bool)GetValue(ApplyCustomFilterAsDefaultProperty); }
            set { SetValue(ApplyCustomFilterAsDefaultProperty, value); }
        }

        public static readonly DependencyProperty ApplyCustomFilterAsDefaultProperty =
            DependencyProperty.Register("ApplyCustomFilterAsDefault", typeof(bool), typeof(CollapsableFilterControl), new PropertyMetadata(null));




        public void SetItems(IEnumerable<object> items, ExternalFilter externalFilter = null)
        {
            FilterBox.SetItems(items, externalFilter);
        }
    }
}
