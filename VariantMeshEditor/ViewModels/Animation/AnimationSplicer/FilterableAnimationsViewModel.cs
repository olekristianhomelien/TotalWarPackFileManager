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

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<FilterableAnimationsViewModel>();

        // Animation selection
        ObservableCollection<PackedFile> _animationList;
        public ObservableCollection<PackedFile> AnimationsForCurrentSkeleton { get { return _animationList; } set { SetAndNotify(ref _animationList, value); } }

        public string _currentSkeletonName;
        public string CurrentSkeletonName { get { return _currentSkeletonName; } set { SetAndNotify(ref _currentSkeletonName, value); } }

        public string _headerText;
        public string HeaderText { get { return _headerText; } set { SetAndNotify(ref _headerText, value); } }

        public event ValueChangedDelegate<PackedFile> SelectedAnimationChanged;
        PackedFile _selectedAnimation;
        public PackedFile SelectedAnimation { get { return _selectedAnimation; } set { SetAndNotify(ref _selectedAnimation, value, SelectedAnimationChanged); } }



        // Skeleton Selection
        public AnimationFile SkeletonFile { get; set; }

        public List<PackedFile> SkeletonList { get; set; } = new List<PackedFile>();

        PackedFile _selectedSkeleton;
        public PackedFile SelectedSkeleton { get { return _selectedSkeleton; } set { SetAndNotify(ref _selectedSkeleton, value); OnSkeletonSelected(_selectedSkeleton); } }

        public event ValueChangedDelegate<FilterableAnimationsViewModel> SelectedSkeletonChanged;

        public bool EnableSkeltonBrowsing { get; set; }


        // Misc
        public OnSeachDelegate FilterByFullPath { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }
        ResourceLibary _resourceLibary;
        AnimationToSkeletonTypeHelper _animationToSkeletonTypeHelper;


        // Mapping settings
        public TimeMatchMethod _matchingMethod = TimeMatchMethod.TimeFit;
        public TimeMatchMethod MatchingMethod { get { return _matchingMethod; } set { SetAndNotify(ref _matchingMethod, value); } }
        public ObservableCollection<SkeletonBoneNode> SelectedSkeletonBonesFlattened { get; set; } = new ObservableCollection<SkeletonBoneNode>();

        void FindAllSkeletons(ResourceLibary resourceLibary)
        {
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
                var animations = _animationToSkeletonTypeHelper.GetAnimationsForSkeleton(SkeletonFile.Header.SkeletonName);
                foreach (var animation in animations)
                    AnimationsForCurrentSkeleton.Add(animation);

                CurrentSkeletonName = SkeletonFile.Header.SkeletonName;
            }

            SelectedSkeletonChanged?.Invoke(this);
            _logger.Here().Information("Selecting a new skeleton - Done");
        }


        public FilterableAnimationsViewModel(string headerText, ResourceLibary resourceLibary, AnimationToSkeletonTypeHelper animationToSkeletonTypeHelper, bool enableSkeltonBrowsing)
        {
            HeaderText = headerText;
            _resourceLibary = resourceLibary;
            _animationToSkeletonTypeHelper = animationToSkeletonTypeHelper;
            EnableSkeltonBrowsing = enableSkeltonBrowsing;
            AnimationsForCurrentSkeleton = new ObservableCollection<PackedFile>();

            FindAllSkeletons(_resourceLibary);
        }
    }
}
