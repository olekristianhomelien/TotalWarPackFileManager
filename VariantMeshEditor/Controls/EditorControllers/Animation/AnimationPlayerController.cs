using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.Views.EditorViews.AnimationViews;
using Viewer.Animation;

namespace VariantMeshEditor.Controls.EditorControllers.Animation
{
    class AnimationPlayerController
    {
        AnimationPlayerView _viewModel;
        AnimationElement _animationElement;

        public AnimationPlayerController(AnimationElement animationElement)
        {
            _animationElement = animationElement;
        }

        public void PopulateExplorerView(AnimationPlayerView view)
        {
            _viewModel = view;
            CreateAnimationSpeed();

            _viewModel.PlayPauseButton.Click += (sender, e) => OnPlayButtonPressed();
            _viewModel.NextFrameButton.Click += (sender, e) => NextFrame();
            _viewModel.PrivFrameButton.Click += (sender, e) => PrivFrame();
            _viewModel.AnimateInPlaceCheckBox.Click += (sender, e) =>{ _animationElement.AnimationPlayer.AnimateInPlace = _viewModel.AnimateInPlaceCheckBox.IsChecked.Value;};
        }

        public void SetAnimation(AnimationClip clip)
        {
            _viewModel.NoFramesLabel.Content = "/" + clip.KeyFrameCollection.Count();
            SyncAllAnimations();
        }

        void OnPlayButtonPressed()
        {
            var player = _animationElement.AnimationPlayer;
            if (player.IsPlaying)
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

        public void Update()
        {
            if (_viewModel != null)
                _viewModel.CurretFrameText.Text = (_animationElement.AnimationPlayer.CurrentFrame + 1).ToString();
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
