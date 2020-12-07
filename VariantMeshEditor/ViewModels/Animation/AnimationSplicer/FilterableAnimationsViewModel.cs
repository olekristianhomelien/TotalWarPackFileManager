using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonDialogs.Common.NotifyPropertyChangedImpl;
using static CommonDialogs.FilterDialog.FilterUserControl;
using Viewer.Scene;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        List<PackedFile> _filterList;

        public List<PackedFile> CurrentItems { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }
        List<PackedFile> AllAnimations { get; set; } = new List<PackedFile>();
        List<PackedFile> AnimationsForCurrentSkeleton { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }

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

            OnlyDisplayAnimationsForCurrentSkeleton = true;
        }
    }
}
