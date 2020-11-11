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
            HandleOnItemSelected();
            FilterBox.Visibility = Visibility.Collapsed;
            BrowseButton.Content = "Browse";
        }

        void HandleOnItemSelected()
        {
            var selectedItem = FilterBox.SelectedItem;
            if (selectedItem != null)
            {
                var val = selectedItem.GetType().GetProperty(DisplayMemberPath).GetValue(selectedItem, null);
                SelectedFileName.Text = val.ToString();
            }
            else
            {
                SelectedFileName.Text = "";
            }
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
                BrowseButton.Content = "Hide";
                FilterBox.Visibility = Visibility.Visible;
            }
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
            DependencyProperty.Register("SearchItems", typeof(IEnumerable), typeof(CollapsableFilterControl), new PropertyMetadata(null));


        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(CollapsableFilterControl), new PropertyMetadata(null));
            
        #endregion
    }
}
