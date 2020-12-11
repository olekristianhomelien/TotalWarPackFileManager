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

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        List<PackedFile> _filterList;

        public List<PackedFile> CurrentItems { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }
        List<PackedFile> AllAnimations { get; set; } = new List<PackedFile>();
        List<PackedFile> AnimationsForCurrentSkeleton { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }


        public string _currentSkeletonName;
        public string CurrentSkeletonName { get { return _currentSkeletonName; } set { SetAndNotify(ref _currentSkeletonName, value); } }

        public string _headerText;
        public string HeaderText { get { return _headerText; } set { SetAndNotify(ref _headerText, value); } }
       

        public event ValueChangedDelegate<PackedFile> SelectionChanged;
        PackedFile _selectedAnimation;
        public PackedFile SelectedItem { get { return _selectedAnimation; } set { SetAndNotify(ref _selectedAnimation, value, SelectionChanged); } }

        bool _onlyDisplayAnimationsForCurrentSkeleton = true;
        public bool OnlyDisplayAnimationsForCurrentSkeleton
        {
            get { return _onlyDisplayAnimationsForCurrentSkeleton; }
            set
            {
                SetAndNotify(ref _onlyDisplayAnimationsForCurrentSkeleton, value);
                if (value)
                    CurrentItems = AnimationsForCurrentSkeleton;
                else
                    CurrentItems = AllAnimations;
            }
        }


        public FrameTypes _defaultFrameTypesToCopy = FrameTypes.Both;
        public FrameTypes DefaultFrameTypesToCopy { get { return _defaultFrameTypesToCopy; } set { SetAndNotify(ref _defaultFrameTypesToCopy, value); } }

        public TransformTypes _defaultTransformTypesToCopy = TransformTypes.Both;
        public TransformTypes DefaultTransformTypesToCopy { get { return _defaultTransformTypesToCopy; } set { SetAndNotify(ref _defaultTransformTypesToCopy, value); } }

        public MatchMethod _matchingMethod = MatchMethod.TimeFit;
        public MatchMethod MatchingMethod { get { return _matchingMethod; } set { SetAndNotify(ref _matchingMethod, value); } }

        public MergeMethod _mergeMethod = MergeMethod.Replace;
        public MergeMethod MergeMethod { get { return _mergeMethod; } set { SetAndNotify(ref _mergeMethod, value); } }


        public FilterableAnimationsViewModel(string headerText)
        {
            HeaderText = headerText;
        }

        public void FindAllAnimations(ResourceLibary resourceLibary, string skeletonName)
        {
            AllAnimations = PackFileLoadHelper.GetAllWithExtention(resourceLibary.PackfileContent, "anim");
            AnimationsForCurrentSkeleton.Clear();

            foreach (var animation in AllAnimations)
            {
                var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(animation.Data)).SkeletonName;
                if (animationSkeletonName == skeletonName)
                    AnimationsForCurrentSkeleton.Add(animation);
            }

            CurrentSkeletonName = skeletonName;
            OnlyDisplayAnimationsForCurrentSkeleton = true;
        }
    }
}
