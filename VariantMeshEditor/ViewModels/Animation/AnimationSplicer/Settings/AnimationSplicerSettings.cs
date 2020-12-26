using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Services;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.Settings
{
    class AnimationSplicerSettings
    {
        public FilterableAnimationSetttings Target;
        public FilterableAnimationSetttings External;

        public MainAnimation SelectedMainAnimation;
        public BoneCopyMethod DefaultBoneCopyMethod;

        public IEnumerable<MappableSkeletonBone> MappableBoneSettings;
    }

    public class FilterableAnimationSetttings
    {
        public string HeaderText;
        public bool EnableSkeltonBrowsing;

        [JsonIgnore]
        public PackedFile SelectedAnimation;
        [JsonIgnore]
        public PackedFile SelectedSkeleton;

        public string SelectedAnimationFileName { get; set; }
        public string SelectedSkeletonFileName { get; set; }

        public TimeMatchMethod MatchingMethod = TimeMatchMethod.TimeFit;
    }
}
