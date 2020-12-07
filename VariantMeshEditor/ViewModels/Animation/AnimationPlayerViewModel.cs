using CommonDialogs.Common;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.ViewModels.VariantMesh;
using Viewer.Animation;

namespace VariantMeshEditor.ViewModels.Animation
{

    public class AnimationPlayerViewModel : NotifyPropertyChangedImpl
    {

        public AdvancedAnimationSettingsModel AdvanceSettings { get; set; }

        int _currentFrame;
        public int CurrentFrame { get { return _currentFrame; } set { SetAndNotify(ref _currentFrame, value); } }


        int _maxFrames;
        public int MaxFames { get { return _maxFrames; } set { SetAndNotify(ref _maxFrames, value); } }



        public List<AnimationSpeed> PossibleAnimationSpeeds { get; set; }


        AnimationSpeed _selectedAnimationSpeed;
        public AnimationSpeed SelectedAnimationSpeed { get{ return _selectedAnimationSpeed; } set { SetAndNotify(ref _selectedAnimationSpeed, value); HandleAnimationSpeedChanged(); } }


        public ICommand PausePlayCommand { get; set; }
        public ICommand NextFrameCommand { get; set; }
        public ICommand PrivFrameCommand { get; set; }

        public ICommand FirstFrameCommand { get; set; }
        public ICommand LastFrameCommand { get; set; }


        public AnimationElement _animationNode; // <--- Temp publc
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

            FirstFrameCommand = new RelayCommand(OnFirstFrame);
            LastFrameCommand = new RelayCommand(OnLastFrame);

            AdvanceSettings = new AdvancedAnimationSettingsModel(animationNode.Parent, _animationNode.AnimationPlayer);
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
            _animationNode.AnimationPlayer.CurrentFrame++;
        }

        void OnPrivFrame()
        {
            _animationNode.AnimationPlayer.Pause();
            _animationNode.AnimationPlayer.CurrentFrame--;
        }

        void OnFirstFrame()
        {
            _animationNode.AnimationPlayer.Pause();
            _animationNode.AnimationPlayer.CurrentFrame = 0;
        }

        void OnLastFrame()
        {
            _animationNode.AnimationPlayer.Pause();
            _animationNode.AnimationPlayer.CurrentFrame = _animationNode.AnimationPlayer.FrameCount();
        }

        void HandleAnimationSpeedChanged()
        {
            _animationNode.AnimationPlayer.FrameRate = SelectedAnimationSpeed.FrameRate;
        }

        //void HandleSyncAllAnimations()
        //{
        //    var root = SceneElementHelper.GetRoot(_animationNode);
        //    List<AnimationElement> animationItems = new List<AnimationElement>();
        //    SceneElementHelper.GetAllChildrenOfType<AnimationElement>(root, animationItems);
        //    animationItems.Remove(_animationNode);
        //
        //    foreach (var animationItem in animationItems)
        //    {
        //        animationItem.AnimationPlayer.CurrentFrame = _animationNode.AnimationPlayer.CurrentFrame;
        //        if (_animationNode.AnimationPlayer.IsPlaying)
        //            animationItem.AnimationPlayer.Play();
        //        else
        //            animationItem.AnimationPlayer.Pause();
        //    }
        //}

        //void HandleAnimateInPlace()
        //{
        //    _animationNode.AnimationPlayer.AnimateInPlace = AnimateInPlace;
        //}

        public void Update()
        {
            CurrentFrame = _animationNode.AnimationPlayer.CurrentFrame+1;
        }



        public void SetAnimationClip(IEnumerable<AnimationClip> animationFiles, Viewer.Animation.Skeleton skeleton)
        {
            if (animationFiles == null || animationFiles.Any() == false)
            {
                _animationNode.AnimationPlayer.SetAnimation(null, skeleton);
            }
            else
            {
                _animationNode.AnimationPlayer.SetAnimationArray(animationFiles.ToList(), skeleton);
            }

            MaxFames = _animationNode.AnimationPlayer.FrameCount();
            CurrentFrame = _animationNode.AnimationPlayer.CurrentFrame+1;
        }


        public class AnimationSpeed
        {
            public double FrameRate { get; set; }
            public string DisplayName { get; set; }
        }
    }


    public class AdvancedAnimationSettingsModel : NotifyPropertyChangedImpl
    {
        #region Sync Animation
        public enum SyncModeEnum
        {
            Default,
            Other,
            SomethingElse
        }

        private bool _syncAllAnimations;

        public bool SyncAllAnimations { get { return _syncAllAnimations; } set { SetAndNotify(ref _syncAllAnimations, value); } }

        public List<SyncModeEnum> PossibleSyncModes { get { return  Enum.GetValues(typeof(SyncModeEnum)).Cast<SyncModeEnum>().ToList(); } }

        private SyncModeEnum _selectedSyncMode;
        public SyncModeEnum SelectedSyncMode { get { return _selectedSyncMode; } set { SetAndNotify(ref _selectedSyncMode, value); } }
        #endregion

        #region Freeze Animation
        public bool FreezeAnimationRoot { get { return _animationPlayer.Settings.FreezeAnimationRoot; } set { _animationPlayer.Settings.FreezeAnimationRoot = value; NotifyPropertyChanged();} }

        public bool FreezeAnimationBone { get { return _animationPlayer.Settings.FreezeAnimationBone; } set { _animationPlayer.Settings.FreezeAnimationBone = value; NotifyPropertyChanged(); } }


        public ObservableCollection<AnimationFile.BoneInfo> PosibleBonesToFreeze { get; set; } = new ObservableCollection<AnimationFile.BoneInfo>();

        AnimationFile.BoneInfo _selectedBoneToFreeze;
        public AnimationFile.BoneInfo SelectedBoneToFreeze 
        { 
            get { return _selectedBoneToFreeze; } 
            set 
            { 
                SetAndNotify(ref _selectedBoneToFreeze, value);
                if (_selectedBoneToFreeze != null)
                    _animationPlayer.Settings.FreezeAnimationBoneIndex = _selectedBoneToFreeze.Id;
                else
                    _animationPlayer.Settings.FreezeAnimationBoneIndex = -1;
            } 
        }

        #endregion

        #region Translation offset
        public bool UseTranslationOffset { get { return _animationPlayer.Settings.UseTranslationOffset; } set { _animationPlayer.Settings.UseTranslationOffset = value; NotifyPropertyChanged(); } }

        string _translationOffsetX = "0";
        public string TranslationOffsetX
        {
            get { return _translationOffsetX; }
            set
            {
                SetAndNotify(ref _translationOffsetX, value);
                if (string.IsNullOrWhiteSpace(value))
                    _animationPlayer.Settings.TranslationOffsetX = 0;
                if (float.TryParse(value, out float result))
                    _animationPlayer.Settings.TranslationOffsetX = result;
            }
        }

        string _translationOffsetY = "0";
        public string TranslationOffsetY
        {
            get { return _translationOffsetY; }
            set
            {
                SetAndNotify(ref _translationOffsetY, value);
                if (string.IsNullOrWhiteSpace(value))
                    _animationPlayer.Settings.TranslationOffsetY = 0;
                if (float.TryParse(value, out float result))
                    _animationPlayer.Settings.TranslationOffsetY = result;
            }
        }

        string _translationOffsetZ = "0";
        public string TranslationOffsetZ
        {
            get { return _translationOffsetZ; }
            set
            {
                SetAndNotify(ref _translationOffsetZ, value);
                if (string.IsNullOrWhiteSpace(value))
                    _animationPlayer.Settings.TranslationOffsetZ = 0;
                if (float.TryParse(value, out float result))
                    _animationPlayer.Settings.TranslationOffsetZ = result;
            }
        }
        #endregion

        #region Rotation offset
        public bool UseRotationOffset { get { return _animationPlayer.Settings.UseRotationOffset; } set { _animationPlayer.Settings.UseRotationOffset = value; NotifyPropertyChanged(); } }
        
        string _rotationOffsetX = "0";
        public string RotationOffsetX 
        { 
            get { return _rotationOffsetX; } 
            set 
            {
                SetAndNotify(ref _rotationOffsetX, value);
                if (string.IsNullOrWhiteSpace(value))
                    _animationPlayer.Settings.RotationOffsetX = 0;
                if (float.TryParse(value, out float result))
                    _animationPlayer.Settings.RotationOffsetX = result;
            } 
        }

        string _rotationOffsetY = "0";
        public string RotationOffsetY
        {
            get { return _rotationOffsetY; }
            set
            {
                SetAndNotify(ref _rotationOffsetY, value);
                if (string.IsNullOrWhiteSpace(value))
                    _animationPlayer.Settings.RotationOffsetY = 0;
                if (float.TryParse(value, out float result))
                    _animationPlayer.Settings.RotationOffsetY = result;
            }
        }

        string _rotationOffsetZ = "0";
        public string RotationOffsetZ
        {
            get { return _rotationOffsetZ; }
            set
            {
                SetAndNotify(ref _rotationOffsetZ, value);
                if (string.IsNullOrWhiteSpace(value))
                    _animationPlayer.Settings.RotationOffsetZ = 0;
                if (float.TryParse(value, out float result))
                    _animationPlayer.Settings.RotationOffsetZ = result;
            }
        }

        #endregion

        #region Snap
        public bool UseAnimationSnap { get { return _animationPlayer.Settings.UseAnimationSnap; } set { _animationPlayer.Settings.UseAnimationSnap = value; NotifyPropertyChanged(); } }
        public bool OnlySnapTranslations { get { return _animationPlayer.Settings.OnlySnapTranslations; } set { _animationPlayer.Settings.OnlySnapTranslations = value; NotifyPropertyChanged(); } }

        public ICommand PopulateSnapBoneListCommand { get; set; }
        public ICommand PopulateSnapMeshListCommand { get; set; }

        public ObservableCollection<FileSceneElement> PossibleSnapMeshList { get; set; } = new ObservableCollection<FileSceneElement>();

        FileSceneElement _selectedSnapMesh;
        public FileSceneElement SelectedSnapMesh 
        { 
            get { return _selectedSnapMesh; } 
            set 
            {
                SetAndNotify(ref _selectedSnapMesh, value);
                _animationPlayer.ExternalAnimationRef.ExternalPlayer = SceneElementHelper.GetFirstChild<AnimationElement>(_selectedSnapMesh)?.AnimationPlayer;
            } 
        }

        AnimationFile.BoneInfo _selectedSnapBone;
        public AnimationFile.BoneInfo SelectedSnapBone 
        { 
            get { return _selectedSnapBone; } 
            set 
            { 
                SetAndNotify(ref _selectedSnapBone, value);
                if (_selectedSnapBone == null)
                    _animationPlayer.ExternalAnimationRef.ExternalBoneIndex = -1;
                else
                    _animationPlayer.ExternalAnimationRef.ExternalBoneIndex = _selectedSnapBone.Id;
            } 
        }

        public ObservableCollection<AnimationFile.BoneInfo> PossibleSnapBones { get; set; } = new ObservableCollection<AnimationFile.BoneInfo>();

        #endregion

        FileSceneElement _parent;
        AnimationPlayer _animationPlayer;

        public AdvancedAnimationSettingsModel(FileSceneElement parent, AnimationPlayer animationPlayer)
        {
            _parent = parent;
            _animationPlayer = animationPlayer;

            PopulateSnapBoneListCommand = new RelayCommand(PopulateBoneList);
            PopulateSnapMeshListCommand = new RelayCommand(PopulateMeshList);

            var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(parent);
            if (skeleton != null)
            {
                foreach (var bone in skeleton?.SkeletonFile.Bones)
                    PosibleBonesToFreeze.Add(bone);

                SelectedBoneToFreeze = PosibleBonesToFreeze.FirstOrDefault(x => x.Name == "root");
            }
        }

        void PopulateBoneList()
        {
            PossibleSnapBones.Clear();
            if (SelectedSnapMesh != null)
            {
                var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(SelectedSnapMesh);
                if (skeleton != null)
                {
                    foreach (var bone in skeleton?.SkeletonFile.Bones)
                        PossibleSnapBones.Add(bone);
                }
            }
        }

        void PopulateMeshList()
        {
            var root = SceneElementHelper.GetRoot(_parent);
            PossibleSnapMeshList.Clear();
            PossibleSnapMeshList.Add(null);
            foreach (var item in root.Children)
            {
                if (item != _parent)
                    PossibleSnapMeshList.Add(item);
            }

            SelectedSnapBone = null;
        }
    }
}
