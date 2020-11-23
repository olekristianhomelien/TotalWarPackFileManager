using CommonDialogs.Common;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Scene;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class AnimationSplicerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonNode;
        AnimationPlayerViewModel _animationPlayer;


        bool _isSelected;
        public bool IsSelected { get { return _isSelected; } set { SetAndNotify(ref _isSelected, value); IsInFocus(IsSelected); } }
        public ObservableCollection<SkeletonBoneNode> Bones { get; set; } = new ObservableCollection<SkeletonBoneNode>();


        public AnimationSplicerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _skeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;


            foreach (var bone in _skeletonNode.SkeletonFile.Bones)
            {
                if (bone.ParentId == -1)
                {
                    Bones.Add(CreateNode(bone));
                }
                else
                {
                    var parentBone = _skeletonNode.SkeletonFile.Bones[bone.ParentId];
                    var treeParent = GetParent(Bones, parentBone);

                    if (treeParent != null)
                        treeParent.Children.Add(CreateNode(bone));
                }
            }
        }

        // Copy pasted from skeletonViewModel
        SkeletonBoneNode CreateNode(AnimationFile.BoneInfo bone)
        {
            SkeletonBoneNode item = new SkeletonBoneNode
            {
                BoneIndex = bone.Id,
                BoneName = bone.Name + " [" + bone.Id + "]" + " P[" + bone.ParentId + "]",
                BoneRef = bone
            };
            return item;
        }

        SkeletonBoneNode GetParent(ObservableCollection<SkeletonBoneNode> root, AnimationFile.BoneInfo parentBone)
        {
            foreach (SkeletonBoneNode item in root)
            {
                if (item.BoneRef == parentBone)
                    return item;

                var result = GetParent(item.Children, parentBone);
                if (result != null)
                    return result;
            }
            return null;
        }
        // ----------------------------

        void ApplyCurrentAnimation()
        {
            _animationPlayer.SetAnimationClip(null, null);
        }

        void IsInFocus(bool isInFocus)
        {
            if (isInFocus)
                ApplyCurrentAnimation();
        }
    }
}
