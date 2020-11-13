﻿using CommonDialogs.Common;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Windows.Input;
using VariantMeshEditor.Util;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class AnimationPlayerViewModel : NotifyPropertyChangedImpl
    {
        int _currentFrame;
        public int CurrentFrame { get { return _currentFrame; } set { SetAndNotify(ref _currentFrame, value); } }


        int _maxFrames;
        public int MaxFames { get { return _maxFrames; } set { SetAndNotify(ref _maxFrames, value); } }


        bool _animateInPlace;
        public bool AnimateInPlace { get { return _animateInPlace; } set { SetAndNotify(ref _animateInPlace, value); HandleAnimateInPlace(); } }


        bool _syncAllAnimations;
        public bool SyncAllAnimations { get { return _syncAllAnimations; } set { SetAndNotify(ref _syncAllAnimations, value); HandleSyncAllAnimations(); } }


        public List<AnimationSpeed> PossibleAnimationSpeeds { get; set; }


        AnimationSpeed _selectedAnimationSpeed;
        public AnimationSpeed SelectedAnimationSpeed { get{ return _selectedAnimationSpeed; } set { SetAndNotify(ref _selectedAnimationSpeed, value); HandleAnimationSpeedChanged(); } }


        public ICommand PausePlayCommand { get; set; }
        public ICommand NextFrameCommand { get; set; }
        public ICommand PrivFrameCommand { get; set; }


        AnimationElement _animationNode;
        public AnimationPlayerViewModel(AnimationElement animationNode)
        {
            _animationNode = animationNode;

            PossibleAnimationSpeeds = new List<AnimationSpeed>()
            {
                new AnimationSpeed(){ FrameRate = 20.0 / 1000.0, DisplayName = "1x" },
                new AnimationSpeed(){ FrameRate = (20.0 / 0.5) / 1000.0, DisplayName = "0.5x" },
                new AnimationSpeed(){ FrameRate = (20.0 / 0.1) / 1000.0, DisplayName = "0.1x" },
                new AnimationSpeed(){ FrameRate = (20.0 / 0.01) / 1000.0, DisplayName = "0.01x" },
            };

            _selectedAnimationSpeed = PossibleAnimationSpeeds[0];

            PausePlayCommand = new RelayCommand(OnPlayPause);
            NextFrameCommand = new RelayCommand(OnNextFrame);
            PrivFrameCommand = new RelayCommand(OnPrivFrame);
        }

        void OnAnimationClipChanged()
        {
            MaxFames = _animationNode.AnimationPlayer.FrameCount();
            CurrentFrame = _animationNode.AnimationPlayer.CurrentFrame;
        }

        void OnPlayPause()
        {
            var player = _animationNode.AnimationPlayer;
            if (player.IsPlaying)
                player.Pause();
            else
                player.Play();
        }

        void OnNextFrame()
        {
            _animationNode.AnimationPlayer.Pause();
            _animationNode.AnimationPlayer.CurrentFrame--;
        }

        void OnPrivFrame()
        {
            _animationNode.AnimationPlayer.Pause();
            _animationNode.AnimationPlayer.CurrentFrame--;
        }

        void HandleAnimationSpeedChanged()
        {
            _animationNode.AnimationPlayer.FrameRate = SelectedAnimationSpeed.FrameRate;
        }

        void HandleSyncAllAnimations()
        {
            var root = SceneElementHelper.GetRoot(_animationNode);
            List<AnimationElement> animationItems = new List<AnimationElement>();
            SceneElementHelper.GetAllChildrenOfType<AnimationElement>(root, animationItems);
            animationItems.Remove(_animationNode);

            foreach (var animationItem in animationItems)
            {
                animationItem.AnimationPlayer.CurrentFrame = _animationNode.AnimationPlayer.CurrentFrame;
                if (_animationNode.AnimationPlayer.IsPlaying)
                    animationItem.AnimationPlayer.Play();
                else
                    animationItem.AnimationPlayer.Pause();
            }
        }

        void HandleAnimateInPlace()
        {
            _animationNode.AnimationPlayer.AnimateInPlace = AnimateInPlace;
        }

        public void Update()
        {
            CurrentFrame = _animationNode.AnimationPlayer.CurrentFrame;
        }


        public class AnimationSpeed
        {
            public double FrameRate { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
