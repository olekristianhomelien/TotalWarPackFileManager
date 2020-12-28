using CommonDialogs.Common;
using CommonDialogs.MathViews;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VariantMeshEditor.Services;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping
{
    public enum BoneMappingType
    {
        None,
        Direct,
        //Chain,
        //Ik
    }

    

    class AdvBoneMappingViewModel : NotifyPropertyChangedImpl
    {
        public ICommand ClearBindingSelfCommand { get; set; }
        public ICommand ClearBindingSelfAndChildrenCommand { get; set; }
        public ICommand AutoMapSelfAndChildrenByNameCommand { get; set; }
        public ICommand AutoMapSelfAndChildrenByHierarchyCommand { get; set; }
        public ICommand ApplySettingsToAllChildNodesCommand { get; set; }


        SkeletonViewModel _originalViewModel;
        SkeletonViewModel _otherViewModel;

        public AdvBoneMappingViewModel(GameSkeleton original, GameSkeleton other, SkeletonViewModel originalViewMode, SkeletonViewModel otherViewModel)
        {
            _originalViewModel = originalViewMode;
            _otherViewModel = otherViewModel;

            CreateSkeletonBoneList(original, out _allOriginalBones);
            CreateSkeletonBoneList(other, out _allOtherBones);
            VisibleOrigianBones = _allOriginalBones;

            ClearBindingSelfCommand = new RelayCommand<AdvBoneMappingBone>((node) => { node.ClearMapping(); });
            ClearBindingSelfAndChildrenCommand = new RelayCommand<AdvBoneMappingBone>((node) => { node.ClearMapping(true); });
            AutoMapSelfAndChildrenByNameCommand = new RelayCommand<AdvBoneMappingBone>((node) => { AutomapDirectBoneLinksBasedOnNames(node, _allOtherBones); });
            AutoMapSelfAndChildrenByHierarchyCommand = new RelayCommand<AdvBoneMappingBone>((node) => { AutomapDirectBoneLinksBasedOnHierarchy(node, _allOtherBones); });
            ApplySettingsToAllChildNodesCommand = new RelayCommand<AdvBoneMappingBone>((node) => { node.OnApplySettingsToAllChildNodesCommand(node); });


        }

        void AutomapDirectBoneLinksBasedOnNames(AdvBoneMappingBone boneToGetMapping, IEnumerable<AdvBoneMappingBone> externalBonesList)
        {
            var otherBone = FindBoneBasedOnName(boneToGetMapping.BoneName, externalBonesList);
            if (otherBone != null)
                boneToGetMapping.CreateMapping(BoneMappingType.Direct, otherBone);

            foreach (var bone in boneToGetMapping.Children)
                AutomapDirectBoneLinksBasedOnNames(bone, externalBonesList);
        }

        void AutomapDirectBoneLinksBasedOnHierarchy(AdvBoneMappingBone boneToGetMapping, IEnumerable<AdvBoneMappingBone> externalBonesList)
        {
        }

  

        AdvBoneMappingBone FindBoneBasedOnName(string name, IEnumerable<AdvBoneMappingBone> boneList)
        {
            foreach (var bone in boneList)
            {
                if (bone.BoneName == name)
                    return bone;

                var result = FindBoneBasedOnName(name, bone.Children);
                if (result != null)
                    return result;
            }

            return null;
        }

        static void CreateSkeletonBoneList(GameSkeleton skeleton, out ObservableCollection<AdvBoneMappingBone> outputList)
        {
            outputList = new ObservableCollection<AdvBoneMappingBone>();

            for (int i = 0; i < skeleton.BoneCount; i++)
            {
                int boneId = i;
                int parentBoneId = skeleton.GetParentBone(i);
                string boneName = skeleton.BoneNames[i];

                AdvBoneMappingBone node = new AdvBoneMappingBone
                {
                    BoneIndex = boneId,
                    BoneName = boneName,
                    ParentBoneIndex = parentBoneId,
                    DisplayName = $"{boneName} [{boneId}]",
                    MappingDisplayStr = string.Empty,
                };

                var treeParent = GetParent(outputList, parentBoneId);
                if (treeParent == null)
                    outputList.Add(node);
                else
                    treeParent.Children.Add(node);
            }
        }

        static AdvBoneMappingBone GetParent(ObservableCollection<AdvBoneMappingBone> root, int parentBoneId)
        {
            foreach (var item in root)
            {
                if (item.BoneIndex == parentBoneId)
                    return item;

                var result = GetParent(item.Children, parentBoneId);
                if (result != null)
                    return result;
            }
            return null;
        }

        static AdvBoneMappingBone GetBoneFromIndex(IEnumerable<AdvBoneMappingBone> root, int index)
        {
            foreach (var item in root)
            {
                if (item.BoneIndex == index)
                    return item;

                var result = GetBoneFromIndex(item.Children, index);
                if (result != null)
                    return result;
            }
            return null;
        }

        ObservableCollection<AdvBoneMappingBone> _allOriginalBones;

        ObservableCollection<AdvBoneMappingBone> _visibleOrigianBones;
        public ObservableCollection<AdvBoneMappingBone> VisibleOrigianBones
        {
            get { return _visibleOrigianBones; }
            set { SetAndNotify(ref _visibleOrigianBones, value); }
        }
        ObservableCollection<AdvBoneMappingBone> _allOtherBones;

        ObservableCollection<AdvBoneMappingBone> _visibleOtherBones;
        public ObservableCollection<AdvBoneMappingBone> VisibleOtherBones
        {
            get { return _visibleOtherBones; }
            set { SetAndNotify(ref _visibleOtherBones, value); }
        }

        AdvBoneMappingBone _selectedOriginalBone;
        public AdvBoneMappingBone SelectedOriginalBone
        {
            get { return _selectedOriginalBone; }
            set { SetAndNotify(ref _selectedOriginalBone, value); OnOrigianlBoneSelected(SelectedOriginalBone); }
        }


        AdvBoneMappingBone _selectedOtherBone;
        public AdvBoneMappingBone SelectedOtherBone
        {
            get { return _selectedOtherBone; }
            set { SetAndNotify(ref _selectedOtherBone, value); OnOtherBoneSelected(SelectedOtherBone); }
        }

        string _originalBoneFilterText = string.Empty;
        public string OriginalBoneFilterText
        {
            get { return _originalBoneFilterText; }
            set 
            {
                SetAndNotify(ref _originalBoneFilterText, value);
                VisibleOrigianBones = FilterHelper.FilterBoneList(OriginalBoneFilterText, _allOriginalBones);
            }
        }

        string _otherBoneFilterText = string.Empty;
        public string OtherBoneFilterText
        {
            get { return _otherBoneFilterText; }
            set
            {
                _canSelectOtherBones = false;
                SetAndNotify(ref _otherBoneFilterText, value);
                var oldSelection = SelectedOtherBone;
                if (SelectedOriginalBone != null)
                    VisibleOtherBones = FilterHelper.FilterBoneList(OtherBoneFilterText, _allOtherBones);
                _canSelectOtherBones = true;
                SelectedOtherBone = oldSelection;
            }
        }

        bool _canSelectOtherBones = true;

        void OnOrigianlBoneSelected(AdvBoneMappingBone selectedSourceBone)
        {
            if (selectedSourceBone == null)
            {
                SelectedOtherBone = null;
                VisibleOtherBones = new ObservableCollection<AdvBoneMappingBone>();
                return;
            }

            // This will remove the filter
            OtherBoneFilterText = "";

            if (selectedSourceBone.MappingType == BoneMappingType.Direct)
                SelectedOtherBone = GetBoneFromIndex(VisibleOtherBones, selectedSourceBone.Settings.MappingBoneId);
            else if (selectedSourceBone.MappingType == BoneMappingType.None)
                SelectedOtherBone = null;

            _originalViewModel.SetSelectedBoneByIndex(selectedSourceBone.BoneIndex);
        }

        void OnOtherBoneSelected(AdvBoneMappingBone selectedTargetBone)
        {
            if (selectedTargetBone == null || _canSelectOtherBones == false)
                return;

            if (SelectedOriginalBone.MappingType  == BoneMappingType.None)
            {
                SelectedOriginalBone.CreateMapping(BoneMappingType.Direct, selectedTargetBone);
            }

            _otherViewModel.SetSelectedBoneByIndex(selectedTargetBone.BoneIndex);
        }
    }

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
            
        
        }

        public void ClearMapping(bool applyToChildren = false)
        {
            Settings = null;

            // Avoid stack overflow! 
            if(MappingType != BoneMappingType.None)
                MappingType = BoneMappingType.None;

            MappingDisplayStr = string.Empty;

            if (applyToChildren)
            {
                foreach (var child in Children)
                    child.ClearMapping(applyToChildren);
            }
        }

        public void CreateMapping(BoneMappingType mappingType, AdvBoneMappingBone source)
        {
            if(mappingType == BoneMappingType.Direct)
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

                Settings = Settings?.Copy()
            };

            if (includeChildren)
                throw new NotImplementedException();

            return newBone;
        }
    }

    abstract class AdvBoneMappingBoneSettings : NotifyPropertyChangedImpl
    {
        bool _hasMapping = false;
        public bool HasMapping
        {
            get { return _hasMapping; }
            set { SetAndNotify(ref _hasMapping, value); }
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

        DoubleViewModel _skeletonScaleValue = new DoubleViewModel();
        public DoubleViewModel SkeletonScaleValue
        {
            get { return _skeletonScaleValue; }
            set { SetAndNotify(ref _skeletonScaleValue, value); }
        }

        protected void CopyBaseValues(AdvBoneMappingBoneSettings other)
        {
            other.HasMapping = HasMapping;
            other.MappingBoneId = MappingBoneId;
            other.MappingBoneName = MappingBoneName;
            other.SkeletonScaleValue = other.SkeletonScaleValue;
        }

        public abstract AdvBoneMappingBoneSettings Copy();
    }


    class DirectAdvBoneMappingBoneSettings : AdvBoneMappingBoneSettings
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


    class FilterHelper
    {
        public static ObservableCollection<AdvBoneMappingBone> FilterBoneList(string filterText, ObservableCollection<AdvBoneMappingBone> completeList)
        {
            var output = new ObservableCollection<AdvBoneMappingBone>();
            FilterBoneListRecursive(filterText, completeList, output);
            return output;
        }

        static void FilterBoneListRecursive(string filterText, ObservableCollection<AdvBoneMappingBone> completeList, ObservableCollection<AdvBoneMappingBone> output)
        {
            foreach (var item in completeList)
            {
                bool isVisible = IsBoneVisibleInFilter(item, filterText, true);
                if (isVisible)
                {
                    var newItem = item.Copy(false);
                    output.Add(newItem);
                    FilterBoneListRecursive(filterText, item.Children, newItem.Children);
                }
            }
        }

        static bool IsBoneVisibleInFilter(AdvBoneMappingBone bone, string filterText, bool checkChildren)
        {
            var contains = bone.BoneName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) != -1;
            if (contains)
                return contains;

            if (checkChildren)
            {
                foreach (var child in bone.Children)
                {
                    var res = IsBoneVisibleInFilter(child, filterText, checkChildren);
                    if (res == true)
                        return true;
                }
            }

            return false;
        }
    }
}
