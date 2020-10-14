using Common;
using CommonDialogs.FilterDialog;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.AnimationViews;
using VariantMeshEditor.Views.EditorViews.Util;
using Viewer.Animation;
using Viewer.Scene;

namespace VariantMeshEditor.Controls.EditorControllers.Animation
{
    class AnimationExplorerController
    {
        ILogger _logger = Logging.Create<AnimationExplorerController>();

        AnimationPlayerController _playerController;

        AnimationEditorView _viewModel;
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonElement;
        AnimationElement _animationElement;
        
        string _currentAnimationName;

        List<AnimationListItem> _animationFiles = new List<AnimationListItem>();
        List<AnimationListItem> _animationsValidForSkeleton = new List<AnimationListItem>();

        public AnimationExplorerController( ResourceLibary resourceLibary, AnimationElement animationElement, SkeletonElement skeletonElement, AnimationPlayerController playerController)
        {
            _resourceLibary = resourceLibary;
            _skeletonElement = skeletonElement;
            _animationElement = animationElement;
            _playerController = playerController;
        }

        public void PopulateExplorerView(AnimationEditorView viewModel)
        {
            _viewModel = viewModel;
            _viewModel.AnimationExplorer.CreateNewAnimationButton.Click += (sender, e) => CreateAnimationExplorer();

            FindAllAnimations();
            CreateAnimationExplorer(true);
        }


        public void CreateTestData(AnimationEditorView viewModel)
        {
            var idle = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\sword_and_shield\stand\hu1_sws_stand_idle_05.anim");
            var hand = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\hands\hu1_hand_pose_clench.anim");

            var mainAnimation = _viewModel.AnimationExplorer.Explorers[0];
            LoadAnimation(mainAnimation, idle);

            var handExplorer = CreateAnimationExplorer();
            LoadAnimation(handExplorer, hand);
        }

        AnimationExplorerItemView CreateAnimationExplorer(bool isMainAnimation = false)
        {
            var explorer = _viewModel.AnimationExplorer.CreateAnimationExplorer();
            explorer.SkeletonName.Text = "";
            explorer.IsMainAnimation = isMainAnimation;
            if (isMainAnimation)
                explorer.RemoveButton.Visibility = System.Windows.Visibility.Collapsed;
            else
                explorer.RemoveButton.Click += (sender, e) => _viewModel.AnimationExplorer.RemoveAnimationExplorer(explorer);

            explorer.FilterBoxGrid.Visibility = System.Windows.Visibility.Collapsed;
            explorer.ErrorBar.Visibility = System.Windows.Visibility.Collapsed;

            explorer.FilterBox.OnItemDoubleClicked += (sender, e) => HandleAnimationDoubleClicked(explorer);
            explorer.FilterBox.OnItemSelected +=(sender, e) => HandleAnimationSelected(explorer);
            explorer.BrowseAnimationButton.Click += (sender, e) => BrowseForAnimation(explorer);
            explorer.DynamicFrameCheckbox.Click += (sender, e) => { _animationElement.AnimationPlayer.ApplyDynamicFrames = explorer.DynamicFrameCheckbox.IsChecked.Value; };
            explorer.StaticFramesCheckbox.Click += (sender, e) => { _animationElement.AnimationPlayer.ApplyDynamicFrames = explorer.StaticFramesCheckbox.IsChecked.Value; };

            return explorer;
        }


        void HandleAnimationDoubleClicked(AnimationExplorerItemView explorer)
        {
            if (HandleAnimationSelected(explorer))
            {
                explorer.FilterBoxGrid.Visibility = System.Windows.Visibility.Collapsed;
                explorer.BrowseAnimationButton.Content = "Browse";
            }
        }

        bool HandleAnimationSelected(AnimationExplorerItemView explorer)
        {
            var selectedItem = explorer.FilterBox.GetSelectedItem() as AnimationListItem;
            if(selectedItem != null)
                return LoadAnimation(explorer, selectedItem.File);
            return false;
        }

        void BrowseForAnimation(AnimationExplorerItemView explorer)
        {
            if (explorer.FilterBoxGrid.Visibility == System.Windows.Visibility.Visible)
            {
                explorer.FilterBoxGrid.Visibility = System.Windows.Visibility.Collapsed;
                explorer.BrowseAnimationButton.Content = "Browse";
            }
            else
            {
                explorer.BrowseAnimationButton.Content = "Hide";
                FindAllAnimationsForSkeleton();
                explorer.FilterBoxGrid.Visibility = System.Windows.Visibility.Visible;
                explorer.FilterBox.SetItems(_animationsValidForSkeleton, 
                    (IEnumerable<object> orgList) => { return _animationFiles; }, 
                    true, 
                    "Only list animations for current skeleton");
               
            }
        }

        bool LoadAnimation(AnimationExplorerItemView explorer, PackedFile file)
        {
            try
            {
                explorer.AnimationFile = AnimationFile.Create(new ByteChunk(file.Data));

                if (explorer.IsMainAnimation)
                {
                    _currentAnimationName = file.Name;
                    explorer.AnimationExpanderName.Text = "Main animation : " + _currentAnimationName;
                }
                else
                {
                    explorer.AnimationExpanderName.Text = "Sub animation : " + file.Name;
                }

                explorer.SkeletonName.Text = explorer.AnimationFile.Header.SkeletonName;
                explorer.AnimationFileNameText.Text = file.FullPath;
                explorer.AnimationType.Text = explorer.AnimationFile.Header.AnimationType.ToString();

                explorer.DynamicFrameCheckbox.IsEnabled = explorer.AnimationFile.DynamicFrames.Count != 0;
                explorer.DynamicFrameCheckbox.IsChecked = explorer.DynamicFrameCheckbox.IsEnabled;
                explorer.DynamicFrameCheckbox.Content = $"Dynamic[{explorer.AnimationFile.DynamicFrames.Count}]";

                explorer.StaticFramesCheckbox.IsEnabled = explorer.AnimationFile.StaticFrame != null;
                explorer.StaticFramesCheckbox.IsChecked = explorer.StaticFramesCheckbox.IsEnabled;

                UpdateCurrentAnimation();
                return true;
            }
            catch (Exception exception)
            {
                var error = $"Error loading skeleton {file.FullPath}:{exception.Message}";
                explorer.ErrorBar.Visibility = System.Windows.Visibility.Visible;
                explorer.ErrorText.Background = new SolidColorBrush(Colors.Red);
                explorer.ErrorText.Text = error;
                _logger.Error(error);
                return false;
            }
        }

        void UpdateCurrentAnimation()
        {
            var animationFiles = _viewModel.AnimationExplorer.Explorers.Where(x=>x.UseAnimationCheckbox.IsChecked== true).Select(x => x.AnimationFile).ToArray();
            animationFiles = animationFiles.Where(x => x != null).ToArray();
            if (animationFiles.Length != 0)
            {
                AnimationClip clip = AnimationClip.Create(animationFiles, _skeletonElement.Skeleton);
                _animationElement.AnimationPlayer.SetAnimation(clip);
                _playerController.SetAnimation(clip);
            }
        }

        void FindAllAnimations()
        { 
            var allAnimationFiles = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "anim");
            foreach (var file in allAnimationFiles)
                _animationFiles.Add(new AnimationListItem() { File = file });
        }

        void FindAllAnimationsForSkeleton()
        {
            if (_animationsValidForSkeleton.Count == 0)
            {
                foreach (var item in _animationFiles)
                {
                    var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(item.File.Data)).SkeletonName;
                    if (animationSkeletonName == _skeletonElement.SkeletonFile.Header.SkeletonName)
                        _animationsValidForSkeleton.Add(new AnimationListItem() { File = item.File });
                }
            }
        }

        public string GetCurrentAnimationName()
        {
            return _currentAnimationName;
        }

        class AnimationListItem
        { 
            public PackedFile File { get; set; }
            public override string ToString()
            {
                return File.FullPath;
            }
        }
    }
}
