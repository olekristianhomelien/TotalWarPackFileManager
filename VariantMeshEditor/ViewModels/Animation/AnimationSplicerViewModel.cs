using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Scene;
using static CommonDialogs.FilterDialog.FilterUserControl;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.ViewModels.Animation
{


    public  class FilterableAnimations : NotifyPropertyChangedImpl
    {
        List<PackedFile> _filterList;

        public List<PackedFile> CurrentItems { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }
        List<PackedFile> AllAnimations { get; set; } = new List<PackedFile>();
        List<PackedFile> AnimationsForCurrentSkeleton { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }


        bool _onlyDisplayAnimationsForCurrentSkeleton = true;
        public bool OnlyDisplayAnimationsForCurrentSkeleton
        {
            get { return _onlyDisplayAnimationsForCurrentSkeleton; }
            set
            {
                SetAndNotify(ref _onlyDisplayAnimationsForCurrentSkeleton, value);
                if (value)
                    CurrentItems = AnimationsForCurrentSkeleton;
                else
                    CurrentItems = AllAnimations;
            }
        }

        public void FindAllAnimations(ResourceLibary resourceLibary, string skeletonName)
        {
            AllAnimations = PackFileLoadHelper.GetAllWithExtention(resourceLibary.PackfileContent, "anim");
            AnimationsForCurrentSkeleton.Clear();

            foreach (var animation in AllAnimations)
            {
                var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(animation.Data)).SkeletonName;
                if (animationSkeletonName == skeletonName)
                    AnimationsForCurrentSkeleton.Add(animation);
            }

            OnlyDisplayAnimationsForCurrentSkeleton = true;
        }
    }


    public class FilterableSkeletons : NotifyPropertyChangedImpl
    {
        public List<PackedFile> SkeletonList { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }

        public void FindAllSkeletons(ResourceLibary resourceLibary)
        {
            var allFilesInFolder = PackFileLoadHelper.GetAllFilesInDirectory(resourceLibary.PackfileContent, "animations\\skeletons");
            SkeletonList = allFilesInFolder.Where(x => x.FileExtention == "anim").ToList();
        }
    }

    public class AnimationSplicerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonNode;
        AnimationPlayerViewModel _animationPlayer;

        PackedFile _selectedAnimationForTargetSkeleton;
        public PackedFile SelectedAnimationForTargetSkeleton { get { return _selectedAnimationForTargetSkeleton; } set{SetAndNotify(ref _selectedAnimationForTargetSkeleton, value);} }

        PackedFile _selectedExternalSkeleton;
        public PackedFile SelectedExternalSkeleton { get { return _selectedExternalSkeleton; } set { SetAndNotify(ref _selectedExternalSkeleton, value); ExternalSkeletonSelected(); } }

        PackedFile _selectedAnimationForExternalSkeleton;
        public PackedFile SelectedAnimationForExternalSkeleton { get { return _selectedAnimationForExternalSkeleton; } set { SetAndNotify(ref _selectedAnimationForExternalSkeleton, value); } }

        public FilterableAnimations AnimationsForTargetSkeleton { get; set; } = new FilterableAnimations();
        public FilterableSkeletons PossibleExternalSkeletons { get; set; } = new FilterableSkeletons();
        public FilterableAnimations AnimationsForExternalSkeleton { get; set; } = new FilterableAnimations();



        AnimationFile _externalSkeletonFile;
        Viewer.Animation.Skeleton _externalSkeleton;


        bool _isSelected;
        public bool IsSelected { get { return _isSelected; } set { SetAndNotify(ref _isSelected, value); IsInFocus(IsSelected); } }
        public ObservableCollection<AdvSkeletonBoneNode> Bones { get; set; } = new ObservableCollection<AdvSkeletonBoneNode>();


        public ObservableCollection<SkeletonBoneNode> FlatExternalBonesList { get; set; } = new ObservableCollection<SkeletonBoneNode>();


        public AnimationSplicerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _skeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;

            AnimationsForTargetSkeleton.FindAllAnimations(_resourceLibary, _skeletonNode.SkeletonFile.Header.SkeletonName);
            PossibleExternalSkeletons.FindAllSkeletons(_resourceLibary);

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

            SelectedAnimationForTargetSkeleton = AnimationsForTargetSkeleton.CurrentItems
                .Find(x => x.FullPath == @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
      
            SelectedExternalSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid07.anim");
            SelectedAnimationForExternalSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid07\club_and_blowpipe\missile_actions\hu7_clbp_aim_idle_01.anim");
        }


        void ExternalSkeletonSelected()
        {
            SelectedAnimationForExternalSkeleton = null;
            _externalSkeleton = null;
            if (SelectedExternalSkeleton != null)
            {
                _externalSkeletonFile = AnimationFile.Create(new ByteChunk(SelectedExternalSkeleton.Data));
                AnimationsForExternalSkeleton.FindAllAnimations(_resourceLibary, _externalSkeletonFile.Header.SkeletonName);

                FlatExternalBonesList.Clear();
                foreach (var bone in _externalSkeletonFile.Bones)
                {
                    SkeletonBoneNode item = new SkeletonBoneNode
                    {
                        BoneIndex = bone.Id,
                        BoneName = bone.Name// + " [" + bone.Id + "]" + " P[" + bone.ParentId + "]",
                    };

                    FlatExternalBonesList.Add(item);
                }

                PrefilBoneMappingBasedOnName(Bones, FlatExternalBonesList);
            }
        }

        void PrefilBoneMappingBasedOnName(IEnumerable<AdvSkeletonBoneNode> nodes, ObservableCollection<SkeletonBoneNode> externalBonesList)
        {
            foreach(var node in nodes)
            {
                node.MappedBone = externalBonesList.FirstOrDefault(x => x.BoneName == node.BoneName);
                PrefilBoneMappingBasedOnName(node.Children, externalBonesList);

            }
        }

        // Copy pasted from skeletonViewModel
        AdvSkeletonBoneNode CreateNode(AnimationFile.BoneInfo bone)
        {
            AdvSkeletonBoneNode item = new AdvSkeletonBoneNode
            {
                BoneIndex = bone.Id,
                BoneName = bone.Name// + " [" + bone.Id + "]" + " P[" + bone.ParentId + "]",
            };
            return item;
        }

        AdvSkeletonBoneNode GetParent(ObservableCollection<AdvSkeletonBoneNode> root, AnimationFile.BoneInfo parentBone)
        {
            foreach (AdvSkeletonBoneNode item in root)
            {
                if (item.BoneIndex == parentBone.Id)
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

    public class AdvSkeletonBoneNode : NotifyPropertyChangedImpl
    {
        string _boneName;
        public string BoneName
        {
            get { return _boneName; }
            set { SetAndNotify(ref _boneName, value); }
        }

        int _boneIndex;
        public int BoneIndex
        {
            get { return _boneIndex; }
            set { SetAndNotify(ref _boneIndex, value); }
        }

        SkeletonBoneNode _mappedBone;
        public SkeletonBoneNode MappedBone { get { return _mappedBone; } set { SetAndNotify(ref _mappedBone, value); } }
        public ObservableCollection<AdvSkeletonBoneNode> Children { get; set; } = new ObservableCollection<AdvSkeletonBoneNode>();
    }
}
