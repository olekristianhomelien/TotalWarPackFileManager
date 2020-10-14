using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;


namespace VariantMeshEditor.Views.EditorViews.AnimationViews
{
    /// <summary>
    /// Interaction logic for AnimationFragmentExplorer.xaml
    /// </summary>
    public partial class AnimationFragmentExplorerView : UserControl
    {
        public AnimationFragmentExplorerView()
        {


            MyItemsSource = new ObservableCollection<Customer>();
            MyItemsSource.Add(new Customer() { Name = "Dogman" });
            MyItemsSource.Add(new Customer() { Name = "Dogman2" });
            MyItemsSource.Add(new Customer() { Name = "train" });
            MyItemsSource.Add(new Customer() { Name = "line" });
            MyItemsSource.Add(new Customer() { Name = "Jenny" });
            InitializeComponent();
    
        }



        public ObservableCollection<Customer> MyItemsSource { get; set; }
        public Customer MySelectedItem { get; set; }

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
