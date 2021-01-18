using Common;
using CommonDialogs.Common;
using Filetypes.AnimationPack;
using Filetypes.ByteParsing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Scene;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class FragmentCollectionViewItem : NotifyPropertyChangedImpl
    {
        public event ValueChangedDelegate<bool> SelectionChanged;
        public AnimationFragmentCollection Collection { get; set; }

        public FragmentCollectionViewItem(AnimationFragmentCollection collection)
        {
            Collection = collection;
            IsSelected = true;
        }

        public string Name { get { return Collection.FileName; } }

        public bool _isSelected = true;
        public bool IsSelected { get { return _isSelected; } set { SetAndNotify(ref _isSelected, value, SelectionChanged); } }
    }

    public class FragmentExplorerViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<FragmentExplorerViewModel>();
        AnimationPlayerViewModel _animationPlayer;
        ResourceLibary _resourceLibary;
        AnimationPackLoader _animationPackData;


        bool _isSelected;
        public bool IsSelected 
        { 
            get { return _isSelected; } 
            set { SetAndNotify(ref _isSelected, value); IsInFocus(_isSelected); } 
        }

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
            set { SetAndNotify(ref _selectedSkeletonName, value); OnSkeletonSelected(_selectedSkeletonName); }
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
            set { SetAndNotify(ref _selectedMount, value); OnMountSelected(_selectedMount); }
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
            set { SetAndNotify(ref _selectedAnimationSet, value); OnAnimationSetSelected(_selectedAnimationSet);}
        }

        public FragmentExplorerViewModel(ResourceLibary resourceLibary, AnimationPlayerViewModel animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _animationPlayer = animationPlayer;

            Load();
        }


        void Load()
        {
            try
            {
                var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\animation_tables\animation_tables.animpack");
                _animationPackData = new AnimationPackLoader();
                _animationPackData.Load(new ByteChunk(file.Data));

                var skeltonNames = _animationPackData.AnimationTableEntries
                    .Select(x => x.SkeletonName)
                    .Distinct()
                    .OrderBy(x => x); ;
        
                foreach (var skeletonName in skeltonNames)
                    SkeletonNameList.Add(skeletonName);
        
                SelectedSkelton = "humanoid01";
            }
            catch (Exception exception)
            {
                var error = $"Error loading AnimationPack : {exception.Message}";
                _logger.Error(error);
            }
       
        }

        void OnSkeletonSelected(string newSkeletonName)
        {
            using (new DisableCallbacks(this))
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
            //var item = _animationPackData.AnimationTableEntries.FirstOrDefault(x => x.Name == animationSet);
            //
            //if (item != null)
            //{
            //    List<AnimationFragmentCollection> fragmentCollections = new List<AnimationFragmentCollection>();
            //    foreach (var entry in item.AnimationSets)
            //    {
            //        var foundFragments = _animationPackData.AnimationFragments.Where(x => x.FileName.Contains(entry.Name));
            //        foreach (var fragment in foundFragments)
            //            fragmentCollections.Add(fragment);
            //
            //    }
            //
            //    var fragmentList = new Dictionary<int, Dictionary<string, List<AnimationFragmentItem>>>();
            //    foreach (var collction in fragmentCollections)
            //    {
            //        foreach (var fragment in collction.AnimationFragments)
            //        {
            //            if (!fragmentList.ContainsKey(fragment.Slot))
            //                fragmentList.Add(fragment.Slot, new Dictionary<string, List<AnimationFragmentItem>>());
            //
            //            if (!fragmentList[fragment.Slot].ContainsKey(collction.FileName))
            //                fragmentList[fragment.Slot].Add(collction.FileName, new List<AnimationFragmentItem>());
            //
            //            fragmentList[fragment.Slot][collction.FileName].Add(fragment);
            //        }
            //    }
            //
            //    foreach (var collection in FragmentCollectionList)
            //        collection.SelectionChanged -= OnChanged;
            //
            //    FragmentCollectionList.Clear();
            //    foreach (var collection in fragmentCollections)
            //        FragmentCollectionList.Add(new FragmentCollectionViewItem(collection));
            //
            //    foreach (var collection in FragmentCollectionList)
            //        collection.SelectionChanged += OnChanged;
            //
            //    var finalFragList = new List<AnimationFragmentItem>();
            //    foreach (var fragment in fragmentList)
            //    {
            //        var currentSlot = fragment.Key;
            //        var allOptions = fragment.Value;
            //        var selectedValue = allOptions.Last();
            //        foreach (var value in selectedValue.Value)
            //            finalFragList.Add(value);
            //    }
            //}
        }

        void OnChanged(bool value)
        {
            var all = FragmentCollectionList
                .Where(x => x.IsSelected)
                .Select(x => x.Collection)
                .ToList();
        }

        void PopulatePossibleMountCollections()
        {
            SelectedMount = null;

            if (string.IsNullOrWhiteSpace(SelectedSkelton))
            {
                MountNameList.Clear();
            }
            else
            {
                var mountNames = _animationPackData.AnimationTableEntries
                    .Where(x => x.SkeletonName == SelectedSkelton)
                    .Select(x => x.MountName)
                    .Distinct()
                    .OrderBy(x => x);

                MountNameList.Clear();
                foreach (var mountName in mountNames)
                    MountNameList.Add(mountName);
            }
        }


        void PopulatePossibleAnimationSets()
        {
            SelectedAnimationSet = null;

            var animationSets = _animationPackData.AnimationTableEntries.Where(x => x.SkeletonName == SelectedSkelton);

            if (!string.IsNullOrWhiteSpace(SelectedMount))
                animationSets = animationSets.Where(x => x.MountName == SelectedMount);

            var animationSetNames = animationSets.Select(x => x.Name)
                                                 .Distinct()
                                                 .OrderBy(x => x);

            AnimationSetList.Clear();
            foreach (var name in animationSetNames)
                AnimationSetList.Add(name);
        }



        void IsInFocus(bool isInFocus)
        {
            if (isInFocus)
                _animationPlayer.SetAnimationClip(null, null);

        }
    }
}
