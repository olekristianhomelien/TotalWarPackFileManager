using CommonDialogs.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping
{
    class AdvBoneMappingBone : NotifyPropertyChangedImpl
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

        public int ComputeChildBones()
        {
            int count = Children.Count();
            foreach (var child in Children)
            {
                var childCount = child.ComputeChildBones();
                count += childCount;
            }

            return count;
        }
    }
}
