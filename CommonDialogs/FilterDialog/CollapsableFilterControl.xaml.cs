using CommonDialogs.Common;
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

        public OnSeachDelegate OnSearch
        {
            get { return (OnSeachDelegate)GetValue(OnSearchProperty); }
            set { SetValue(OnSearchProperty, value); }
        }

        public static readonly DependencyProperty OnSearchProperty =
            DependencyProperty.Register("OnSearch", typeof(OnSeachDelegate), typeof(CollapsableFilterControl), new PropertyMetadata(null));

        #endregion
    }
}
