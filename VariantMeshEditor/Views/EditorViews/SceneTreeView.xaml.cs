using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Linq;
using VariantMeshEditor.ViewModels;
using System.Windows.Data;
using System;

namespace VariantMeshEditor.Views.EditorViews
{
    public partial class SceneTreeView : UserControl
    {
        public SceneTreeView()
        {
            InitializeComponent();
        }

        private void TreeViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView.SelectedItem is FileSceneElement item)
                item.IsChecked = !item.IsChecked;
            e.Handled = true;
        }
    }

    public class TreeItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is FileSceneElement)
            {
                FileSceneElement sceneElement = item as FileSceneElement;
                switch (sceneElement.Type)
                {
                    case FileSceneElementEnum.Slot:
                        return element.FindResource("SlotTreeItemTemplate") as HierarchicalDataTemplate; ;

                    default:
                        return element.FindResource("DefaultTreeItemTemplate") as HierarchicalDataTemplate;
                }

            }

            return null;
        }
    }

    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            if (item != null)
            {
                
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }
}