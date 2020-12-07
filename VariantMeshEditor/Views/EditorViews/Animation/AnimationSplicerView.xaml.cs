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

namespace VariantMeshEditor.Views.EditorViews.Animation
{
    /// <summary>
    /// Interaction logic for AnimationSplicerEditorView.xaml
    /// </summary>
    public partial class AnimationSplicerView : UserControl
    {
        public AnimationSplicerView()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
           //TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
           //
           //if (treeViewItem != null)
           //{
           //    treeViewItem.IsSelected = true;
           //    treeViewItem.Focus();
           //    e.Handled = true;
           //}
        }  //

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
    }
}
