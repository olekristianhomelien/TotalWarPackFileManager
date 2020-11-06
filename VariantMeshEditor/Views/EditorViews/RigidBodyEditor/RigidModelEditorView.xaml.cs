using CommonDialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace VariantMeshEditor.Views.EditorViews.RigidBodyEditor
{

    public partial class RigidModelEditorView : UserControl
    {

        public RigidModelEditorView()
        {

            InitializeComponent();
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
