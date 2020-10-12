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

namespace VariantMeshEditor.Controls.EditorControllers
{
    class AnimationController
    {
        ILogger _logger = Logging.Create<AnimationController>();

        AnimationEditorView _viewModel;
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonElement;
        AnimationElement _animationElement;
        
        string _currentAnimationName;

        List<AnimationListItem> _animationFiles = new List<AnimationListItem>();
        List<AnimationListItem> _animationsValidForSkeleton = new List<AnimationListItem>();

        public AnimationController( ResourceLibary resourceLibary, AnimationElement animationElement, SkeletonElement skeletonElement)
        {
            _resourceLibary = resourceLibary;
            _skeletonElement = skeletonElement;
            _animationElement = animationElement;

            CreateTestData();
        }

        public AnimationEditorView GetView()
        {
            if (_viewModel == null)
            {
                _viewModel = new AnimationEditorView();
                _viewModel.CreateNewAnimationButton.Click += (sender, e) => CreateAnimationExplorer();

                _viewModel.PlayPauseButton.Click += (sender, e) => OnPlayButtonPressed();
                _viewModel.NextFrameButton.Click += (sender, e) => NextFrame();
                _viewModel.PrivFrameButton.Click += (sender, e) => PrivFrame();
                _viewModel.AnimateInPlaceCheckBox.Click += (sender, e) => OnAnimationSettingsChanged();

                FindAllAnimations();
                
                CreateAnimationSpeed();
                CreateAnimationExplorer(true);
            }
            return _viewModel;
        }


        void CreateTestData()
        {
            GetView();

            var idle = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\sword_and_shield\stand\hu1_sws_stand_idle_05.anim");
            var hand = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\hands\hu1_hand_pose_clench.anim");

            var mainAnimation = _viewModel.Explorers[0];
            LoadAnimation(mainAnimation, idle);

            var handExplorer = CreateAnimationExplorer();
            LoadAnimation(handExplorer, hand);
        }

        AnimationExplorerView CreateAnimationExplorer(bool isMainAnimation = false)
        {
            var explorer = _viewModel.CreateAnimationExplorer();
            explorer.SkeletonName.Text = "";
            explorer.IsMainAnimation = isMainAnimation;
            if (isMainAnimation)
                explorer.RemoveButton.Visibility = System.Windows.Visibility.Collapsed;
            else
                explorer.RemoveButton.Click += (sender, e) => _viewModel.RemoveAnimationExplorer(explorer);

            explorer.FilterBoxGrid.Visibility = System.Windows.Visibility.Collapsed;
            explorer.ErrorBar.Visibility = System.Windows.Visibility.Collapsed;

            explorer.FilterBox.OnItemDoubleClicked += (sender, e) => HandleAnimationDoubleClicked(explorer);
            explorer.FilterBox.OnItemSelected +=(sender, e) => HandleAnimationSelected(explorer);
            explorer.BrowseAnimationButton.Click += (sender, e) => BrowseForAnimation(explorer);
            explorer.DynamicFrameCheckbox.Click += (sender, e) => OnAnimationSettingsChanged();
            explorer.StaticFramesCheckbox.Click += (sender, e) => OnAnimationSettingsChanged();

            return explorer;
        }


        void HandleAnimationDoubleClicked(AnimationExplorerView explorer)
        {
            if (HandleAnimationSelected(explorer))
            {
                explorer.FilterBoxGrid.Visibility = System.Windows.Visibility.Collapsed;
                explorer.BrowseAnimationButton.Content = "Browse";
            }
        }

        bool HandleAnimationSelected(AnimationExplorerView explorer)
        {
            var selectedItem = explorer.FilterBox.GetSelectedItem() as AnimationListItem;
            if(selectedItem != null)
                return LoadAnimation(explorer, selectedItem.File);
            return false;
        }

        void BrowseForAnimation(AnimationExplorerView explorer)
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
                explorer.FilterBox.SetItems(_animationsValidForSkeleton, GetAllAnimationsFilter, true, "Only list animations for current skeleton");
               
            }
        }

        bool LoadAnimation(AnimationExplorerView explorer, PackedFile file)
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

            var animationFiles = _viewModel.Explorers.Where(x=>x.UseAnimationCheckbox.IsChecked== true).Select(x => x.AnimationFile).ToArray();
            if (animationFiles.Length != 0)
            {
                AnimationClip clip = AnimationClip.Create(animationFiles, _skeletonElement.SkeletonFile, _skeletonElement.Skeleton);
                _animationElement.AnimationPlayer.SetAnimation(clip);
                _viewModel.NoFramesLabel.Content = "/" + clip.KeyFrameCollection.Count();
            }
            

           // _animationElement.AnimationPlayer.UpdatCurrentAnimationSettings(
           //         _viewModel.AnimateInPlaceCheckBox.IsChecked.Value,
           //         _viewModel.Explorers[0].DynamicFrameCheckbox.IsChecked.Value,
           //         _viewModel.Explorers[0].StaticFramesCheckbox.IsChecked.Value);
           //
            SyncAllAnimations();
        }


        IEnumerable<object> GetAllAnimationsFilter(IEnumerable<object> orgList)
        {
            return _animationFiles;
        }

        void OnPlayButtonPressed()
        {
            var player = _animationElement.AnimationPlayer;
            if(player.IsPlaying)
                player.Pause();
            else
                player.Play();

            SyncAllAnimations();
        }

        void NextFrame()
        {
            _animationElement.AnimationPlayer.Pause();
            _animationElement.AnimationPlayer.CurrentFrame++;

            SyncAllAnimations();
        }

        void PrivFrame()
        {
            _animationElement.AnimationPlayer.Pause();
            _animationElement.AnimationPlayer.CurrentFrame--;

            SyncAllAnimations();
        }

        void OnAnimationSettingsChanged()
        {
            return;
            _animationElement.AnimationPlayer.UpdatCurrentAnimationSettings(
                _viewModel.AnimateInPlaceCheckBox.IsChecked.Value, 
                _viewModel.Explorers[0].DynamicFrameCheckbox.IsChecked.Value, 
                _viewModel.Explorers[0].StaticFramesCheckbox.IsChecked.Value);
        }

        void CreateAnimationSpeed()
        {
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = 20.0 / 1000.0, DisplayName = "1x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.5) / 1000.0, DisplayName = "0.5x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.1) / 1000.0, DisplayName = "0.1x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.01) / 1000.0, DisplayName = "0.01x" });
            _viewModel.AnimationSpeedComboBox.SelectedIndex = 0;

            _viewModel.AnimationSpeedComboBox.SelectionChanged += OnAnimationSpeedChanged;
        }

        private void OnAnimationSpeedChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                foreach (AnimationSpeedItem item in e.AddedItems)
                    _animationElement.AnimationPlayer.FrameRate = item.FrameRate;
            }

            SyncAllAnimations();
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

        public void Update()
        {
            if(_viewModel != null)
                _viewModel.CurretFrameText.Text = (_animationElement.AnimationPlayer.CurrentFrame + 1).ToString();
        }

        void SyncAllAnimations()
        {
            var root = SceneElementHelper.GetRoot(_animationElement);
            List<AnimationElement> animationItems = new List<AnimationElement>();
            SceneElementHelper.GetAllChildrenOfType<AnimationElement>(root, animationItems);
            animationItems.Remove(_animationElement);

            foreach (var animationItem in animationItems)
            {
                animationItem.AnimationPlayer.CurrentFrame = _animationElement.AnimationPlayer.CurrentFrame;
                if (_animationElement.AnimationPlayer.IsPlaying)
                    animationItem.AnimationPlayer.Play();
                else
                    animationItem.AnimationPlayer.Pause();
            }
        }

        class AnimationListItem
        { 
            public PackedFile File { get; set; }
            public override string ToString()
            {
                return File.FullPath;
            }
        }

        class AnimationSpeedItem
        {
            public double FrameRate { get; set; }
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}
