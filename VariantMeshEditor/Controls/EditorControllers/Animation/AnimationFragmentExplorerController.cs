using Common;
using CommonDialogs.Common;
using Filetypes.AnimationPack;
using Filetypes.ByteParsing;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using static CommonDialogs.ComboBoxes.FilteredComboBox;
using static CommonDialogs.Common.NotifyPropertyChangedImpl;

namespace VariantMeshEditor.Controls.EditorControllers.Animation
{
    class AnimationFragmentExplorerController
    {



        FragmentExplorerViewModel _fragmentExplorerViewModel;
        ResourceLibary _resourceLibary;
        AnimationPackLoader _animationPackData;
        Dictionary<int, Dictionary<string, List<AnimationFragmentItem>>> _fragmentList;
        //AnimationEditorView _editorView;


        public AnimationFragmentExplorerController(ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
        }

        //public void PopulateExplorerView(AnimationEditorView view)
        //{
        //    _editorView = view;
        //
        //    try
        //    {
        //        var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\animation_tables\animation_tables.animpack");
        //        _animationPackData = new AnimationPackLoader();
        //        _animationPackData.Load(new ByteChunk(file.Data));
        //
        //        _fragmentExplorerViewModel = new FragmentExplorerViewModel();
        //
        //        var skeltonNames = _animationPackData.AnimationTableEntries
        //            .Select(x => x.SkeletonName)
        //            .Distinct()
        //            .OrderBy(x => x); ;
        //
        //        foreach (var skeletonName in skeltonNames)
        //            _fragmentExplorerViewModel.SkeletonNameList.Add(skeletonName);
        //
        //        _fragmentExplorerViewModel.OnSelectedSkeletonChanged += OnSkeletonSelected;
        //        _fragmentExplorerViewModel.OnSelectedMountChanged += OnMountSelected;
        //        _fragmentExplorerViewModel.OnSelectedAnimationSetChanged += OnAnimationSetSelected;
        //
        //        view.AnimationFragmentExplorer.DataContext = _fragmentExplorerViewModel;
        //
        //
        //        _fragmentExplorerViewModel.SelectedSkelton = "humanoid01";
        //    }
        //    catch (Exception e)
        //    {
        //
        //    }
        //
        //    //_viewModel = viewModel;
        //    //_viewModel.AnimationExplorer.CreateNewAnimationButton.Click += (sender, e) => CreateAnimationExplorer();
        //    //
        //    //FindAllAnimations();
        //    //CreateAnimationExplorer(true);
        //}

        void OnSkeletonSelected(string newSkeletonName)
        {
            using (new DisableCallbacks(_fragmentExplorerViewModel))
            {
                PopulatePossibleMountCollections();
                PopulatePossibleAnimationSets();
            }
        }

        void OnMountSelected(string newMountName)
        {
            PopulatePossibleAnimationSets();
        }

        void OnAnimationSetSelected(string animationSet)
        {
            var item = _animationPackData.AnimationTableEntries.FirstOrDefault(x => x.Name == animationSet);

            if (item != null)
            {
                List<AnimationFragmentCollection> fragmentCollections = new List<AnimationFragmentCollection>();
                foreach (var entry in item.AnimationSets)
                {
                    var foundFragments = _animationPackData.AnimationFragments.Where(x => x.FileName.Contains(entry.Name));
                    foreach (var fragment in foundFragments)
                        fragmentCollections.Add(fragment);

                }

                _fragmentList = new Dictionary<int, Dictionary<string, List<AnimationFragmentItem>>>();
                foreach (var collction in fragmentCollections)
                {
                    foreach (var fragment in collction.AnimationFragments)
                    {
                        if (!_fragmentList.ContainsKey(fragment.Slot))
                            _fragmentList.Add(fragment.Slot, new Dictionary<string, List<AnimationFragmentItem>>());

                        if (!_fragmentList[fragment.Slot].ContainsKey(collction.FileName))
                            _fragmentList[fragment.Slot].Add(collction.FileName, new List<AnimationFragmentItem>());

                        _fragmentList[fragment.Slot][collction.FileName].Add(fragment);
                    }
                }

                _fragmentExplorerViewModel.FragmentCollectionList.Clear();
                foreach (var collection in fragmentCollections)
                    _fragmentExplorerViewModel.FragmentCollectionList.Add(new FragmentCollectionViewItem(collection, true));

                var finalFragList = new List<AnimationFragmentItem>();
                foreach (var fragment in _fragmentList)
                {
                    var currentSlot = fragment.Key;
                    var allOptions = fragment.Value;
                    var selectedValue = allOptions.Last();
                    foreach (var value in selectedValue.Value)
                        finalFragList.Add(value);
                }

                //_editorView.AnimationFragmentExplorer.FragmentFilterDialog.SetItems(finalFragList, GetFragmentFilterDialogHeaders());
                //_editorView.AnimationFragmentExplorer.FragmentFilterDialog.OnSearch = _fragmentExplorerViewModel.OnSerach;
            }
        }




        void PopulatePossibleMountCollections()
        {
            _fragmentExplorerViewModel.SelectedMount = null;

            if (string.IsNullOrWhiteSpace(_fragmentExplorerViewModel.SelectedSkelton))
            {
                _fragmentExplorerViewModel.MountNameList.Clear();
            }
            else
            {
                var mountNames = _animationPackData.AnimationTableEntries
                    .Where(x => x.SkeletonName == _fragmentExplorerViewModel.SelectedSkelton)
                    .Select(x => x.MountName)
                    .Distinct()
                    .OrderBy(x=>x);

                _fragmentExplorerViewModel.MountNameList.Clear();
                foreach (var mountName in mountNames)
                    _fragmentExplorerViewModel.MountNameList.Add(mountName);
            }
        }

        void PopulatePossibleAnimationSets()
        {
            _fragmentExplorerViewModel.SelectedAnimationSet = null;

            var animationSets = _animationPackData.AnimationTableEntries.Where(x => x.SkeletonName == _fragmentExplorerViewModel.SelectedSkelton);
            
            if (!string.IsNullOrWhiteSpace(_fragmentExplorerViewModel.SelectedMount))
                animationSets = animationSets.Where(x => x.MountName == _fragmentExplorerViewModel.SelectedMount);
             
            var animationSetNames = animationSets.Select(x => x.Name)
                                                 .Distinct()
                                                 .OrderBy(x => x);

            _fragmentExplorerViewModel.AnimationSetList.Clear();
            foreach (var name in animationSetNames)
                _fragmentExplorerViewModel.AnimationSetList.Add(name);



        }

        GridViewColumn[] GetFragmentFilterDialogHeaders()
        {
            return new GridViewColumn[]
            {
                new GridViewColumn()
                {
                    Header = "Slot",
                    DisplayMemberBinding = new Binding("Slot")
                },

                new GridViewColumn()
                {
                    Header = "AnimationFile",
                    DisplayMemberBinding = new Binding("AnimationFile")
                },


                new GridViewColumn()
                {
                    Header = "MetaDataFile",
                    DisplayMemberBinding = new Binding("MetaDataFile")
                },
            };
        }
    }



    public class FragmentCollectionViewItem
    {
        AnimationFragmentCollection _collection;
        string _type;

        public FragmentCollectionViewItem(AnimationFragmentCollection collection, bool isNewFormat)
        {
            _collection = collection;
            IsSelected = true;
            if (isNewFormat)
                _type = "(Fragment)";
            else
                _type = "(Txt)";
        }

        public string Name { get { return _collection.FileName + "  " + _type; } }
        public bool IsSelected { get; set; }
    }

    public class FragmentExplorerViewModel : NotifyPropertyChangedImpl
    {
        public ValueChangedDelegate<string> OnSelectedSkeletonChanged;
        public ValueChangedDelegate<string> OnSelectedMountChanged;
        public ValueChangedDelegate<string> OnSelectedAnimationSetChanged;



        ObservableCollection<FragmentCollectionViewItem> _fragmentCollectionList = new ObservableCollection<FragmentCollectionViewItem>();
        public ObservableCollection<FragmentCollectionViewItem> FragmentCollectionList
        {
            get => _fragmentCollectionList;
            set => SetAndNotify(ref _fragmentCollectionList, value);
        }




        ObservableCollection<string> _skeletonNameList = new ObservableCollection<string>();
        public ObservableCollection<string> SkeletonNameList
        {
            get => _skeletonNameList;
            set => SetAndNotify(ref _skeletonNameList, value);
        }

        string _selectedSkeletonName;
        public string SelectedSkelton
        {
            get => _selectedSkeletonName;
            set => SetAndNotify(ref _selectedSkeletonName, value, OnSelectedSkeletonChanged);
        }


        ObservableCollection<string> _mountNameList = new ObservableCollection<string>();
        public ObservableCollection<string> MountNameList
        {
            get => _mountNameList;
            set => SetAndNotify(ref _mountNameList, value);
        }


        string _selectedMount;
        public string SelectedMount
        {
            get => _selectedMount;
            set => SetAndNotify(ref _selectedMount, value, OnSelectedMountChanged);
        }

        ObservableCollection<string> _animationSetList = new ObservableCollection<string>();
        public ObservableCollection<string> AnimationSetList
        {
            get => _animationSetList;
            set => SetAndNotify(ref _animationSetList, value);
        }

        
        string _selectedAnimationSet;
        public string SelectedAnimationSet
        {
            get => _selectedAnimationSet;
            set => SetAndNotify(ref _selectedAnimationSet, value, OnSelectedAnimationSetChanged);
        }


        public bool OnSerach(object value)
        {
            return true;
        }
    }

}
