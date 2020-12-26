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
        public FilterableAnimationSetttings TargetAnimation { get; set; }
        public FilterableAnimationSetttings ExternalAnimation { get; set; }

        public MainAnimation SelectedMainAnimation { get; set; }
        public BoneCopyMethod DefaultBoneCopyMethod { get; set; }

        public IEnumerable<MappedSkeletonBoneConfig> MappableBoneSettings { get; set; }

        public void PreperForSave()
        {
            TargetAnimation.PreperForSave();
            ExternalAnimation.PreperForSave();
        }
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

        public void PreperForSave()
        {
            SelectedAnimationFileName = SelectedAnimation?.FullPath;
            SelectedSkeletonFileName = SelectedSkeleton?.FullPath;
        }
    }
}
