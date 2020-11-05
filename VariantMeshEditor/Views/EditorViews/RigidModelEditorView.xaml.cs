using CommonDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews.Util;
using static VariantMeshEditor.Views.EditorViews.RigidModelEditorView;

namespace VariantMeshEditor.Views.EditorViews
{
    /// <summary>
    /// Interaction logic for RigidModelEditorView.xaml
    /// </summary>
    /// 

    public class GroupStyleSelector : StyleSelector
    {
        public Style NoGroupHeaderStyle { get; set; }
        public Style DefaultGroupStyle { get; set; }
        public Style ModelGroupStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var group = item as CollectionViewGroup;

            var name = group?.Name.ToString();
            if (name.Contains("Lod"))
                return DefaultGroupStyle;
            if (name.Contains("Model"))
                return ModelGroupStyle;
            return NoGroupHeaderStyle;


        }
    }

    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Mail { get; set; }

        public string LodId { get; set; }

        public string ModelId { get; set; }
    }

    public partial class RigidModelEditorView : UserControl
    {



        public RigidModelEditorView()
        {

            InitializeComponent();
            //List<User> items = new List<User>();
            //items.Add(new User() { Name = "John Doe", Age = 42, LodId = "Lod0", ModelId = "Model0"});
            //items.Add(new User() { Name = "Jane Doe", Age = 39, LodId = "Lod0", ModelId = "Model1" });
            //items.Add(new User() { Name = "Sammy Doe", Age = 13, LodId = "Lod1", ModelId = "Model0" });
            ////lvUsers.ItemsSource = items;
            ////this.DataContext = this;
            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvUsers.ItemsSource);
            //PropertyGroupDescription groupDescription = new PropertyGroupDescription("LodId");
            //PropertyGroupDescription groupDescription2 = new PropertyGroupDescription("ModelId");
            //view.GroupDescriptions.Add(groupDescription);
            //view.GroupDescriptions.Add(groupDescription2);
        }


        private void OnOpenStateChanged(object sender, RoutedEventArgs e)
        {
            var expander = (sender as CollapsableButton);
            var treeItem = expander.GetParentOfType<TreeViewItem>();
            treeItem.IsExpanded = expander.IsExpanded;// expander.IsExpanded;
        }



    }

    public static class Extensions
    {
        public static T GetParentOfType<T>(this Expander child) where T : DependencyObject
        {
            DependencyObject parentDependencyObject = child;
            do
            {
                parentDependencyObject = VisualTreeHelper.GetParent(parentDependencyObject);
                if (parentDependencyObject is T parent)
                    return parent;
            }
            while (parentDependencyObject != null);
            return null;
        }

        public static T GetParentOfType<T>(this CollapsableButton child) where T : DependencyObject
        {
            DependencyObject parentDependencyObject = child;
            do
            {
                parentDependencyObject = VisualTreeHelper.GetParent(parentDependencyObject);
                if (parentDependencyObject is T parent)
                    return parent;
            }
            while (parentDependencyObject != null);
            return null;
        }
    }
}
