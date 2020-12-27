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
using Viewer.Animation;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{

    public class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<FilterableAnimationsViewModel>();

        public FilterableAnimationSetttings Data { get; private set; } = new FilterableAnimationSetttings();

        // Animation selection
        ObservableCollection<PackedFile> _animationList;
        public ObservableCollection<PackedFile> AnimationsForCurrentSkeleton { get { return _animationList; } set { SetAndNotify(ref _animationList, value); } }

        public string _currentSkeletonName;
        public string CurrentSkeletonName { get { return _currentSkeletonName; } set { SetAndNotify(ref _currentSkeletonName, value); } }

        public string HeaderText { get { return Data.HeaderText; } set { SetAndNotify(ref Data.HeaderText, value); } }

        public event ValueChangedDelegate<PackedFile> SelectedAnimationChanged;
        public PackedFile SelectedAnimation { get { return Data.SelectedAnimation; } set { SetAndNotify(ref Data.SelectedAnimation, value); OnAnimationChanged(Data.SelectedAnimation); } }

        public AnimationClip SelectedAnimationClip { get; set; }

        // Skeleton Selection
        public List<PackedFile> SkeletonList { get; set; } = new List<PackedFile>();
        public PackedFile SelectedSkeleton { get { return Data.SelectedSkeleton; } set { SetAndNotify(ref Data.SelectedSkeleton, value); OnSkeletonSelected(Data.SelectedSkeleton); } }

        public event ValueChangedDelegate<FilterableAnimationsViewModel> SelectedSkeletonChanged;
        public bool EnableSkeltonBrowsing { get { return Data.EnableSkeltonBrowsing; } set { SetAndNotify(ref Data.EnableSkeltonBrowsing, value); } }
        public GameSkeleton SelectedGameSkeleton { get; set; }

        // Misc
        public OnSeachDelegate FilterByFullPath { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }
        ResourceLibary _resourceLibary;
        SkeletonAnimationLookUpHelper _skeletonAnimationLookUpHelper;


        // Mapping settings
        public TimeMatchMethod MatchingMethod { get { return Data.MatchingMethod; } set { SetAndNotify(ref Data.MatchingMethod, value); } }
        public ObservableCollection<SkeletonBoneNode> SelectedSkeletonBonesFlattened { get; set; } = new ObservableCollection<SkeletonBoneNode>();
        public FilterableAnimationsViewModel(string headerText, ResourceLibary resourceLibary, SkeletonAnimationLookUpHelper animationToSkeletonTypeHelper, bool enableSkeltonBrowsing)
        {
            HeaderText = headerText;
            _resourceLibary = resourceLibary;
            _skeletonAnimationLookUpHelper = animationToSkeletonTypeHelper;
            EnableSkeltonBrowsing = enableSkeltonBrowsing;
            AnimationsForCurrentSkeleton = new ObservableCollection<PackedFile>();

            FindAllSkeletons(_resourceLibary);
        }

        void OnSkeletonSelected(PackedFile selectedSkeleton)
        {
            _logger.Here().Information($"Selecting a new skeleton: {selectedSkeleton}");

            SelectedSkeletonBonesFlattened.Clear();
            AnimationsForCurrentSkeleton.Clear();
            SelectedAnimation = null;
            CurrentSkeletonName = "";
            SelectedGameSkeleton = null;

            if (selectedSkeleton != null)
            {
                // Create the skeleton
                var skeletonFile = AnimationFile.Create(selectedSkeleton);
                SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = -1, BoneName = "" });
                foreach (var bone in skeletonFile.Bones)
                    SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = bone.Id, BoneName = bone.Name });

                // Find all the animations for this skeleton
                var animations = _skeletonAnimationLookUpHelper.GetAnimationsForSkeleton(skeletonFile.Header.SkeletonName);
                foreach (var animation in animations)
                    AnimationsForCurrentSkeleton.Add(animation);

                CurrentSkeletonName = skeletonFile.Header.SkeletonName;
                SelectedGameSkeleton = new GameSkeleton(skeletonFile, null);
            }

            SelectedSkeletonChanged?.Invoke(this);
            _logger.Here().Information("Selecting a new skeleton - Done");
        }

        void OnAnimationChanged(PackedFile selectedAnimation)
        {
            _logger.Here().Information($"Selecting a new animation: {selectedAnimation}");

            if (selectedAnimation == null)
            {
                SelectedAnimationClip = null;
            }
            else
            {
                var anim = AnimationFile.Create(selectedAnimation);
                SelectedAnimationClip = new AnimationClip(anim);
            }

            SelectedAnimationChanged?.Invoke(selectedAnimation);
            _logger.Here().Information($"Selecting a new animation - Done");
        }

        void FindAllSkeletons(ResourceLibary resourceLibary)
        {
            if (!EnableSkeltonBrowsing)
                return;
            var allFilesInFolder = PackFileLoadHelper.GetAllFilesInDirectory(resourceLibary.PackfileContent, "animations\\skeletons");
            SkeletonList = allFilesInFolder.Where(x => x.FileExtention == "anim").ToList();
        }
    }
}
