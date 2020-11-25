using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;
using static CommonDialogs.FilterDialog.FilterUserControl;
using static Filetypes.RigidModel.AnimationFile;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.ViewModels.Animation
{


    public  class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        List<PackedFile> _filterList;

        public List<PackedFile> CurrentItems { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }
        List<PackedFile> AllAnimations { get; set; } = new List<PackedFile>();
        List<PackedFile> AnimationsForCurrentSkeleton { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }

        public event ValueChangedDelegate<PackedFile> SelectionChanged;
        PackedFile _selectedAnimation;
        public PackedFile SelectedItem { get { return _selectedAnimation; } set { SetAndNotify(ref _selectedAnimation, value, SelectionChanged); } }

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


    public class FilterableSkeletonsViewModel : NotifyPropertyChangedImpl
    {
        public AnimationFile SkeletonFile { get; set; }

        public List<PackedFile> SkeletonList { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }


        public event ValueChangedDelegate<PackedFile> SelectionChanged;
        PackedFile _selectedSkeleton;
        public PackedFile SelectedItem { get { return _selectedSkeleton; } set { SetAndNotify(ref _selectedSkeleton, value); OnSkeletonSelected(); } }


        public ObservableCollection<SkeletonBoneNode> SelectedSkeletonBonesFlattened { get; set; } = new ObservableCollection<SkeletonBoneNode>();

        public void FindAllSkeletons(ResourceLibary resourceLibary)
        {
            var allFilesInFolder = PackFileLoadHelper.GetAllFilesInDirectory(resourceLibary.PackfileContent, "animations\\skeletons");
            SkeletonList = allFilesInFolder.Where(x => x.FileExtention == "anim").ToList();
        }

        void OnSkeletonSelected()
        {
            SelectedSkeletonBonesFlattened.Clear();
            if (SelectedItem != null)
            {
                SkeletonFile = Create(new ByteChunk(SelectedItem.Data));
                SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = -1, BoneName = ""});
                foreach (var bone in SkeletonFile.Bones)
                    SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = bone.Id, BoneName = bone.Name });
            }

            SelectionChanged?.Invoke(SelectedItem);
        }
    }

    public class AnimationSplicerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        SkeletonElement _targetSkeletonNode;
        AnimationPlayerViewModel _animationPlayer;

        bool _isSelected;
        public bool IsSelected { get { return _isSelected; } set { SetAndNotify(ref _isSelected, value); IsInFocus(IsSelected); } }

        public FilterableAnimationsViewModel TargetAnimation { get; set; } = new FilterableAnimationsViewModel();
        public FilterableSkeletonsViewModel ExternalSkeleton { get; set; } = new FilterableSkeletonsViewModel();
        public FilterableAnimationsViewModel ExternalAnimation { get; set; } = new FilterableAnimationsViewModel();

        ObservableCollection<MappableSkeletonBone> _targetSkeletonBones;
        public ObservableCollection<MappableSkeletonBone> TargetSkeletonBones { get { return _targetSkeletonBones; } set{ SetAndNotify(ref _targetSkeletonBones, value);}}


        public ICommand ForceComputeCommand { get; set; } 


        public AnimationSplicerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _targetSkeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;

            TargetAnimation.SelectionChanged += AnimationsForTargetSkeleton_SelectionChanged;
            ExternalSkeleton.SelectionChanged += PossibleExternalSkeletons_SelectionChanged;
            ExternalAnimation.SelectionChanged += AnimationsForExternalSkeleton_SelectionChanged;

            // Initial init
            TargetAnimation.FindAllAnimations(_resourceLibary, _targetSkeletonNode.SkeletonFile.Header.SkeletonName);
            ExternalSkeleton.FindAllSkeletons(_resourceLibary);
            TargetSkeletonBones = MappableSkeletonBone.Create(_targetSkeletonNode.SkeletonFile.Bones);

            ForceComputeCommand = new RelayCommand(Rebuild);

            // Temp - Populate with debug data. 
            TargetAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
            ExternalSkeleton.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid07.anim");
            ExternalAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid07\club_and_blowpipe\missile_actions\hu7_clbp_aim_idle_01.anim");

            
        }


        class MappingItem
        { 
            public string OriginalName { get; set; }
            public int OriginalId { get; set; }

            public string NewName { get; set; }
            public int NewId { get; set; }
        }

        List<int> RemapListTest(List<int> mappingList, List<MappingItem> animMapping)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < mappingList.Count; i++)
            {
                var orgBoneId = mappingList[i];
                var mappingItem = animMapping.FirstOrDefault(x=>x.NewId == orgBoneId);
                if(mappingItem != null)
                    output.Add(mappingItem.OriginalId);

            }
            return output;
        }

        void Rebuild()
        {
            if (ExternalAnimation.SelectedItem == null)
                return;

            var currentsSkeltonBoneCount = _targetSkeletonNode.Skeleton.BoneCount;
            List<MappingItem> mappingList = new List<MappingItem>();
            FillMappingValue(TargetSkeletonBones, mappingList);


            var externalAnim = AnimationFile.Create(new ByteChunk(ExternalAnimation.SelectedItem.Data));
            var externalAnimationClip = new AnimationClip(externalAnim);

            var outputAnimationFile = new AnimationClip();
            //outputAnimationFile.DynamicRotationMappingID = new List<int>();
            //outputAnimationFile.DynamicTranslationMappingID = new List<int>();
            for (int i = 0; i < currentsSkeltonBoneCount; i++)
            {
                outputAnimationFile.DynamicRotationMappingID.Add(i);
                outputAnimationFile.DynamicTranslationMappingID.Add(i);
            }
        
            for (int frameIndex = 0; frameIndex < externalAnimationClip.DynamicFrames.Count(); frameIndex++)
            {
                var currentOutputFrame = new AnimationClip.KeyFrame();

                for (int boneIndex = 0; boneIndex < currentsSkeltonBoneCount; boneIndex++)
                {
                    var skeletonPosRotation = _targetSkeletonNode.Skeleton.Rotation[boneIndex];
                    var skeletonPosTranslation = _targetSkeletonNode.Skeleton.Translation[boneIndex];

                    var boneToGetAnimDataFrom = mappingList.FirstOrDefault(x => x.OriginalId == boneIndex && x.NewId != -1);
                    if (boneToGetAnimDataFrom != null)
                    {
                        var boneRotationIndexInExternal = externalAnimationClip.DynamicRotationMappingID.IndexOf(boneToGetAnimDataFrom.NewId);
                        bool hasBoneRotationInExternal = boneRotationIndexInExternal != -1;
                        if (hasBoneRotationInExternal)
                        {
                            currentOutputFrame.Rotation.Add(externalAnimationClip.DynamicFrames[frameIndex].Rotation[boneRotationIndexInExternal]);
                        }
                        else
                        { 
                            currentOutputFrame.Rotation.Add(skeletonPosRotation); 
                        }
                        
                         var boneTranslationIndexInExternal = externalAnimationClip.DynamicTranslationMappingID.IndexOf(boneToGetAnimDataFrom.NewId);
                         bool hasBoneTranslationInExternal = boneTranslationIndexInExternal != -1;
                        if (hasBoneTranslationInExternal)
                        {
                            currentOutputFrame.Translation.Add(externalAnimationClip.DynamicFrames[frameIndex].Translation[boneTranslationIndexInExternal]);
                        }
                        else
                        { 
                            currentOutputFrame.Translation.Add(skeletonPosTranslation);
                        }
                    }
                    else
                    {
                        currentOutputFrame.Translation.Add(skeletonPosTranslation);
                        currentOutputFrame.Rotation.Add(skeletonPosRotation);
                    }
                }
                outputAnimationFile.DynamicFrames.Add(currentOutputFrame);
            }
            _animationPlayer.SetAnimationClip(new List<AnimationClip>() { outputAnimationFile }, _targetSkeletonNode.Skeleton);
            return;
        }

        private void PossibleExternalSkeletons_SelectionChanged(PackedFile newSelectedSkeleton)
        {
            ExternalAnimation.SelectedItem = null;
            if (ExternalSkeleton.SelectedItem != null)
            {
                ExternalAnimation.FindAllAnimations(_resourceLibary, ExternalSkeleton.SkeletonFile.Header.SkeletonName);
                PrefilBoneMappingBasedOnName(TargetSkeletonBones, ExternalSkeleton.SelectedSkeletonBonesFlattened);
            }
        }

        private void AnimationsForExternalSkeleton_SelectionChanged(PackedFile newSelectedSkeleton)
        {
            Rebuild();
        }


        private void AnimationsForTargetSkeleton_SelectionChanged(PackedFile newSelectedSkeleton)
        {
            Rebuild();
        }

        void FillMappingValue(IEnumerable<MappableSkeletonBone> nodes, List<MappingItem> mappingList)
        {
            foreach (var node in nodes)
            {
                if (node.MappedBone != null)
                {
                    mappingList.Add(
                        new MappingItem()
                        { 
                            OriginalId = node.BoneIndex,
                            OriginalName = node.BoneName,

                            NewId = node.MappedBone.BoneIndex,
                            NewName = node.MappedBone.BoneName,
                        }
                    );
                }
                FillMappingValue(node.Children, mappingList);
            }
        }

        void ExternalSkeletonSelected()
        {
            ExternalAnimation.SelectedItem = null;
        }

        void PrefilBoneMappingBasedOnName(IEnumerable<MappableSkeletonBone> nodes, ObservableCollection<SkeletonBoneNode> externalBonesList)
        {
            foreach(var node in nodes)
            {
                if (node != null)
                {
                    node.MappedBone = externalBonesList.FirstOrDefault(x => x?.BoneName == node.BoneName);
                    PrefilBoneMappingBasedOnName(node.Children, externalBonesList);
                }
            }
        }

        void ApplyCurrentAnimation()
        {
            //_animationPlayer.SetAnimationClip(null, null);
        }

        void IsInFocus(bool isInFocus)
        {
            if (isInFocus)
                ApplyCurrentAnimation();

            //if (value == true)
            //{
            //    _animationPlayer._animationNode.AnimationPlayer.GoblinSkeleton = _skeletonNode.Skeleton;
            //
            //    _animationPlayer._animationNode.AnimationPlayer.SkinkSkeleton = new Viewer.Animation.Skeleton(_externalSkeletonFile);
            //    _animationPlayer._animationNode.AnimationPlayer.SkinAnimFile = AnimationFile.Create(new ByteChunk(SelectedAnimationForExternalSkeleton.Data));
            //    _animationPlayer._animationNode.AnimationPlayer.MappingValues = new List<int>(_skeletonNode.Skeleton.BoneCount);
            //    for (int i = 0; i < _skeletonNode.Skeleton.BoneCount; i++)
            //        _animationPlayer._animationNode.AnimationPlayer.MappingValues.Add(-1);
            //
            //    FillMappingValue(Bones, _animationPlayer._animationNode.AnimationPlayer.MappingValues);
            //}
        }


       
    }


    public class MappableSkeletonBone : NotifyPropertyChangedImpl
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
        public ObservableCollection<MappableSkeletonBone> Children { get; set; } = new ObservableCollection<MappableSkeletonBone>();


        public static ObservableCollection<MappableSkeletonBone> Create(BoneInfo[] bones)
        {
           var output = new ObservableCollection<MappableSkeletonBone>();
            foreach (var bone in bones)
            {
                if (bone.ParentId == -1)
                {
                    output.Add(CreateNode(bone));
                }
                else
                {
                    var parentBone = bones[bone.ParentId];
                    var treeParent = GetParent(output, parentBone);

                    if (treeParent != null)
                        treeParent.Children.Add(CreateNode(bone));
                }
            }
            return output;
        }

        static MappableSkeletonBone CreateNode(BoneInfo bone)
        {
            MappableSkeletonBone item = new MappableSkeletonBone
            {
                BoneIndex = bone.Id,
                BoneName = bone.Name// + " [" + bone.Id + "]" + " P[" + bone.ParentId + "]",
            };
            return item;
        }

        static MappableSkeletonBone GetParent(ObservableCollection<MappableSkeletonBone> root, AnimationFile.BoneInfo parentBone)
        {
            foreach (MappableSkeletonBone item in root)
            {
                if (item.BoneIndex == parentBone.Id)
                    return item;

                var result = GetParent(item.Children, parentBone);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
