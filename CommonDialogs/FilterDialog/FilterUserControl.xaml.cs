﻿using CommonDialogs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
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

namespace CommonDialogs.FilterDialog
{
    /// <summary>
    /// Interaction logic for FilterUserControl.xaml
    /// </summary>
    public partial class FilterUserControl : UserControl
    {
        IEnumerable<object> _originalList;
        SolidColorBrush _noErrorBackground;
        SolidColorBrush _errorBackground;
        public delegate IEnumerable<object> ExternalFilter(IEnumerable<object> originalList);
        public delegate bool OnSeachDelegate(object item);

        bool _useExternalFilter = false;

        ExternalFilter _externalFilter;


        public EventHandler OnItemDoubleClicked;
        public EventHandler OnItemSelected;

        public FilterUserControl()
        {
            InitializeComponent();
            _noErrorBackground = new SolidColorBrush(Colors.White);
            _errorBackground = new SolidColorBrush(Colors.Red);
            SearchTextBox.TextChanged += (sender, e) => FilterConditionChanged();
            ClearFilterButton.Click += (sender, e) => SearchTextBox.Text = "";
            ExtraFilterButton.Click += (sender, e) => { _useExternalFilter = !_useExternalFilter; FilterConditionChanged(); };
            ResultList.SelectionChanged += (sender, e) => OnItemSelected?.Invoke(null, null);
        }







        public string CustomFilterText
        {
            get { return (string)GetValue(CustomFilterTextProperty); }
            set { SetValue(CustomFilterTextProperty, value); }
        }

        public static readonly DependencyProperty CustomFilterTextProperty =
            DependencyProperty.Register("CustomFilterText", typeof(string), typeof(FilterUserControl), new PropertyMetadata(null));

        public Visibility CustomFilterVisibility
        {
            get { return (Visibility)GetValue(CustomFilterVisibilityProperty); }
            set { SetValue(CustomFilterVisibilityProperty, value); }
        }


        public static readonly DependencyProperty CustomFilterVisibilityProperty =
            DependencyProperty.Register("CustomFilterVisibility", typeof(Visibility), typeof(FilterUserControl), new PropertyMetadata(null));


        public bool ApplyCustomFilterAsDefault
        {
            get { return (bool)GetValue(ApplyCustomFilterAsDefaultProperty); }
            set { SetValue(ApplyCustomFilterAsDefaultProperty, value); }
        }

        public static readonly DependencyProperty ApplyCustomFilterAsDefaultProperty =
            DependencyProperty.Register("ApplyCustomFilterAsDefault", typeof(bool), typeof(FilterUserControl), new PropertyMetadata(null));




        public OnSeachDelegate OnSearch
        {
            get { return (OnSeachDelegate)GetValue(OnSearchProperty); }
            set { SetValue(OnSearchProperty, value); }
        }

        public static readonly DependencyProperty OnSearchProperty =
            DependencyProperty.Register("OnSearch", typeof(OnSeachDelegate), typeof(FilterUserControl), new PropertyMetadata(null));

        public object GetSelectedItem()
        {
            return ResultList.SelectedItem;
        }

        public void SetItems(IEnumerable<object> items, IEnumerable<GridViewColumn> columns, ExternalFilter externalFilter = null)
        {
            if (columns != null && columns.Any())
            {
                var gridView = new GridView();
                ResultList.View = gridView;
                foreach (var column in columns)
                {
                    gridView.Columns.Add(column);
                }
            }

            _originalList = items;
            if (externalFilter != null)
            {
                _externalFilter = externalFilter;
            }
            else
            {
                ExtraFilterButton.Visibility = Visibility.Hidden;
            }

            FilterConditionChanged();
        }

        private void FilterConditionChanged()
        {
            using (new WaitCursor())
            {

                var s = OnSearch;
                if (s != null)
                    OnSearch("Cat");

                var itemsToFilter = _originalList;
                if (_useExternalFilter)
                    itemsToFilter = _externalFilter(_originalList);
                ResultList.ItemsSource = itemsToFilter;

                SearchTextBox.Background = _noErrorBackground;
                var toolTip = SearchTextBox.ToolTip as ToolTip;
                if (toolTip == null)
                {
                    toolTip = new ToolTip();
                    SearchTextBox.ToolTip = toolTip;
                }

                var filterText = SearchTextBox.Text.ToLower();
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    toolTip.IsOpen = false;
                    return;
                }

                Regex rx = null;
                try
                {
                    rx = new Regex(filterText, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    toolTip.IsOpen = false;
                }
                catch (Exception e)
                {
                    SearchTextBox.Background = _errorBackground;
                    toolTip.IsOpen = true;
                    toolTip.Content = e.Message;
                    toolTip.Content += "\n\nCommon usage:\n";
                    toolTip.Content += "Value0.*Value1.*Value2 -> for searching for multiple substrings";
                    return;
                }

                ResultList.ItemsSource = itemsToFilter.Where(x => rx.Match(x.ToString()).Success);

            }
        }

        private void ResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnItemDoubleClicked?.Invoke(sender, e);
        }

        

    }
}
