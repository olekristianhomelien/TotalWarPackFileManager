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
            var treeItem = GetParentOfType<TreeViewItem>(expander);
            treeItem.IsExpanded = expander.IsExpanded;
        }

        public static T GetParentOfType<T>(CollapsableButton child) where T : DependencyObject
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
