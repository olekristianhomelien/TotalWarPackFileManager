using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.ViewModels.VariantMesh;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;

namespace VariantMeshEditor.Controls.EditorControllers
{
    class SlotController
    {
        SlotElement _slotElement;
        SlotEditorView _viewModel;
        SkeletonElement _skeletonElement;
        public int AttachmentBoneIndex { get; set; } = -1;
        public SlotController(SlotEditorView viewModel, SlotElement slot, SkeletonElement skeletonElement)
        {
            _viewModel = viewModel;
            _slotElement = slot;
            _skeletonElement = skeletonElement;
            CreateUi();
        }

        void CreateUi()
        {
            //_viewModel.SlotName.Text = "Name:" + _slotElement.SlotName;
            _viewModel.AddButton.Click += AddButton_Click;

            //_viewModel.AttachmentPointComboBox.Items.Add("");
            //_viewModel.AttachmentPointComboBox.SelectionChanged += AttachmentPointComboBox_SelectionChanged;
            //if (_skeletonElement != null)
            //{
            //    foreach (var bone in _skeletonElement.SkeletonFile.Bones)
            //        _viewModel.AttachmentPointComboBox.Items.Add(bone.Name);
            //
            //    if (!string.IsNullOrWhiteSpace(_slotElement.AttachmentPoint))
            //    {
            //        //var selectedItem = skeletonElement.SkeletonModel.Bones.FirstOrDefault(x => x.Name == _slotElement.AttachmentPoint);
            //        //if (selectedItem != null)
            //            _viewModel.AttachmentPointComboBox.SelectedItem = _slotElement.AttachmentPoint;
            //    }
            //}

            CreateMeshList();
        }

        private void AttachmentPointComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
           // var selectedItem = _viewModel.AttachmentPointComboBox.SelectedItem as string;
           // if (string.IsNullOrWhiteSpace(selectedItem))
           // {
           //     AttachmentBoneIndex = -1;
           // }
           // else
           // {
           //     AttachmentBoneIndex = _skeletonElement.Skeleton.GetBoneIndex(selectedItem);
           // }  
        }

        void CreateMeshList()
        {
           //_viewModel.MeshStackPanel.Children.Clear();
           //foreach (var child in _slotElement.Children)
           //{
           //    if (child.Type == FileSceneElementEnum.VariantMesh ||
           //        child.Type == FileSceneElementEnum.WsModel ||
           //        child.Type == FileSceneElementEnum.RigidModel)
           //    {
           //        var meshReference = new BrowsableItemView();
           //        meshReference.LabelName.Content = "Mesh:";
           //        meshReference.PathTextBox.Text = child.FullPath;
           //        meshReference.Tag = child;
           //
           //        meshReference.RemoveButton.Click += RemoveButton_Click;
           //        meshReference.RemoveButton.Tag = child;
           //
           //        _viewModel.MeshStackPanel.Children.Add(meshReference);
           //    }
           //}
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
          // VariantMeshElement newSlot = new VariantMeshElement(_slotElement, "New");
          // _slotElement.Children.Add(newSlot);
          // //_sceneTreeView.CreateNode(newSlot, false, _slotElement);
          // CreateMeshList();
        }

        private void RemoveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
           // var button = sender as System.Windows.Controls.Button;
           // var parent = button.Tag as FileSceneElement;
           //

            //_sceneTreeView.RemoveNode();
        }
    }
}
