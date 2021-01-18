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

            int _currentFrameIndex = -1;
            AnimationFrame _frame = null;

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
                if (HasAnimationData(boneIndex))
                {
                    if (frameIndex == -1)
                    {
                        _currentFrameIndex = frameIndex;
                        _frame = AnimationSampler.Sample(-1, 0, _skeleton, _animationClip, true, false);
                    }
                    else
                    {

                        var safeFameIndex = frameIndex % _animationClip.DynamicFrames.Count();
                        if (_currentFrameIndex != frameIndex)
                        {
                            _currentFrameIndex = frameIndex;
                            _frame = AnimationSampler.Sample(safeFameIndex, 0, _skeleton, _animationClip);
                        }
                    }

                    return new AnimationFrameTransform(_frame.BoneTransforms[boneIndex].Translation, _frame.BoneTransforms[boneIndex].Rotation);
                }
                else
                {
                    return GetSkeletonTransform(boneIndex);
                }
            }

            public AnimationFrameTransform GetSkeletonTransform(int boneIndex)
            {
                return new AnimationFrameTransform(_skeleton.Translation[boneIndex], _skeleton.Rotation[boneIndex]);
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
            public AnimationFrameTransform(Vector3 pos, Quaternion rot)
            {
                Position = pos;
                Rotation = rot;
            }
            public Vector3 Position { get; set; } = Vector3.Zero;
            public Quaternion Rotation { get; set; } = Quaternion.Identity;
        }


        static public AnimationClip UpdateStaticPose(AnimationBuilderSettings settings)
        {
            var outputAnimationFile = new AnimationClip();
            outputAnimationFile.StaticFrame = new AnimationClip.KeyFrame();

            AnimationBuilderSampler sourceSampler = new AnimationBuilderSampler(settings.SourceSkeleton, settings.SourceAnimationClip);
            AnimationBuilderSampler otherSampler = new AnimationBuilderSampler(settings.OtherSkeletonFile, settings.OtherAnimationClip);

            if ( !(settings.OtherAnimationClip.DynamicFrames.Count() == 0 && settings.OtherAnimationClip.StaticFrame != null))
                throw new Exception("The animation is not a pose, dont use this method");

            int rotationCounter = 0;
            int translatonCounter = 0;
            for (int boneIndex = 0; boneIndex < settings.SourceSkeleton.BoneCount; boneIndex++)
            {
                var mappedBone = GetMappedBone(settings.BoneSettings, boneIndex);
                if (HasValidMapping(mappedBone))
                {
                    var otherIndex = mappedBone.Settings.MappingBoneId;
                    var otherRotMapping = settings.OtherAnimationClip.RotationMappings[otherIndex];
                    var otherTransMapping = settings.OtherAnimationClip.TranslationMappings[otherIndex];

                    var frameTransform = GetFrameTransform(settings, boneIndex, -1, sourceSampler, otherSampler);

                    if (otherRotMapping.IsStatic || true)
                    {
                        var r = new AnimationFile.AnimationBoneMapping(10000 + rotationCounter++);
                        outputAnimationFile.RotationMappings.Add(r);
                        outputAnimationFile.StaticFrame.Rotation.Add(frameTransform.Rotation);
                    }
                    else
                    {
                        outputAnimationFile.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(-1));
                    }

                    if (otherTransMapping.IsStatic || true)
                    {
                        var t = new AnimationFile.AnimationBoneMapping(10000 + translatonCounter++);
                        outputAnimationFile.TranslationMappings.Add(t);
                        outputAnimationFile.StaticFrame.Position.Add(frameTransform.Position);
                    }
                    else
                    {
                        outputAnimationFile.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(-1));
                    }

                  
                }
                else
                {
                    outputAnimationFile.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(-1));
                    outputAnimationFile.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(-1));
                }

                //var frameTransform = GetFrameTransform(settings, boneIndex, -1, sourceSampler, otherSampler);
                //if (outputAnimationFile.RotationMappings[boneIndex].IsStatic)
                //    outputAnimationFile.StaticFrame.Rotation.Add(frameTransform.Rotation);
                //if (outputAnimationFile.TranslationMappings[boneIndex].IsStatic)
                //    outputAnimationFile.StaticFrame.Position.Add(frameTransform.Position);
            }

            return outputAnimationFile;
        }

        static public AnimationClip UpdateAnimation(AnimationBuilderSettings settings)
        {
            var outputAnimationFile = CreateOutputAnimation(settings.SourceSkeleton, settings.OtherAnimationClip.DynamicFrames.Count());

            AnimationBuilderSampler sourceSampler = new AnimationBuilderSampler(settings.SourceSkeleton, settings.SourceAnimationClip);
            AnimationBuilderSampler otherSampler = new AnimationBuilderSampler(settings.OtherSkeletonFile, settings.OtherAnimationClip);

            for (int frameIndex = 0; frameIndex < outputAnimationFile.DynamicFrames.Count(); frameIndex++)
            {
                for (int boneIndex = 0; boneIndex < settings.SourceSkeleton.BoneCount; boneIndex++)
                {
                    var frameTransform = GetFrameTransform(settings, boneIndex, frameIndex, sourceSampler, otherSampler);
                    outputAnimationFile.DynamicFrames[frameIndex].Rotation.Add(frameTransform.Rotation);
                    outputAnimationFile.DynamicFrames[frameIndex].Position.Add(frameTransform.Position);
                }
            }

            return outputAnimationFile;
        }

        public static AnimationClip CreateMergedAnimation(AnimationBuilderSettings settings)
        {
            // Validate
            if (Validate(settings) == false)
                return null;

            if (settings.OtherAnimationClip.IsPoseClip())
            {
                var outputAnimationFile = UpdateStaticPose(settings);

                for (int boneIndex = 0; boneIndex < settings.SourceSkeleton.BoneCount; boneIndex++)
                {
                    var mappedBone = GetMappedBone(settings.BoneSettings, boneIndex);
                    if (HasValidMapping(mappedBone) && mappedBone.Settings is AttachmentPointAdvBoneMappingBoneSettings attachmentSettings)
                    {
                        var frameTransform = AttachmentPoinstShit(settings, outputAnimationFile, 0, mappedBone, attachmentSettings);

                        //if (outputAnimationFile.RotationMappings[boneIndex].IsStatic)
                        {
                            var idx = outputAnimationFile.RotationMappings[boneIndex].Id;
                            outputAnimationFile.StaticFrame.Rotation[boneIndex] = frameTransform.Rotation;
                        }

                        //if (outputAnimationFile.TranslationMappings[boneIndex].IsStatic)
                        {
                            var idx = outputAnimationFile.TranslationMappings[boneIndex].Id;
                            outputAnimationFile.StaticFrame.Position[boneIndex] = frameTransform.Position;
                        }
                        
                    }
                }

                return outputAnimationFile;
            }
            else
            {
                var outputAnimationFile = UpdateAnimation(settings);

                //AnimationBuilderSampler newAnimationSampler = new AnimationBuilderSampler(sourceSkeleton, outputAnimationFile);
                for (int frameIndex = 0; frameIndex < outputAnimationFile.DynamicFrames.Count(); frameIndex++)
                {
                    for (int boneIndex = 0; boneIndex < settings.SourceSkeleton.BoneCount; boneIndex++)
                    {
                        var mappedBone = GetMappedBone(settings.BoneSettings, boneIndex);
                        if (HasValidMapping(mappedBone) && mappedBone.Settings is AttachmentPointAdvBoneMappingBoneSettings attachmentSettings)
                        {
                            
                            var tran = AttachmentPoinstShit(settings, outputAnimationFile, frameIndex, mappedBone, attachmentSettings);

                            outputAnimationFile.DynamicFrames[frameIndex].Rotation[mappedBone.BoneIndex] = tran.Rotation;
                            outputAnimationFile.DynamicFrames[frameIndex].Position[mappedBone.BoneIndex] = tran.Position;
                            
                        }
                    }
                }

                return outputAnimationFile;
            }
        }

        private static AnimationFrameTransform AttachmentPoinstShit(AnimationBuilderSettings settings, AnimationClip outputAnimationFile, int frameIndex, AdvBoneMappingBone mappedBone, AttachmentPointAdvBoneMappingBoneSettings attachmentSettings)
        {
            var newAnimationClipFrame = AnimationSampler.Sample(frameIndex, 0, settings.SourceSkeleton, outputAnimationFile);
            var transformDiff = newAnimationClipFrame.GetSkeletonAnimatedWorldDiff(settings.SourceSkeleton, 0, mappedBone.Settings.MappingBoneId);
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

            return new AnimationFrameTransform(position + positionOffsetLocalSpace, Quaternion.Normalize(rotation));
        }

        static AnimationFrameTransform GetFrameTransform(AnimationBuilderSettings settings, int boneIndex, int frameIndex, AnimationBuilderSampler sourceSampler, AnimationBuilderSampler otherSampler)
        {
            AnimationFrameTransform finalTransfrom = new AnimationFrameTransform(Vector3.Zero, Quaternion.Identity);
            var mappedBone = GetMappedBone(settings.BoneSettings, boneIndex);

            if (HasValidMapping(mappedBone))
            {
                var mappedBondeIndex = mappedBone.Settings.MappingBoneId;
                switch (mappedBone.MappingType)
                {
                    case BoneMappingType.AttachmentPoint:
                        break;  // Handled later
                    case BoneMappingType.Direct:
                        finalTransfrom = ProcessDirectBone(mappedBone.Settings as DirectAdvBoneMappingBoneSettings, sourceSampler, otherSampler, frameIndex, boneIndex, mappedBondeIndex);
                        break;
                    case BoneMappingType.Direct_smart:
                        finalTransfrom = ProcessDirectSmartBone(mappedBone.Settings as DirectSmartAdvBoneMappingBoneSettings, sourceSampler, otherSampler, frameIndex, boneIndex, mappedBondeIndex);
                        break;
                    default:
                        throw new Exception($"Unsupported MappingType - { mappedBone.MappingType }");
                }
            }
            else
            {
                finalTransfrom = sourceSampler.GetAnimationTransform(boneIndex, frameIndex);
            }

            var rotation = finalTransfrom.Rotation;
            var positionOffset = Vector3.Zero;
            ComputeMappedBoneAttributeContributions(mappedBone, ref rotation, ref positionOffset);
            var positionOffsetLocalSpace = Vector3.Transform(positionOffset, rotation);

            return new AnimationFrameTransform(positionOffsetLocalSpace + finalTransfrom.Position, Quaternion.Normalize(rotation));
        }

        static AnimationFrameTransform ProcessDirectBone(DirectAdvBoneMappingBoneSettings directSettings, AnimationBuilderSampler sourceSampler, AnimationBuilderSampler otherSampler, int frameIndex, int boneIndex, int mappedBoneIndex)
        {
            var otherAnimatedTransform = otherSampler.GetAnimationTransform(mappedBoneIndex, frameIndex);

            var scale = 1.0f;
            if (directSettings.ScaleSkeletonBasedOnBoneLength)
                scale = GetBoneRatio(sourceSampler, boneIndex, otherSampler, mappedBoneIndex);

            return new AnimationFrameTransform(otherAnimatedTransform.Position * scale, otherAnimatedTransform.Rotation);
        }

        static AnimationFrameTransform ProcessDirectSmartBone(DirectSmartAdvBoneMappingBoneSettings directSmartSettings, AnimationBuilderSampler sourceSampler, AnimationBuilderSampler otherSampler, int frameIndex, int boneIndex, int mappedBoneIndex)
        {
            var boneRatio = GetBoneRatio(sourceSampler, boneIndex, otherSampler, mappedBoneIndex);

            var sourceSkeletonTransform = sourceSampler.GetSkeletonTransform(boneIndex);
            var otherskeletonTransform = otherSampler.GetSkeletonTransform(mappedBoneIndex);
            var otherAnimatedTransform = otherSampler.GetAnimationTransform(mappedBoneIndex, frameIndex);

            if (directSmartSettings.BoneCopyMethod == BoneCopyMethod.Ratio)
            {
                if (directSmartSettings.Ratio_ScaleRotation)
                {
                    otherAnimatedTransform.Rotation.ToAxisAngle(out Vector3 axis, out float angle);
                    otherAnimatedTransform.Rotation = Quaternion.CreateFromAxisAngle(axis, angle * boneRatio);
                }

                var skeletonRotationDifference = Quaternion.Identity;
                if (directSmartSettings.Ratio_ScaleMethod == RatioScaleMethod.Larger)
                    skeletonRotationDifference = otherskeletonTransform.Rotation * Quaternion.Inverse(sourceSkeletonTransform.Rotation);
                else
                    skeletonRotationDifference = sourceSkeletonTransform.Rotation * Quaternion.Inverse(otherskeletonTransform.Rotation);

                return new AnimationFrameTransform((otherAnimatedTransform.Position * boneRatio), (otherAnimatedTransform.Rotation * skeletonRotationDifference));
            }
            else if (directSmartSettings.BoneCopyMethod == BoneCopyMethod.Relative)
            {
                var relativeRotation = otherAnimatedTransform.Rotation * Quaternion.Inverse(otherskeletonTransform.Rotation);
                var relativePosition = otherAnimatedTransform.Position - otherskeletonTransform.Position;

                return new AnimationFrameTransform(relativePosition, relativeRotation);
            }

            throw new Exception($"Unsupported BoneCopyMethod - { directSmartSettings.BoneCopyMethod }");
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


        static float GetBoneRatio(AnimationBuilderSampler sourceSampler, int sourceBoneIndex, AnimationBuilderSampler otherSampler, int otherBoneIndex)
        {
            float boneRatio = sourceSampler.GetBoneLength(sourceBoneIndex) / otherSampler.GetBoneLength(otherBoneIndex);
            if (float.IsNaN(boneRatio))
                boneRatio = 1;

            return boneRatio;
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


        static bool HasValidMapping(AdvBoneMappingBone bone)
        {
            return bone.Settings.HasMapping && bone.Settings.UseMapping;
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
    }


}