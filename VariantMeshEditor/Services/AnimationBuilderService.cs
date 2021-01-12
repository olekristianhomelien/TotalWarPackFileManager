using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping;
using Viewer.Animation;

namespace VariantMeshEditor.Services
{
    // Per bone configurations
    public enum TransformTypes
    {
        Both,
        Rotation,
        Translation,
    }

    public enum BoneCopyMethod
    {
        Ratio,
        Relative,
    }

    public enum RatioScaleMethod
    {
        Larger,
        Smaller
    }

    // Whole animation configurations
    public enum TimeMatchMethod
    {
        TimeFit,
        HoldLastFrame
    }

    public enum MainAnimation
    {
        Source,
        Other
    }

    public class AnimationBuilderService
    {
        public class AnimationBuilderSettings
        {
            public GameSkeleton SourceSkeleton { get; set; }
            public AnimationClip SourceAnimationClip { get; set; }

            public GameSkeleton OtherSkeletonFile { get; set; }
            public AnimationClip OtherAnimationClip { get; set; }

            public IEnumerable<AdvBoneMappingBone> BoneSettings { get; set; }
            public MainAnimation SelectedMainAnimation { get; set; }
        }

        static bool Validate(AnimationBuilderSettings settings)
        {
            if (settings.SourceSkeleton == null)
                return false;

            if (settings.OtherSkeletonFile == null)
                return false;

            if (settings.OtherAnimationClip == null)
                return false;

            if (settings.BoneSettings == null || settings.BoneSettings.Count() == 0)
                return false;
            return true;
        }
        /*
                class ProcssConfiguration
                {
                    public GameSkeleton SourceSkeleton { get; set; }
                    public AnimationClip SourceAnimation { get; set; }
                    public GameSkeleton OtherSkeleton { get; set; }
                    public AnimationClip OtherAnimationClip { get; set; }
                    public AdvBoneMappingBone MappingInfo { get; set; }
                }

                static float ComputeBoneRatio(ProcssConfiguration settings)
                {
                    var sourceBoneLength = GetBoneLength(settings.SourceSkeleton, settings.MappingInfo.BoneIndex);
                    var otherBoneLength = GetBoneLength(settings.OtherSkeleton, settings.MappingInfo.Settings.MappingBoneId);
                    float boneRatio = sourceBoneLength / otherBoneLength;
                    if (float.IsNaN(boneRatio))
                        boneRatio = 1;
                    return boneRatio;
                }


                static void GetSkeletonAndAnimationTransform(ProcssConfiguration configuration, out Quaternion skeletonRotation, out Vector3 skeletonPosition, out Quaternion animatedRotation, out Vector3 animatedPosition, int frameIndex)
                {
                    var mappedBondeIndex = configuration.MappingInfo.Settings.MappingBoneId;

                    GetSkeletonTransform(configuration.OtherSkeleton, mappedBondeIndex, out skeletonRotation, out skeletonPosition);

                    animatedPosition = skeletonPosition;
                    animatedRotation = skeletonRotation;

                    if (HasAnimationData(mappedBondeIndex, configuration.OtherAnimationClip))
                        GetAnimationTransform(configuration.OtherAnimationClip, frameIndex, configuration.OtherSkeleton, mappedBondeIndex, out animatedRotation, out animatedPosition);
                }

                static void ProcessSmartDirect(ProcssConfiguration configuration, float interpolationValue, ref AnimationClip outputAnimation, int outputAnimationFrameIndex, int outputAnimationBoneIndex)
                {
                    GetSkeletonTransform(configuration.SourceSkeleton, configuration.MappingInfo.BoneIndex, out var sourceSkeletonRotation, out var sourceSkeletonPosition);
                    GetSkeletonAndAnimationTransform(configuration, out var otherSkeletonRotation, out _, out var otherAnimatedRotation, out var otherAnimatedPosition, outputAnimationBoneIndex); // Change to interplate
                    var boneRatio = ComputeBoneRatio(configuration);

                    //------------------
                    var mappingSettings = configuration.MappingInfo.Settings as DirectSmartAdvBoneMappingBoneSettings;
                    if (mappingSettings.Ratio_ScaleRotation)
                    {
                        otherAnimatedRotation.ToAxisAngle(out Vector3 axis, out float angle);
                        otherAnimatedRotation = Quaternion.CreateFromAxisAngle(axis, angle * boneRatio);
                    }

                    var skeletonRotationDifference = Quaternion.Identity;
                    if (mappingSettings.Ratio_ScaleMethod == RatioScaleMethod.Larger)
                        skeletonRotationDifference = otherSkeletonRotation * Quaternion.Inverse(sourceSkeletonRotation);
                    else
                        skeletonRotationDifference = sourceSkeletonRotation * Quaternion.Inverse(otherSkeletonRotation);

                    var rotation = (otherAnimatedRotation * skeletonRotationDifference);
                    rotation.Normalize();

                    outputAnimation.DynamicFrames[outputAnimationFrameIndex].Rotation[outputAnimationBoneIndex] = rotation;
                    outputAnimation.DynamicFrames[outputAnimationFrameIndex].Position[outputAnimationBoneIndex] = otherAnimatedPosition * boneRatio;
                }

                static void Loop(GameSkeleton sourceSkeleton, AnimationClip sourceAnimation, GameSkeleton otherSkeleton, AnimationClip otherAnimationClip, IEnumerable<AdvBoneMappingBone> boneSettings, int outputFrameCount, Dictionary<BoneMappingType, BoneProcessFunc> processMap, ref AnimationClip outputAnimation)
                {
                    for (int frameIndex = 0; frameIndex < outputFrameCount; frameIndex++)
                    {
                        for (int boneIndex = 0; boneIndex < sourceSkeleton.BoneCount; boneIndex++)
                        {
                            var boneToGetAnimDataFrom = GetMappedBone(boneSettings, boneIndex);

                            if (HasValidMapping(boneToGetAnimDataFrom))
                            {
                                float t = frameIndex / (float)outputFrameCount;

                                ProcssConfiguration configuration = new ProcssConfiguration()
                                {
                                    SourceSkeleton = sourceSkeleton,
                                    SourceAnimation = sourceAnimation,
                                    OtherSkeleton = otherSkeleton,
                                    OtherAnimationClip = otherAnimationClip,
                                    MappingInfo = boneToGetAnimDataFrom,
                                };

                                if (processMap.ContainsKey(boneToGetAnimDataFrom.MappingType))
                                    processMap[boneToGetAnimDataFrom.MappingType](configuration, t, ref outputAnimation, frameIndex, boneIndex);
                            }
                        }
                    }
                }

                delegate void BoneProcessFunc(ProcssConfiguration configuration, float interpolationValue, ref AnimationClip outputAnimation, int outputAnimationFrameIndex, int outputAnimationBoneIndex);


                static AnimationClip NewAnimationProcessor(GameSkeleton sourceSkeleton, AnimationClip sourceAnimation, GameSkeleton otherSkeleton, AnimationClip otherAnimationClip, IEnumerable<AdvBoneMappingBone> boneSettings)
                {
                    var outputAnimationFrameCount = 20;
                    var outputAnimation = CreateOutputAnimation(sourceSkeleton, outputAnimationFrameCount);

                    var normalMappings = new Dictionary<BoneMappingType, BoneProcessFunc>();
                    normalMappings.Add(BoneMappingType.Direct, null);
                    normalMappings.Add(BoneMappingType.Direct_smart, ProcessSmartDirect);
                    Loop(sourceSkeleton, sourceAnimation, otherSkeleton, otherAnimationClip, boneSettings, outputAnimationFrameCount, normalMappings, ref outputAnimation);

                    var postProcessingMappings = new Dictionary<BoneMappingType, BoneProcessFunc>();
                    postProcessingMappings.Add(BoneMappingType.AttachmentPoint, null);
                    Loop(sourceSkeleton, sourceAnimation, otherSkeleton, otherAnimationClip, boneSettings, outputAnimationFrameCount, postProcessingMappings, ref outputAnimation);

                    return outputAnimation;
                }*/


        class AnimationBuilderSampler
        {
            GameSkeleton _skeleton; 
            AnimationClip _animationClip;
            public AnimationBuilderSampler(GameSkeleton skeleton, AnimationClip animationClip)
            {
                _skeleton = skeleton;
                _animationClip = animationClip;
            }

            public bool HasAnimationData(int boneIndex)
            {
                if (_animationClip == null)
                    return false;

                var remappedId = _animationClip.RotationMappings[boneIndex].Id;
                if (remappedId != -1)
                    return true;

                remappedId = _animationClip.TranslationMappings[boneIndex].Id;
                if (remappedId != -1)
                    return true;

                return true;
            }


            public AnimationFrameTransform GetAnimationTransform(int boneIndex, int frameIndex)
            {
                var safeFameIndex = frameIndex % _animationClip.DynamicFrames.Count();
                var transform = GetSkeletonTransform(boneIndex);
                transform = GetTransformFromAnimation(_animationClip.StaticFrame, boneIndex,  AnimationFile.AnimationBoneMappingType.Static, transform);
                transform = GetTransformFromAnimation(_animationClip.DynamicFrames[safeFameIndex], boneIndex, AnimationFile.AnimationBoneMappingType.Dynamic, transform);

                return transform;
            }

            public AnimationFrameTransform GetAnimationTransform(int boneIndex, float t)
            {
                throw new NotImplementedException();
            }

            public AnimationFrameTransform GetSkeletonTransform(int boneIndex)
            {
                return new AnimationFrameTransform
                {
                    Position = _skeleton.Translation[boneIndex],
                    Rotation = _skeleton.Rotation[boneIndex]
                };
            }

            AnimationFrameTransform GetTransformFromAnimation(AnimationClip.KeyFrame frame, int boneIndex, AnimationFile.AnimationBoneMappingType mappingType,AnimationFrameTransform existingTransform = null)
            {
                AnimationFrameTransform transform = new AnimationFrameTransform();
                if(existingTransform != null)
                    transform = existingTransform.Copy();

                if (frame != null)
                {
                    if (_animationClip.RotationMappings[boneIndex].MappingType == mappingType)
                    {
                        var otherRemappedId = _animationClip.RotationMappings[boneIndex].Id;
                        if (otherRemappedId != -1)
                            transform.Rotation = frame.Rotation[otherRemappedId];
                    }

                    if (_animationClip.TranslationMappings[boneIndex].MappingType == mappingType)
                    {
                        var otherRemappedId = _animationClip.TranslationMappings[boneIndex].Id;
                        if (otherRemappedId != -1)
                            transform.Position = frame.Position[otherRemappedId];
                    }
                }
                return transform;
            }

            public float GetBoneLength( int boneId)
            {
                var parentBoneId = _skeleton.GetParentBone(boneId);
                if (parentBoneId == -1)
                    return 0;

                var p0 = Vector3.Transform(Vector3.Zero, _skeleton.GetWorldTransform(boneId));
                var p1 = Vector3.Transform(Vector3.Zero, _skeleton.GetWorldTransform(parentBoneId));
                var dist = Vector3.Distance(p0, p1);
                return Math.Abs(dist);
            }
        }

        public class AnimationFrameTransform
        {
            public AnimationFrameTransform Copy()
            {
                return new AnimationFrameTransform()
                {
                    Position = Position,
                    Rotation = Rotation
                };
            }

            public Vector3 Position { get; set; } = Vector3.Zero;
            public Quaternion Rotation { get; set; } = Quaternion.Identity;
        }



        public static AnimationClip CreateMergedAnimation(AnimationBuilderSettings settings)
        {
            // Validate
            if (Validate(settings) == false)
                return null;

            var sourceAnimationClip = settings.SourceAnimationClip;
            var sourceSkeleton = settings.SourceSkeleton;
            var otherSkeleton = settings.OtherSkeletonFile;
            var otherAnimationClip = settings.OtherAnimationClip;
            var outputAnimationFile = CreateOutputAnimation(sourceSkeleton, otherAnimationClip.DynamicFrames.Count());

            //NewAnimationProcessor(sourceSkeleton, sourceAnimationClip, otherSkeleton, otherAnimationClip, settings.BoneSettings);

            for (int frameIndex = 0; frameIndex < otherAnimationClip.DynamicFrames.Count(); frameIndex++)
            {
                for (int boneIndex = 0; boneIndex < sourceSkeleton.BoneCount; boneIndex++)
                {
                    var rotation = Quaternion.Identity;
                    var position = Vector3.Zero;

                    var sourceBoneLength = GetBoneLength(sourceSkeleton, boneIndex);
                    var boneToGetAnimDataFrom = GetMappedBone(settings.BoneSettings, boneIndex);
              
                    if (HasValidMapping(boneToGetAnimDataFrom))
                    {
                        var mappedBondeIndex = boneToGetAnimDataFrom.Settings.MappingBoneId;
                        var otherBoneLength = GetBoneLength(otherSkeleton, mappedBondeIndex);

                        GetSkeletonTransform(sourceSkeleton, boneIndex, out Quaternion sourceSkeletonRotation, out Vector3 sourceSkeletonPosition);
                        GetSkeletonTransform(otherSkeleton, mappedBondeIndex, out Quaternion otherSkeletonRotation, out Vector3 otherSkeletonPosition);

                        Vector3 otherAnimatedPosition = otherSkeletonPosition;
                        Quaternion otherAnimatedRotation = otherSkeletonRotation;

                        float boneRatio = sourceBoneLength / otherBoneLength;
                        if (float.IsNaN(boneRatio))
                            boneRatio = 1;

                        if (HasAnimationData(mappedBondeIndex, otherAnimationClip))
                            GetAnimationTransform(otherAnimationClip, frameIndex, otherSkeleton, mappedBondeIndex, out otherAnimatedRotation, out otherAnimatedPosition);

                        if (boneToGetAnimDataFrom.MappingType == BoneMappingType.Direct_smart)
                        {
                            var mappingSettings = boneToGetAnimDataFrom.Settings as DirectSmartAdvBoneMappingBoneSettings;

                            if (mappingSettings.BoneCopyMethod == BoneCopyMethod.Ratio)
                            {
                                var ratio = sourceBoneLength / otherBoneLength;
                                if (float.IsNaN(ratio))
                                    ratio = 1;

                                if (mappingSettings.Ratio_ScaleRotation)
                                {
                                    otherAnimatedRotation.ToAxisAngle(out Vector3 axis, out float angle);
                                    otherAnimatedRotation = Quaternion.CreateFromAxisAngle(axis, angle * ratio);
                                }

                                var skeletonRotationDifference = Quaternion.Identity;
                                if (mappingSettings.Ratio_ScaleMethod == RatioScaleMethod.Larger)
                                    skeletonRotationDifference = otherSkeletonRotation * Quaternion.Inverse(sourceSkeletonRotation);
                                else
                                    skeletonRotationDifference = sourceSkeletonRotation * Quaternion.Inverse(otherSkeletonRotation);

                                position = (otherAnimatedPosition * ratio);
                                rotation = (otherAnimatedRotation * skeletonRotationDifference);
                            }
                            else if (mappingSettings.BoneCopyMethod == BoneCopyMethod.Relative)
                            {
                                var relativeRotation = otherAnimatedRotation * Quaternion.Inverse(otherSkeletonRotation);
                                var relativePosition = otherAnimatedPosition - otherSkeletonPosition;
                                rotation = (relativeRotation * sourceSkeletonRotation);
                                position = relativePosition + sourceSkeletonPosition;
                            }
                            else
                            {
                                throw new Exception($"Unsupported BoneCopyMethod - { mappingSettings.BoneCopyMethod }");
                            }
                        }
                        else if (boneToGetAnimDataFrom.MappingType == BoneMappingType.Direct)
                        {

                            var scale = 1.0f;
                            var mappingSettings = boneToGetAnimDataFrom.Settings as DirectAdvBoneMappingBoneSettings;
                            if (mappingSettings.ScaleSkeletonBasedOnBoneLength)
                                scale = boneRatio;

                            position = otherAnimatedPosition * scale;
                            rotation = otherAnimatedRotation;
                        }
                        else if (boneToGetAnimDataFrom.MappingType == BoneMappingType.AttachmentPoint)
                        {
                            // Handle later
                        }
                        else
                        {
                            throw new Exception($"Unsupported MappingType - { boneToGetAnimDataFrom.MappingType }");
                        }
                    }
                    else
                    {
                        GetSkeletonTransform(sourceSkeleton, boneIndex, out rotation, out position);

                        // Source animation is not mandatary
                        if (sourceAnimationClip != null)
                            GetAnimationTransform(sourceAnimationClip, frameIndex, sourceSkeleton, boneIndex, out rotation, out position);
                    }

                    ComputeMappedBoneAttributeContributions(boneToGetAnimDataFrom, ref rotation, ref position);
                    rotation.Normalize();
                    outputAnimationFile.DynamicFrames[frameIndex].Rotation.Add(rotation);
                    outputAnimationFile.DynamicFrames[frameIndex].Position.Add(position);
                }
            }

            for (int frameIndex = 0; frameIndex < outputAnimationFile.DynamicFrames.Count(); frameIndex++)
            {
                for (int boneIndex = 0; boneIndex < sourceSkeleton.BoneCount; boneIndex++)
                {
                    var mappedBone = GetMappedBone(settings.BoneSettings, boneIndex);
                    if (HasValidMapping(mappedBone))
                    {
                        var attachmentSettings = mappedBone.Settings as AttachmentPointAdvBoneMappingBoneSettings;
                        if (attachmentSettings != null)
                        {
                            var newAnimationClipFrame = AnimationSampler.Sample(frameIndex, 0, sourceSkeleton, outputAnimationFile);
                            var transformDiff = newAnimationClipFrame.GetSkeletonAnimatedWorldDiff(sourceSkeleton, 0, mappedBone.Settings.MappingBoneId);
                            transformDiff.Decompose(out _, out var rotation, out var position);

                            if (attachmentSettings.KeepOriginalRotation)
                            {
                               //var originalAnimationClipFrame = AnimationSampler.Sample(frameIndex, 0, otherSkeleton, otherAnimationClip);
                               //var originalTransformDiff = originalAnimationClipFrame.GetSkeletonAnimatedWorldDiff(otherSkeleton, 0, mappedBone.Settings.MappingBoneId);
                               //originalTransformDiff.Decompose(out _, out rotation, out _);
                            }

                            var positionOffset = Vector3.Zero;
                            ComputeMappedBoneAttributeContributions(mappedBone, ref rotation, ref positionOffset);

                            var positionOffsetLocalSpace = Vector3.Transform(positionOffset, rotation);
                            
                            outputAnimationFile.DynamicFrames[frameIndex].Rotation[mappedBone.BoneIndex] = Quaternion.Normalize(rotation);
                            outputAnimationFile.DynamicFrames[frameIndex].Position[mappedBone.BoneIndex] = position + positionOffsetLocalSpace;
                        }
                    }
                }
            }

            return outputAnimationFile;
        }
 

        static AnimationClip CreateOutputAnimation(GameSkeleton sourceSkeleton, int outputAnimationFrameCount)
        {
            var outputAnimationFile = new AnimationClip();
            for (int i = 0; i < sourceSkeleton.BoneCount; i++)
            {
                outputAnimationFile.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
                outputAnimationFile.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
            }

            for (int frameIndex = 0; frameIndex < outputAnimationFrameCount; frameIndex++)
                outputAnimationFile.DynamicFrames.Add(new AnimationClip.KeyFrame());

            return outputAnimationFile;
        }

        static bool HasAnimationData(int boneIndex, AnimationClip animation)
        {
            if (animation == null)
                return false;

            var remappedId = animation.RotationMappings[boneIndex].Id;
            if (remappedId != -1)
                return true;

            remappedId = animation.TranslationMappings[boneIndex].Id;
            if (remappedId != -1)
                return true;

            return true;
        }

        static void GetAnimationTransform(AnimationClip animation, int frameIndex, GameSkeleton skeleton, int boneIndex, out Quaternion out_rotation, out Vector3 out_position)
        {
            var safeFameIndex = frameIndex % animation.DynamicFrames.Count();

            // Copy the skeleton transform as default
            GetSkeletonTransform(skeleton, boneIndex, out out_rotation, out out_position);

            // Static
            ProcessFrame(animation.StaticFrame, boneIndex, animation.RotationMappings, animation.TranslationMappings,
                AnimationFile.AnimationBoneMappingType.Static, ref out_rotation, ref out_position);

            // Dynamic
            ProcessFrame(animation.DynamicFrames[safeFameIndex], boneIndex, animation.RotationMappings, animation.TranslationMappings,
                AnimationFile.AnimationBoneMappingType.Dynamic, ref out_rotation, ref out_position);
        }

        static void GetSkeletonTransform(GameSkeleton skeleton, int boneIndex, out Quaternion out_rotation, out Vector3 out_position)
        {
            out_rotation = skeleton.Rotation[boneIndex];
            out_position = skeleton.Translation[boneIndex];
        }

        static void ComputeMappedBoneAttributeContributions(AdvBoneMappingBone bone, ref Quaternion out_rotation, ref Vector3 out_position)
        {
            if (bone.Settings.UseConstantOffset)
            {
                //if (bone.RotationOffsetAlongPrimaryAxis.Value != 0)
                //{
                //    out_rotation.ToAxisAngle(out Vector3 axis, out float angle);
                //    out_rotation = Quaternion.CreateFromAxisAngle(axis, angle + MathHelper.ToRadians((float)bone.RotationOffsetAlongPrimaryAxis.Value));
                //}

                var x = Matrix.CreateRotationX(MathHelper.ToRadians((float)bone.Settings.ContantRotationOffset.X.Value));
                var y = Matrix.CreateRotationY(MathHelper.ToRadians((float)bone.Settings.ContantRotationOffset.Y.Value));
                var z = Matrix.CreateRotationZ(MathHelper.ToRadians((float)bone.Settings.ContantRotationOffset.Z.Value));
                var rotationMatrix = x * y * z;
                var rotationoffset = Quaternion.CreateFromRotationMatrix(rotationMatrix);

                out_position += MathConverter.ToVector3(bone.Settings.ContantTranslationOffset);
                out_rotation = out_rotation * rotationoffset;


                // scale
                out_position *= (float)bone.Settings.SkeletonScaleValue.Value;
            }
        }

        static void ProcessFrame(AnimationClip.KeyFrame frame, int boneIndex,
            List<AnimationFile.AnimationBoneMapping> rotationMapping,
            List<AnimationFile.AnimationBoneMapping> translationMapping,
            AnimationFile.AnimationBoneMappingType mappingType,
            ref Quaternion out_rotation, ref Vector3 out_position)
        {
            if (frame != null)
            {
                if (rotationMapping[boneIndex].MappingType == mappingType)
                {
                    var otherRemappedId = rotationMapping[boneIndex].Id;
                    if (otherRemappedId != -1)
                        out_rotation = frame.Rotation[otherRemappedId];
                }

                if (translationMapping[boneIndex].MappingType == mappingType)
                {
                    var otherRemappedId = translationMapping[boneIndex].Id;
                    if (otherRemappedId != -1)
                        out_position = frame.Position[otherRemappedId];
                }
            }
        }

        static bool HasValidMapping(AdvBoneMappingBone bone)
        {
            return bone.Settings.HasMapping && bone.Settings.UseMapping;
        }

        static int GetAnimationFrameCount(AnimationClip source, AnimationClip other, MainAnimation mainAnimationSetting)
        {
            if (mainAnimationSetting == MainAnimation.Source)
            {
                if (source == null)
                    throw new Exception("Source can not be the main animation if there is no animation selected for source");
                return source.DynamicFrames.Count;
            }

            if (mainAnimationSetting == MainAnimation.Other)
            {
                return other.DynamicFrames.Count;
            }

            throw new Exception("Unsupported value for MainAnimation provided");
        }

        static AdvBoneMappingBone GetMappedBone(IEnumerable<AdvBoneMappingBone> nodes, int boneIndex)
        {
            foreach (var node in nodes)
            {
                if (node.BoneIndex == boneIndex)
                    return node;
                var res = GetMappedBone(node.Children, boneIndex);
                if (res != null)
                    return res;
            }
            return null;
        }

        static float GetBoneLength(GameSkeleton skeleton, int boneId)
        {
            var parentBoneId = skeleton.GetParentBone(boneId);
            if (parentBoneId == -1)
                return 0;

            var p0 = Vector3.Transform(Vector3.Zero, skeleton.GetWorldTransform(boneId));
            var p1 = Vector3.Transform(Vector3.Zero, skeleton.GetWorldTransform(parentBoneId));

            var dist = Vector3.Distance(p0, p1);

            return Math.Abs(dist);
        }

    }


}