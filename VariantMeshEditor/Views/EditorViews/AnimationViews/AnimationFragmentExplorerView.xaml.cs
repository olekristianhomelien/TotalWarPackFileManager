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


            MyItemsSource2 = new ObservableCollection<Customer>();
            MyItemsSource2.Add(new Customer() { Name = "Dogman" });
            MyItemsSource2.Add(new Customer() { Name = "Dogman2" });
            MyItemsSource2.Add(new Customer() { Name = "train" });
            MyItemsSource2.Add(new Customer() { Name = "line" });
            MyItemsSource2.Add(new Customer() { Name = "Jenny" });
            InitializeComponent();
    
        }



      public ObservableCollection<Customer> MyItemsSource2 { get; set; }
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
