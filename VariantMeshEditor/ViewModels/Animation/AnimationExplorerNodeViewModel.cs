using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static CommonDialogs.FilterDialog.FilterUserControl;

namespace VariantMeshEditor.ViewModels.Animation
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

            var foundAttr = attributesThatTriggerAnimationUpdate.FirstOrDefault(x => x == e.PropertyName);
            if (foundAttr != null)
                OnAnimationChanged?.Invoke();
        }

        void OnRemoveButtonClicked()
        {
            if (MessageBox.Show("Are you sure you want to delete this animation", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                AnimationFile = AnimationFile.Create(file);

                if (IsMainAnimation)
                    AnimationName = "Main animation : " + file.Name;
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
}
