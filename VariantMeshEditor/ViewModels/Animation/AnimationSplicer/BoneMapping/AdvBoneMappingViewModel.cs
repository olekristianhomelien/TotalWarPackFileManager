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

        public AdvBoneMappingViewModel(ObservableCollection<AdvBoneMappingBone> orgBoneMapping, GameSkeleton other, SkeletonViewModel originalViewMode, SkeletonViewModel otherViewModel)
        {
            _originalViewModel = originalViewMode;
            _otherViewModel = otherViewModel;

            _allOriginalBones = orgBoneMapping;
            _allOtherBones = BoneMappingHelper.CreateSkeletonBoneList(other);
            VisibleOrigianBones = _allOriginalBones;
            VisibleOtherBones = _allOtherBones;

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
                    BoneMappingHelper.AutomapDirectBoneLinksBasedOnHierarchy(node, _allOtherBones);
                else
                    BoneMappingHelper.AutomapDirectBoneLinksBasedOnHierarchy(_allOriginalBones.FirstOrDefault(), _allOtherBones); 
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
            SelectedOtherBone = null;
            if(!string.IsNullOrWhiteSpace(OtherBoneFilterText))
                OtherBoneFilterText = "";

            if (selectedSourceBone.Settings.MappingType == BoneMappingType.Direct)
                SelectedOtherBone = BoneMappingHelper.GetBoneFromIndex(VisibleOtherBones, selectedSourceBone.Settings.MappingBoneId);
            else if (selectedSourceBone.Settings.MappingType == BoneMappingType.None)
                SelectedOtherBone = null;

            _originalViewModel.SetSelectedBoneByIndex(selectedSourceBone.BoneIndex);
        }

        void OnOtherBoneSelected(AdvBoneMappingBone selectedTargetBone)
        {
            if (selectedTargetBone == null || _canSelectOtherBones == false)
                return;

            if (SelectedOriginalBone.Settings.MappingType  == BoneMappingType.None 
                || SelectedOriginalBone.Settings.MappingType == BoneMappingType.Direct)
            {
                SelectedOriginalBone.CreateDirectMapping(BoneMappingType.Direct, selectedTargetBone);
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
