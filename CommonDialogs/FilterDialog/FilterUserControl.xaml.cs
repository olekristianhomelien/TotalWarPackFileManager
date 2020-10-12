using CommonDialogs.Common;
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

namespace CommonDialogs
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

        public object GetSelectedItem()
        {
            return ResultList.SelectedItem;
        }

        public void SetItems(IEnumerable<object> items, ExternalFilter externalFilter = null, bool applyFilterAsDefault = true, string externalFilterName = "Apply external Filter")
        {
            _originalList = items;
            if (externalFilter != null)
            {
                ExtraFilterButton.Content = externalFilterName;
                _externalFilter = externalFilter;
                if (applyFilterAsDefault)
                    ExtraFilterButton.IsChecked = true;
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
