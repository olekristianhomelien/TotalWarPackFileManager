using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace CommonDialogs.FilterDialog
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        object _selectedItem;
        public FilterWindow()
        {
            InitializeComponent();
        }

        public FilterWindow(IEnumerable<object> items, FilterUserControl.ExternalFilter externalFilter = null, bool applyFilterAsDefault = true, string externalFilterName = "Apply external Filter")
        {
            InitializeComponent();
            Filter.SetItems(items, externalFilter, applyFilterAsDefault, externalFilterName);
            Filter.OnItemDoubleClicked += (sender, e) => OkButton_Click(null, null);
        }

        public object GetSelectedItem()
        {
            return _selectedItem;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedItem = Filter.GetSelectedItem();
            DialogResult = GetSelectedItem() != null;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
