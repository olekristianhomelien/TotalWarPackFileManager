using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace MyControls
{
    public class FilteredComboBox : ComboBox
    {
        private string oldFilter = string.Empty;

        private string currentFilter = string.Empty;

        protected TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;


        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(newValue);
                view.Filter += FilterItem;
            }

            if (oldValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(oldValue);
                if (view != null) view.Filter -= FilterItem;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                case Key.Enter:
                    IsDropDownOpen = false;
                    break;
                case Key.Escape:
                    IsDropDownOpen = false;
                    SelectedIndex = -1;
                    Text = currentFilter;
                    break;
                default:
                    if (e.Key == Key.Down) IsDropDownOpen = true;

                    base.OnPreviewKeyDown(e);
                    break;
            }

            // Cache text
            oldFilter = Text;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    break;
                case Key.Tab:
                case Key.Enter:

                    ClearFilter();
                    break;
                default:
                    if (Text != oldFilter)
                    {
                        var temp = Text;
                        RefreshFilter(); //RefreshFilter will change Text property
                        Text = temp;

                        if (SelectedIndex != -1 && Text != Items[SelectedIndex].ToString())
                        {
                            SelectedIndex = -1; //Clear selection. This line will also clear Text property
                            Text = temp;
                        }


                        IsDropDownOpen = true;

                        EditableTextBox.SelectionStart = int.MaxValue;
                    }

                    //automatically select the item when the input text matches it
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Text == Items[i].ToString())
                            SelectedIndex = i;
                    }

                    base.OnKeyUp(e);
                    currentFilter = Text;
                    break;
            }
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            ClearFilter();
            var temp = SelectedIndex;
            SelectedIndex = -1;
            Text = string.Empty;
            SelectedIndex = temp;
            base.OnPreviewLostKeyboardFocus(e);
        }

        private void RefreshFilter()
        {
            if (ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            view.Refresh();
        }

        private void ClearFilter()
        {
            currentFilter = string.Empty;
            RefreshFilter();
        }

        private bool FilterItem(object value)
        {
            if (value == null) return false;
            if (Text.Length == 0) return true;

            return value.ToString().ToLower().Contains(Text.ToLower());
        }
    }
}

namespace VariantMeshEditor.Views.EditorViews.AnimationViews
{
    /// <summary>
    /// Interaction logic for AnimationFragmentExplorer.xaml
    /// </summary>
    public partial class AnimationFragmentExplorer : UserControl
    {
        public AnimationFragmentExplorer()
        {
            ItemList = new ObservableCollection<string>();
            for (var i = 0; i < 1000; i++)
            {
                ItemList.Add($"Item {i}");
            }

            MyItemsSource = new ObservableCollection<Customer>();
            MyItemsSource.Add(new Customer() { Name = "Dogman" });
            MyItemsSource.Add(new Customer() { Name = "Dogman2" });
            MyItemsSource.Add(new Customer() { Name = "train" });
            MyItemsSource.Add(new Customer() { Name = "line" });
            MyItemsSource.Add(new Customer() { Name = "Jenny" });
            InitializeComponent();
           // cb.ItemsSource = ItemList;
    
        }

        private void Cb_OnPreviewTextInput(object sender, TextCompositionEventArgs e) 
        {
            //cb.IsDropDownOpen = true;
        }

        public ObservableCollection<string> ItemList { get; set; }
        public ObservableCollection<Customer> MyItemsSource { get; set; }
        public Customer MySelectedItem { get; set; }
        string _searchTextText;

        public string SearchTextText
        {
            get => _searchTextText;
            set
            {
                
                if (_searchTextText == value) return;
                _searchTextText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchTextText)));
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
