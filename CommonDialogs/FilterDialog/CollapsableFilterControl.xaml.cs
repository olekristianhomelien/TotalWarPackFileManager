using CommonDialogs.Common;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static CommonDialogs.FilterDialog.FilterUserControl;

namespace CommonDialogs.FilterDialog
{
    /// <summary>
    /// Interaction logic for CollapsableFilterControl.xaml
    /// </summary>
    public partial class CollapsableFilterControl : UserControl
    {

        public delegate bool OnItemSelectedDelegate(object sender);
        public delegate void OnOpeningFirstTimeDelegate(CollapsableFilterControl sender);

        public OnItemSelectedDelegate OnItemSelected;
        public OnOpeningFirstTimeDelegate OnOpeningFirstTime;
        bool _firstTimeOpening = true;

        public CollapsableFilterControl()
        {
            InitializeComponent();
            FilterBox.OnItemSelected += (a, b) => HandleOnItemSelected();
            FilterBox.OnItemDoubleClicked += (a, b) => HandleItemDoubleClicked();
            BrowseButton.Click += (a, b) => ToggleSearchFiled();
            FilterBox.Visibility = Visibility.Collapsed;
        }


        void HandleItemDoubleClicked()
        {
            var restult = HandleOnItemSelected();
            if (restult.HasValue && restult.Value)
            {
                FilterBox.Visibility = Visibility.Collapsed;
                BrowseButton.Content = "Browse";
            }
        }

        bool? HandleOnItemSelected()
        {
            var selectedItem = FilterBox.GetSelectedItem();
            SelectedFileName.Text = selectedItem.ToString();
            return OnItemSelected?.Invoke(selectedItem);
        }

        void ToggleSearchFiled()
        {
            if (FilterBox.Visibility == Visibility.Visible)
            {
                FilterBox.Visibility = Visibility.Collapsed;
                BrowseButton.Content = "Browse";
            }
            else
            {
                if (_firstTimeOpening)

                {
                    using (new WaitCursor())
                    {
                        OnOpeningFirstTime?.Invoke(this);
                        _firstTimeOpening = false;
                    }
                }
                
                BrowseButton.Content = "Hide";
                FilterBox.Visibility = Visibility.Visible;
            }
        }

        public void SetItems(IEnumerable<object> items, IEnumerable<GridViewColumn> columns, ExternalFilter externalFilter = null)
        {
            FilterBox.SetItems(items, columns, externalFilter);
        }


        #region properties

        public FrameworkElement InnerContent
        {
            get { return (FrameworkElement)GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerContent", typeof(FrameworkElement), typeof(CollapsableFilterControl), new UIPropertyMetadata(null));

        public int LabelTotalWidth
        {
            get { return (int)GetValue(LabelTotalWidthProperty); }
            set { SetValue(LabelTotalWidthProperty, value);}
        }

        public static readonly DependencyProperty LabelTotalWidthProperty =
            DependencyProperty.Register("LabelTotalWidth", typeof(int), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(CollapsableFilterControl), new PropertyMetadata(null));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public OnSeachDelegate OnSearch
        {
            get { return (OnSeachDelegate)GetValue(OnSearchProperty); }
            set { SetValue(OnSearchProperty, value); }
        }

        public static readonly DependencyProperty OnSearchProperty =
            DependencyProperty.Register("OnSearch", typeof(OnSeachDelegate), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public IEnumerable SearchItems
        {
            get { return (IEnumerable)GetValue(SearchItemsProperty); }
            set { SetValue(SearchItemsProperty, value); }
        }

        public static readonly DependencyProperty SearchItemsProperty =
            DependencyProperty.Register("SearchItems", typeof(IEnumerable), typeof(CollapsableFilterControl), new PropertyMetadata(OnCurrentReadingChanged));

        static private void OnCurrentReadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
   
        }


        #endregion
    }
}
