using CommonDialogs.Common;
using CommonDialogs.MathViews;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        Direct_smart,
        AttachmentPoint
        //Chain,
        //Ik
    }

    class AdvBoneMappingViewModel : NotifyPropertyChangedImpl
    {
        enum CurrentOtherBoneListValue
        { 
            None,
            Original,
            Other
        }

        CurrentOtherBoneListValue _currentOtherBoneListValue = CurrentOtherBoneListValue.None;
        bool _canSelectOtherBones = true;
        public ICommand ClearBindingSelfCommand { get; set; }
        public ICommand ClearBindingSelfAndChildrenCommand { get; set; }
        public ICommand AutoMapSelfAndChildrenByNameCommand { get; set; }
        public ICommand AutoMapSelfAndChildrenByHierarchyCommand { get; set; }
        public ICommand ApplySettingsToAllChildNodesCommand { get; set; }

        SkeletonViewModel _originalViewModel;
        SkeletonViewModel _otherViewModel;

        public AdvBoneMappingViewModel(ObservableCollection<AdvBoneMappingBone> orgBoneMapping, GameSkeleton other, SkeletonViewModel originalViewMode, SkeletonViewModel otherViewModel)
        {
            _originalViewModel = originalViewMode;
            _otherViewModel = otherViewModel;

            _allOriginalBones = orgBoneMapping;
            _allOtherBones = BoneMappingHelper.CreateSkeletonBoneList(other);
            VisibleOrigianBones = _allOriginalBones;
            VisibleOtherBones = new ObservableCollection<AdvBoneMappingBone>();

            ClearBindingSelfCommand = new RelayCommand<AdvBoneMappingBone>((node) => { node.ClearMapping(); });
            ClearBindingSelfAndChildrenCommand = new RelayCommand<AdvBoneMappingBone>((node) =>
            {
                if (node != null)
                    node.ClearMapping(true);
                else
                    _allOriginalBones.FirstOrDefault().ClearMapping(true);
            });

            AutoMapSelfAndChildrenByNameCommand = new RelayCommand<AdvBoneMappingBone>((node) => 
            { 
                if(node != null)
                    BoneMappingHelper.AutomapDirectBoneLinksBasedOnNames(node, _allOtherBones); 
                else
                    BoneMappingHelper.AutomapDirectBoneLinksBasedOnNames(_allOriginalBones.FirstOrDefault(), _allOtherBones);
            });

            AutoMapSelfAndChildrenByHierarchyCommand = new RelayCommand<AdvBoneMappingBone>((node) => 
            {
                if (node != null)
                    BoneMappingHelper.AutomapDirectBoneLinksBasedOnHierarchy(node, SelectedOtherBone);
                //else
                //    BoneMappingHelper.AutomapDirectBoneLinksBasedOnHierarchy(_allOriginalBones.FirstOrDefault(), _allOtherBones); 
            });

            ApplySettingsToAllChildNodesCommand = new RelayCommand<AdvBoneMappingBone>((node) => { node.OnApplySettingsToAllChildNodesCommand(node); });

            BoneMappingHelper.ComputeBoneCount(_allOtherBones);
        }

        public ObservableCollection<AdvBoneMappingBone> GetBoneMapping()
        {
            return _allOriginalBones;
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
            set 
            {
                if (_selectedOriginalBone == value)
                    return;
                OnOrigianlBoneDeSelected(_selectedOriginalBone); 
                SetAndNotify(ref _selectedOriginalBone, value);  
                OnOrigianlBoneSelected(SelectedOriginalBone); }
        }


        AdvBoneMappingBone _selectedOtherBone;
        public AdvBoneMappingBone SelectedOtherBone
        {
            get { return _selectedOtherBone; }
            set 
            {
                if (_selectedOtherBone == value)
                    return;
                SetAndNotify(ref _selectedOtherBone, value); OnOtherBoneSelected(SelectedOtherBone); 
            }
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
                {
                    SetOtherBonesList(_currentOtherBoneListValue, null, true);
                    VisibleOtherBones = FilterHelper.FilterBoneList(OtherBoneFilterText, VisibleOtherBones);
                }
                _canSelectOtherBones = true;
                SelectedOtherBone = oldSelection;
            }
        }

        private void SelectedSourceBone_OnBoneMappingTypeChanged(object sender, BoneMappingType newValue)
        {
            var bone = sender as AdvBoneMappingBone;
            if (bone != null)
            {
                var index = -1;
                var name = string.Empty;
                if (bone.Settings != null)
                {
                    index = bone.Settings.MappingBoneId;
                    name = bone.Settings.MappingBoneName;
                }

                if (newValue == BoneMappingType.None)
                    bone.ClearMapping();
                else if (newValue == BoneMappingType.Direct)
                    bone.CreateDirectMapping(name, index);
                else if (newValue == BoneMappingType.Direct_smart)
                    bone.CreateDirectSmartMapping(name, index);
                else if (newValue == BoneMappingType.AttachmentPoint)
                    bone.CreateAttachmentPointMapping(name, index);
                else
                    throw new Exception("Unkown bonemapping type");
            }

            if (newValue == BoneMappingType.Direct_smart || newValue == BoneMappingType.Direct)
                SetOtherBonesList(CurrentOtherBoneListValue.Other, null);
            else if (newValue == BoneMappingType.AttachmentPoint)
                SetOtherBonesList(CurrentOtherBoneListValue.Original, null);
            else
                SetOtherBonesList(CurrentOtherBoneListValue.None, null);
        }

        void OnOrigianlBoneDeSelected(AdvBoneMappingBone deselectedBone)
        {
            if (deselectedBone != null)
                deselectedBone.OnBoneMappingTypeChanged -= SelectedSourceBone_OnBoneMappingTypeChanged;
        }

        void OnOrigianlBoneSelected(AdvBoneMappingBone selectedSourceBone)
        {
            if (selectedSourceBone == null)
            {
                SetOtherBonesList(CurrentOtherBoneListValue.None, null);
                return;
            }

            selectedSourceBone.OnBoneMappingTypeChanged += SelectedSourceBone_OnBoneMappingTypeChanged;

            // This will remove the filter
            SelectedOtherBone = null;
            if(!string.IsNullOrWhiteSpace(OtherBoneFilterText))
                OtherBoneFilterText = "";

            if(selectedSourceBone.MappingType == BoneMappingType.Direct_smart)
                SetOtherBonesList(CurrentOtherBoneListValue.Other, selectedSourceBone);
            else if (selectedSourceBone.MappingType == BoneMappingType.AttachmentPoint)
                SetOtherBonesList(CurrentOtherBoneListValue.Original, selectedSourceBone);
            else 
                SetOtherBonesList(CurrentOtherBoneListValue.Other, selectedSourceBone);

            _originalViewModel.SetSelectedBoneByIndex(selectedSourceBone.BoneIndex);
        }

        void SetOtherBonesList(CurrentOtherBoneListValue value, AdvBoneMappingBone selectedBone, bool force = false)
        {
            if (!force)
            {
                if (_currentOtherBoneListValue == value)
                    return;
            }

            if (value == CurrentOtherBoneListValue.None)
                VisibleOtherBones = new ObservableCollection<AdvBoneMappingBone>();
                
            else if (value == CurrentOtherBoneListValue.Other)
                VisibleOtherBones = _allOtherBones;
            else if (value == CurrentOtherBoneListValue.Original)
                VisibleOtherBones = _allOriginalBones;
             
            SelectedOtherBone = null;
            if (value == CurrentOtherBoneListValue.Other || value == CurrentOtherBoneListValue.Original)
            {
                if (selectedBone?.Settings != null)
                    SelectedOtherBone = BoneMappingHelper.GetBoneFromIndex(VisibleOtherBones, selectedBone.Settings.MappingBoneId);
            }

            _currentOtherBoneListValue = value;
        }


        void OnOtherBoneSelected(AdvBoneMappingBone selectedTargetBone)
        {
            if (selectedTargetBone == null || _canSelectOtherBones == false)
                return;

            if (SelectedOriginalBone == null)
                return;

            if (SelectedOriginalBone.MappingType == BoneMappingType.None)
            {
                SelectedOriginalBone.CreateDirectMapping(selectedTargetBone.Settings.MappingBoneName, selectedTargetBone.Settings.MappingBoneId);
            }
            else
            {
                SelectedOriginalBone.Settings.MappingBoneName = selectedTargetBone.BoneName;
                SelectedOriginalBone.Settings.MappingBoneId = selectedTargetBone.BoneIndex;
                SelectedOriginalBone.Settings.UpdateDisplayString();
            }
            _otherViewModel.SetSelectedBoneByIndex(selectedTargetBone.BoneIndex);
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
