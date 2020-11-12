using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers.Animation;
using VariantMeshEditor.Util;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;
using static CommonDialogs.FilterDialog.FilterUserControl;

namespace VariantMeshEditor.ViewModels
{

    public class AnimationExplorerNodeViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<AnimationExplorerNodeViewModel>();
        AnimationExplorerViewModel Parent { get; set; }
        public AnimationFile AnimationFile { get; set; }

        public delegate void AnimationChanged();
        public event AnimationChanged OnAnimationChanged;

        public ICommand RemoveCommand { get; set; }

        #region Properties
        #region Frame properties
        bool _hasStaticFrame = false;
        public bool HasStaticFrame
        {
            get { return _hasStaticFrame; }
            set { SetAndNotify(ref _hasStaticFrame, value); }
        }

        bool _isStaticFrameEnabled = false;
        public bool IsStaticFrameEnabled
        {
            get { return _isStaticFrameEnabled; }
            set { SetAndNotify(ref _isStaticFrameEnabled, value); }
        }

        bool _hasDynamicFrames = false;
        public bool HasDynamicFrames
        {
            get { return _hasDynamicFrames; }
            set { SetAndNotify(ref _hasDynamicFrames, value); }
        }

        bool _isDynamicFramesEnabled = false;
        public bool IsDynamicFramesEnabled
        {
            get { return _isDynamicFramesEnabled; }
            set { SetAndNotify(ref _isDynamicFramesEnabled, value); }
        }

        string _dynamicFramesText = "Dynamic[0]";
        public string DynamicFramesText
        {
            get { return _dynamicFramesText; }
            set { SetAndNotify(ref _dynamicFramesText, value); }
        }
        #endregion

        #region Animation properties
        public bool IsMainAnimation { get; set; } = false;

        bool _useAnimation = true;
        public bool UseAnimation
        {
            get { return _useAnimation; }
            set { SetAndNotify(ref _useAnimation, value); }
        }

        string _animationName;
        public string AnimationName
        {
            get { return _animationName; }
            set { SetAndNotify(ref _animationName, value); }
        }

        string _skeletonName;
        public string SkeletonName
        {
            get { return _skeletonName; }
            set { SetAndNotify(ref _skeletonName, value); }
        }

        string _animationVersion;
        public string AnimationVersion
        {
            get { return _animationVersion; }
            set { SetAndNotify(ref _animationVersion, value); }
        }
        #endregion

        #region Filter properties
        bool _onlyDisplayAnimationsForCurrentSkeleton = true;
        public bool OnlyDisplayAnimationsForCurrentSkeleton
        {
            get { return _onlyDisplayAnimationsForCurrentSkeleton; }
            set
            {
                SetAndNotify(ref _onlyDisplayAnimationsForCurrentSkeleton, value);
                if (value)
                    FilterList = Parent.AnimationFilesForSkeleton;
                else
                    FilterList = Parent.AnimationFiles;
            }
        }

        List<PackedFile> _filterList;
        public List<PackedFile> FilterList { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }

        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }
        #endregion

        #region Error properties
        public bool HasErrorMessage { get { return !string.IsNullOrWhiteSpace(ErrorMessage); } }

        string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                SetAndNotify(ref _errorMessage, value);
                NotifyPropertyChanged(nameof(HasErrorMessage));
            }
        }
        #endregion

        #region SelectedFile propterties
        public event ValueChangedDelegate<PackedFile> OnSelectedAnimationChanged;
        PackedFile _selectedAnimationPackFile;
        public PackedFile SelectedAnimationPackFile
        {
            get { return _selectedAnimationPackFile; }
            set { SetAndNotify(ref _selectedAnimationPackFile, value, OnSelectedAnimationChanged); }
        }
        #endregion
        #endregion

        public AnimationExplorerNodeViewModel(AnimationExplorerViewModel parent)
        {
            Parent = parent;
            OnSelectedAnimationChanged += LoadAnimation;
            OnlyDisplayAnimationsForCurrentSkeleton = true;

            PropertyChanged += Model_PropertyChanged;
            RemoveCommand = new RelayCommand(OnRemoveButtonClicked);
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var attributesThatTriggerAnimationUpdate = new string[]
            { 
                nameof(IsStaticFrameEnabled), 
                nameof(IsDynamicFramesEnabled),
                nameof(UseAnimation)
            };

            var foundAttr = attributesThatTriggerAnimationUpdate.FirstOrDefault(x=>x == e.PropertyName);
            if(foundAttr != null)
                OnAnimationChanged?.Invoke();
        }

        void OnRemoveButtonClicked()
        {
            if(MessageBox.Show("Are you sure you want to delete this animation", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Parent.AnimationList.Remove(this);
        }

        void LoadAnimation(PackedFile file)
        {
            if (file == null)
            {
                AnimationName = "";
                SkeletonName = "";
                AnimationVersion = "";
                return;
            }

            try
            {
                AnimationFile = AnimationFile.Create(new ByteChunk(file.Data));

                if (IsMainAnimation)
                    AnimationName  = "Main animation : " + file.Name;
                else
                    AnimationName = "Sub animation : " + file.Name;

                SkeletonName = AnimationFile.Header.SkeletonName;
                AnimationVersion = AnimationFile.Header.AnimationType.ToString();

                DynamicFramesText = $"Dynamic[{AnimationFile.DynamicFrames.Count}]";
                HasDynamicFrames = AnimationFile.DynamicFrames.Count != 0;
                IsDynamicFramesEnabled = HasDynamicFrames;

                HasStaticFrame = AnimationFile.StaticFrame != null;
                IsStaticFrameEnabled = HasStaticFrame;

                if (!IsMainAnimation && HasDynamicFrames)
                    ErrorMessage = "Only the main animation can have dynamic frames.";
                else
                    ErrorMessage = null;
                OnAnimationChanged?.Invoke();
            }
            catch (Exception exception)
            {
                var error = $"Error loading skeleton {file.FullPath}:{exception.Message}";
                ErrorMessage = error;
                _logger.Error(error);
            }
        }
    }

    public class AnimationExplorerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonNode;
        AnimationPlayer _animationPlayer;
        public ICommand AddNewAnimationCommand { get; set; }


        public List<PackedFile> AnimationFiles { get; set; } = new List<PackedFile>();
        public List<PackedFile> AnimationFilesForSkeleton { get; set; } = new List<PackedFile>();

        public AnimationExplorerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayer animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _skeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;


            FindAllAnimations();
            AddNewAnimationNode(true);

            AddNewAnimationCommand = new RelayCommand(() => { AddNewAnimationNode(); });
        }

        private void OnAnimationChanged()
        {
            var animationFiles = AnimationList
                .Where(x => x.UseAnimation == true && x.HasErrorMessage == false && x.AnimationFile != null)
                .Select(x => x.AnimationFile);

            if (animationFiles.Any())
            {
               AnimationClip clip = AnimationClip.Create(animationFiles.ToArray(), _skeletonNode.Skeleton);
                _animationPlayer.SetAnimation(clip);
               //_playerController.SetAnimation(clip);
            }
            else
            {
                _animationPlayer.SetAnimation(null);
            }
        }

        void AddNewAnimationNode(bool isMainAnimation = false)
        {
            var node = new AnimationExplorerNodeViewModel(this);
            node.OnAnimationChanged += OnAnimationChanged;
            node.IsMainAnimation = isMainAnimation;

            AnimationList.Add(node);
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

        public ObservableCollection<AnimationExplorerNodeViewModel> AnimationList { get; set; } = new ObservableCollection<AnimationExplorerNodeViewModel>();
    }














    public class AnimationElement : FileSceneElement
    {
        public AnimationExplorerViewModel AnimationExplorer { get; set; }

        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();


        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation")
        {
            ApplyElementCheckboxVisability = Visibility.Hidden;
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this);
            if (skeleton.Count == 1)
            {
                AnimationExplorer = new AnimationExplorerViewModel(resourceLibary, skeleton.First(), AnimationPlayer);
            }
        }

        protected override void UpdateNode(GameTime time)
        {
            AnimationPlayer.Update(time);
            //DisplayName = "Animation - " + _controller.GetCurrentAnimationName();
            //_controller.Update();
        }
    }
}
