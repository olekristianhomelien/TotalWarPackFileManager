using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Serilog;
using System;
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
        ILogger _logger = Logging.Create<AnimationExplorerViewModel>();

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

            AddNewAnimationCommand = new RelayCommand(() => { AddNewAnimationNode(); });
        }

        void ApplyCurrentAnimation()
        {
            var animationFiles = AnimationList
                .Where(x => x.UseAnimation == true && x.HasErrorMessage == false && x.AnimationFile != null)
                .Select(x => x);

            if (animationFiles.Any())
            {
                var animations = animationFiles.Select(x =>
                {
                    var clip = new AnimationClip(x.AnimationFile);
                    clip.UseDynamicFames = x.IsDynamicFramesEnabled;
                    clip.UseStaticFrame = x.IsStaticFrameEnabled;
                    return clip;
                });

                _animationPlayer.SetAnimationClip(animations.ToList(), _skeletonNode.GameSkeleton);
            }
            else
                _animationPlayer.SetAnimationClip(null, _skeletonNode.GameSkeleton);
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
            _logger.Here().Information("Finding all animations");

            AnimationFiles = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "anim");
            _logger.Here().Information("Animations found =" + AnimationFiles.Count());

            foreach (var animation in AnimationFiles)
            {
                try
                {
                    var animationSkeletonName = AnimationFile.GetAnimationHeader(animation).SkeletonName;
                    if (animationSkeletonName == _skeletonNode.SkeletonFile.Header.SkeletonName)
                        AnimationFilesForSkeleton.Add(animation);
                }
                catch (Exception e)
                {
                    _logger.Here().Error("Parsing failed for " + animation.FullPath + "\n" + e.ToString());
                }
            }

            _logger.Here().Information("Finding all done");
        }
    }
}
