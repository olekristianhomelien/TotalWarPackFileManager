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

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<FilterableAnimationsViewModel>();

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
        


        public TimeMatchMethod _matchingMethod = TimeMatchMethod.TimeFit;
        public TimeMatchMethod MatchingMethod { get { return _matchingMethod; } set { SetAndNotify(ref _matchingMethod, value); } }

  

        
        public FilterableAnimationsViewModel(string headerText)
        {
            HeaderText = headerText;
        }

        public void FindAllAnimations(ResourceLibary resourceLibary, string skeletonName)
        {
            _logger.Here().Information("Finding all animations");

            AllAnimations = PackFileLoadHelper.GetAllWithExtention(resourceLibary.PackfileContent, "anim");
            AnimationsForCurrentSkeleton.Clear();

            _logger.Here().Information("Animations found =" + AllAnimations.Count());

            foreach (var animation in AllAnimations)
            {
                try
                {
                    var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(animation.Data)).SkeletonName;
                    if (animationSkeletonName == skeletonName)
                        AnimationsForCurrentSkeleton.Add(animation);
                }
                catch (Exception e)
                {
                    _logger.Here().Error("Parsing failed for " + animation.FullPath + "\n" + e.ToString());
                }
            }

            CurrentSkeletonName = skeletonName;
            OnlyDisplayAnimationsForCurrentSkeleton = true;

            _logger.Here().Information("Finding all done");
        }
    }
}
