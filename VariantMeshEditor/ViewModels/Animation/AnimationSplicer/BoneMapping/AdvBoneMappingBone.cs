using CommonDialogs.Common;
using CommonDialogs.MathViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Services;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping
{
    public class AdvBoneMappingBone : NotifyPropertyChangedImpl
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

        int _parentBoneIndex;
        public int ParentBoneIndex
        {
            get { return _parentBoneIndex; }
            set { SetAndNotify(ref _parentBoneIndex, value); }
        }

        string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetAndNotify(ref _displayName, value); }
        }

        public int ChildNodeCounts { get; set; } = -99; // -99 just to indicate something is wrong, should always be calculated.

        string _mappingDisplayStr;
        public string MappingDisplayStr
        {
            get { return _mappingDisplayStr; }
            set { SetAndNotify(ref _mappingDisplayStr, value); }
        }

        BoneMappingType _mappingType = BoneMappingType.None;
        public BoneMappingType MappingType
        {
            get { return _mappingType; }
            set
            {
                SetAndNotify(ref _mappingType, value);
                if (MappingType == BoneMappingType.None)
                    ClearMapping(false);
            }
        }

        AdvBoneMappingBoneSettings _settings;
        public AdvBoneMappingBoneSettings Settings
        {
            get { return _settings; }
            set { SetAndNotify(ref _settings, value); }
        }

        public ObservableCollection<AdvBoneMappingBone> Children { get; set; } = new ObservableCollection<AdvBoneMappingBone>();


        public void OnApplySettingsToAllChildNodesCommand(AdvBoneMappingBone settingsOwner)
        {
            var directSettingsOwner = settingsOwner.Settings as DirectAdvBoneMappingBoneSettings;
            if (directSettingsOwner == null)
                return;

            if (Settings?.HasMapping == true && settingsOwner != this)
            {
                var directSettings = Settings as DirectAdvBoneMappingBoneSettings;
                if (directSettings == null)
                {
                    var oldSettings = Settings;
                    directSettings = new DirectAdvBoneMappingBoneSettings();
                    directSettings.CopyBaseValues(oldSettings);
                }

                directSettings.SkeletonScaleValue.Value = directSettingsOwner.SkeletonScaleValue.Value;
                directSettings.BoneCopyMethod = directSettingsOwner.BoneCopyMethod;
                directSettings.Ratio_ScaleMethod = directSettingsOwner.Ratio_ScaleMethod;
                directSettings.Ratio_ScaleRotation = directSettingsOwner.Ratio_ScaleRotation;
            }

            foreach (var child in Children)
                child.OnApplySettingsToAllChildNodesCommand(settingsOwner);
        }

        public void ClearMapping(bool applyToChildren = false)
        {
            Settings = null;

            // Avoid stack overflow! 
            if (MappingType != BoneMappingType.None)
                MappingType = BoneMappingType.None;

            MappingDisplayStr = string.Empty;

            if (applyToChildren)
            {
                foreach (var child in Children)
                    child.ClearMapping(applyToChildren);
            }
        }

        public void CreateDirectMapping(BoneMappingType mappingType, AdvBoneMappingBone source)
        {
            if (Settings as DirectAdvBoneMappingBoneSettings == null)
                Settings = new DirectAdvBoneMappingBoneSettings();
            Settings.HasMapping = true;
            Settings.MappingBoneName = source.BoneName;
            Settings.MappingBoneId = source.BoneIndex;

            MappingType = mappingType;
            MappingDisplayStr = $"[{source.BoneName} [{source.BoneIndex}]]";
        }

        public AdvBoneMappingBone Copy(bool includeChildren = false)
        {
            var newBone = new AdvBoneMappingBone()
            {
                BoneName = BoneName,
                BoneIndex = BoneIndex,
                ParentBoneIndex = ParentBoneIndex,
                DisplayName = DisplayName,
                MappingDisplayStr = MappingDisplayStr,
                ChildNodeCounts = ChildNodeCounts,

                Settings = Settings?.Copy()
            };

            if (includeChildren)
                throw new NotImplementedException();

            return newBone;
        }

        public int ComputeChildBoneCount()
        {
            int count = Children.Count();
            foreach (var child in Children)
            {
                var childCount = child.ComputeChildBoneCount();
                count += childCount;
            }

            return count;
        }
    }

    public abstract class AdvBoneMappingBoneSettings : NotifyPropertyChangedImpl
    {
        bool _hasMapping = false;
        public bool HasMapping
        {
            get { return _hasMapping; }
            set { SetAndNotify(ref _hasMapping, value); }
        }

        bool _useMapping = true;
        public bool UseMapping
        {
            get { return _useMapping; }
            set { SetAndNotify(ref _useMapping, value); }
        }

        int _mappingBoneId = -1;
        public int MappingBoneId
        {
            get { return _mappingBoneId; }
            set { SetAndNotify(ref _mappingBoneId, value); }
        }

        string _mappingBoneName = string.Empty;
        public string MappingBoneName
        {
            get { return _mappingBoneName; }
            set { SetAndNotify(ref _mappingBoneName, value); }
        }

        DoubleViewModel _skeletonScaleValue = new DoubleViewModel(1);
        public DoubleViewModel SkeletonScaleValue
        {
            get { return _skeletonScaleValue; }
            set { SetAndNotify(ref _skeletonScaleValue, value); }
        }


        bool _useConstantOffset = true;
        public bool UseConstantOffset
        {
            get { return _useConstantOffset; }
            set { SetAndNotify(ref _useConstantOffset, value); }
        }

        Vector3ViewModel _contantTranslationOffset = new Vector3ViewModel();
        public Vector3ViewModel ContantTranslationOffset
        {
            get { return _contantTranslationOffset; }
            set { SetAndNotify(ref _contantTranslationOffset, value); }
        }

        Vector3ViewModel _contantRotationOffset = new Vector3ViewModel();
        public Vector3ViewModel ContantRotationOffset
        {
            get { return _contantRotationOffset; }
            set { SetAndNotify(ref _contantRotationOffset, value); }
        }

        public void CopyBaseValues(AdvBoneMappingBoneSettings other)
        {
            other.HasMapping = HasMapping;
            other.MappingBoneId = MappingBoneId;
            other.MappingBoneName = MappingBoneName;
            other.SkeletonScaleValue = SkeletonScaleValue;
            other.ContantTranslationOffset = ContantTranslationOffset;
            other.ContantRotationOffset = ContantRotationOffset;
            other.UseMapping = UseMapping;
            other.UseConstantOffset = UseConstantOffset;
        }

        public abstract AdvBoneMappingBoneSettings Copy();
    }


    public class DirectAdvBoneMappingBoneSettings : AdvBoneMappingBoneSettings
    {
        BoneCopyMethod _boneCopyMethod = BoneCopyMethod.Ratio;
        public BoneCopyMethod BoneCopyMethod
        {
            get { return _boneCopyMethod; }
            set
            {
                SetAndNotify(ref _boneCopyMethod, value);
                RatioSettingsAvailable = _boneCopyMethod == BoneCopyMethod.Ratio;
            }
        }

        bool _ratioSettingsAvailable = true;
        public bool RatioSettingsAvailable
        {
            get { return _ratioSettingsAvailable; }
            set { SetAndNotify(ref _ratioSettingsAvailable, value); }
        }

        RatioScaleMethod _ratio_ScaleMethod = RatioScaleMethod.Larger;
        public RatioScaleMethod Ratio_ScaleMethod
        {
            get { return _ratio_ScaleMethod; }
            set { SetAndNotify(ref _ratio_ScaleMethod, value); }
        }

        bool _ratio_ScaleRotation = false;
        public bool Ratio_ScaleRotation
        {
            get { return _ratio_ScaleRotation; }
            set { SetAndNotify(ref _ratio_ScaleRotation, value); }
        }

        public override AdvBoneMappingBoneSettings Copy()
        {
            var setting = new DirectAdvBoneMappingBoneSettings()
            {
                BoneCopyMethod = BoneCopyMethod,
                RatioSettingsAvailable = RatioSettingsAvailable,
                Ratio_ScaleMethod = Ratio_ScaleMethod,
                Ratio_ScaleRotation = Ratio_ScaleRotation
            };

            CopyBaseValues(setting);
            return setting;
        }
    }
}
