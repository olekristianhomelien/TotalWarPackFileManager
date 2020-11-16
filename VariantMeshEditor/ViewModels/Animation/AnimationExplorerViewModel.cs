﻿using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class AnimationExplorerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonNode;
        AnimationPlayerViewModel _animationPlayer;
        public ICommand AddNewAnimationCommand { get; set; }

        public List<PackedFile> AnimationFiles { get; set; } = new List<PackedFile>();
        public List<PackedFile> AnimationFilesForSkeleton { get; set; } = new List<PackedFile>();
        public ObservableCollection<AnimationExplorerNodeViewModel> AnimationList { get; set; } = new ObservableCollection<AnimationExplorerNodeViewModel>();

        bool _isSelected = true;
        public bool IsSelected{ get { return _isSelected; } set { SetAndNotify(ref _isSelected, value); IsInFocus(IsSelected); }}


        public AnimationExplorerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _skeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;

            FindAllAnimations();
            AddNewAnimationNode(true);

            //bool loadDebugData = true;
            //if (loadDebugData)
            //{
            //    var mainAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\sword_and_shield\combat_idles\hu1_sws_combat_idle_02.anim");
            //    AnimationList[0].SelectedAnimationPackFile = mainAnim;
            //
            //    AddNewAnimationNode();
            //    var handAnim = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\hands\hu1_hand_pose_clench.anim");
            //    AnimationList[1].SelectedAnimationPackFile = handAnim;
            //}

            AddNewAnimationCommand = new RelayCommand(() => { AddNewAnimationNode(); });
        }

        void ApplyCurrentAnimation()
        {
            var animationFiles = AnimationList
                .Where(x => x.UseAnimation == true && x.HasErrorMessage == false && x.AnimationFile != null)
                .Select(x => x.AnimationFile);

            if (animationFiles.Any())
                _animationPlayer.SetAnimationClip(animationFiles.ToArray(), _skeletonNode.Skeleton);
            else
                _animationPlayer.SetAnimationClip(null, null);
        }

        void IsInFocus(bool isInFocus)
        {
            if (isInFocus)
                ApplyCurrentAnimation();
        }

        public AnimationExplorerNodeViewModel AddNewAnimationNode(bool isMainAnimation = false)
        {
            var node = new AnimationExplorerNodeViewModel(this);
            node.OnAnimationChanged += ApplyCurrentAnimation;
            node.IsMainAnimation = isMainAnimation;

            AnimationList.Add(node);
            return node;
        }

        void FindAllAnimations()
        {
            AnimationFiles = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "anim");

            foreach (var animation in AnimationFiles)
            {
                var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(animation.Data)).SkeletonName;
                if (animationSkeletonName == _skeletonNode.SkeletonFile.Header.SkeletonName)
                    AnimationFilesForSkeleton.Add(animation);
            }
        }


    }
}
