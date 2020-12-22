using Common;
using Filetypes.RigidModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer;
using Viewer.Scene;

namespace VariantMeshEditor.Util
{
    public class AnimationToSkeletonTypeHelper
    {
        ILogger _logger = Logging.Create<AnimationSplicerViewModel>();
        Dictionary<string, List<PackedFile>> _skeletonNameToAnimationMap = new Dictionary<string, List<PackedFile>>();

        public void FindAllAnimations(ResourceLibary resourceLibary)
        {
            _logger.Here().Information("Finding all animations");

            var AllAnimations = PackFileLoadHelper.GetAllWithExtention(resourceLibary.PackfileContent, "anim");

            _logger.Here().Information("Animations found =" + AllAnimations.Count());

            foreach (var animation in AllAnimations)
            {
                try
                {
                    var animationSkeletonName = AnimationFile.GetAnimationHeader(animation).SkeletonName;
                    if (_skeletonNameToAnimationMap.ContainsKey(animationSkeletonName) == false)
                        _skeletonNameToAnimationMap.Add(animationSkeletonName, new List<PackedFile>());

                    _skeletonNameToAnimationMap[animationSkeletonName].Add(animation);

                }
                catch (Exception e)
                {
                    _logger.Here().Error("Parsing failed for " + animation.FullPath + "\n" + e.ToString());
                }
            }

            _logger.Here().Information("Finding all done");
        }

        public List<PackedFile> GetAnimationsForSkeleton(string skeletonName)
        {
            if (_skeletonNameToAnimationMap.ContainsKey(skeletonName) == false)
                return new List<PackedFile>();
            return _skeletonNameToAnimationMap[skeletonName];
        }
    }

}
