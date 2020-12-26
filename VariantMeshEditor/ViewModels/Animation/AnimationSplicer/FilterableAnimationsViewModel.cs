using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonDialogs.FilterDialog.FilterUserControl;
using Viewer.Scene;
using VariantMeshEditor.Services;
using Serilog;
using System.Collections.ObjectModel;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;
using VariantMeshEditor.Util;
using Newtonsoft.Json;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.Settings;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{

    public class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<FilterableAnimationsViewModel>();

        FilterableAnimationSetttings _data = new FilterableAnimationSetttings();

        // Animation selection
        ObservableCollection<PackedFile> _animationList;
        public ObservableCollection<PackedFile> AnimationsForCurrentSkeleton { get { return _animationList; } set { SetAndNotify(ref _animationList, value); } }

        public string _currentSkeletonName;
        public string CurrentSkeletonName { get { return _currentSkeletonName; } set { SetAndNotify(ref _currentSkeletonName, value); } }

        public string HeaderText { get { return _data.HeaderText; } set { SetAndNotify(ref _data.HeaderText, value); } }

        public event ValueChangedDelegate<PackedFile> SelectedAnimationChanged;
        public PackedFile SelectedAnimation { get { return _data.SelectedAnimation; } set { SetAndNotify(ref _data.SelectedAnimation, value, SelectedAnimationChanged); } }

        // Skeleton Selection
        public AnimationFile SkeletonFile { get; set; }
        public List<PackedFile> SkeletonList { get; set; } = new List<PackedFile>();
        public PackedFile SelectedSkeleton { get { return _data.SelectedSkeleton; } set { SetAndNotify(ref _data.SelectedSkeleton, value); OnSkeletonSelected(_data.SelectedSkeleton); } }

        public event ValueChangedDelegate<FilterableAnimationsViewModel> SelectedSkeletonChanged;
        public bool EnableSkeltonBrowsing { get { return _data.EnableSkeltonBrowsing; } set { SetAndNotify(ref _data.EnableSkeltonBrowsing, value); } }

        // Misc
        public OnSeachDelegate FilterByFullPath { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }
        ResourceLibary _resourceLibary;
        SkeletonAnimationLookUpHelper _skeletonAnimationLookUpHelper;


        // Mapping settings
        public TimeMatchMethod MatchingMethod { get { return _data.MatchingMethod; } set { SetAndNotify(ref _data.MatchingMethod, value); } }
        public ObservableCollection<SkeletonBoneNode> SelectedSkeletonBonesFlattened { get; set; } = new ObservableCollection<SkeletonBoneNode>();

        void FindAllSkeletons(ResourceLibary resourceLibary)
        {
            if (!EnableSkeltonBrowsing)
                return;
            var allFilesInFolder = PackFileLoadHelper.GetAllFilesInDirectory(resourceLibary.PackfileContent, "animations\\skeletons");
            SkeletonList = allFilesInFolder.Where(x => x.FileExtention == "anim").ToList();
        }

        void OnSkeletonSelected(PackedFile selectedSkeleton)
        {
            _logger.Here().Information($"Selecting a new skeleton: {selectedSkeleton}");

            SelectedSkeletonBonesFlattened.Clear();
            AnimationsForCurrentSkeleton.Clear();
            SelectedAnimation = null;
            CurrentSkeletonName = "";

            if (selectedSkeleton != null)
            {
                // Create the skeleton
                SkeletonFile = AnimationFile.Create(selectedSkeleton);
                SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = -1, BoneName = "" });
                foreach (var bone in SkeletonFile.Bones)
                    SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = bone.Id, BoneName = bone.Name });

                // Find all the animations for this skeleton
                var animations = _skeletonAnimationLookUpHelper.GetAnimationsForSkeleton(SkeletonFile.Header.SkeletonName);
                foreach (var animation in animations)
                    AnimationsForCurrentSkeleton.Add(animation);

                CurrentSkeletonName = SkeletonFile.Header.SkeletonName;
            }

            SelectedSkeletonChanged?.Invoke(this);
            _logger.Here().Information("Selecting a new skeleton - Done");
        }


        public FilterableAnimationsViewModel(string headerText, ResourceLibary resourceLibary, SkeletonAnimationLookUpHelper animationToSkeletonTypeHelper, bool enableSkeltonBrowsing)
        {
            HeaderText = headerText;
            _resourceLibary = resourceLibary;
            _skeletonAnimationLookUpHelper = animationToSkeletonTypeHelper;
            EnableSkeltonBrowsing = enableSkeltonBrowsing;
            AnimationsForCurrentSkeleton = new ObservableCollection<PackedFile>();

            FindAllSkeletons(_resourceLibary);
        }

        //public void Serialize()
        //{ 
        //    
        //}
        //
        //public void Load(FilterableAnimationSetttings settings)
        //{
        //    HeaderText = settings.HeaderText;
        //    EnableSkeltonBrowsing = settings.EnableSkeltonBrowsing;
        //    MatchingMethod = settings.MatchingMethod;
        //    SelectedSkeleton = settings.SelectedSkeleton;
        //    SelectedAnimation = settings.SelectedAnimation;
        //
        //    JsonConvert.SerializeObject()
        //    JsonSerializer.ser
        //    var con = JsonSerializer.CreateDefault();
        //    con.Serialize()
        //}
    }
}
