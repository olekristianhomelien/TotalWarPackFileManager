using CommonDialogs.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels.Skeleton
{
    class SkeletonViewModel : NotifyPropertyChangedImpl
    {
        SkeletonElement _skeletonElement;

        ObservableCollection
        public SkeletonViewModel(SkeletonElement skeletonElement)
        {
            _skeletonElement = skeletonElement;

            _viewModel = new SkeletonEditorView();
            _viewModel.SkeletonName.Content = "Skeleton Name: " + _skeletonElement.DisplayName;
            CreateBoneOverview();
        }


        void CreateBoneOverview()
        {
            _viewModel.SkeletonBonesView.Items.Clear();
            _viewModel.BoneCount.Content = "Bone Count : " + _skeletonElement.SkeletonFile.Bones.Count();
            var index = 0;
            foreach (var bone in _skeletonElement.SkeletonFile.Bones)
            {
                index++;
                if (bone.ParentId == -1)
                {
                    _viewModel.SkeletonBonesView.Items.Add(CreateNode(bone));
                }
                else
                {
                    var parentBone = _skeletonElement.SkeletonFile.Bones[bone.ParentId];
                    var treeParent = GetParent(_viewModel.SkeletonBonesView.Items, parentBone);

                    if (treeParent != null)
                        treeParent.Items.Add(CreateNode(bone));
                }
            }
        }

        TreeViewItem CreateNode(AnimationFile.BoneInfo bone)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = bone.Name + " [" + bone.Id + "]",
                Tag = bone,
                IsExpanded = true
            };
            return item;
        }

        TreeViewItem GetParent(ItemCollection root, AnimationFile.BoneInfo parentBone)
        {
            foreach (TreeViewItem item in root)
            {
                if (item.Tag == parentBone)
                    return item;

                var result = GetParent(item.Items, parentBone);
                if (result != null)
                    return result;
            }
            return null;
        }

    }
}
