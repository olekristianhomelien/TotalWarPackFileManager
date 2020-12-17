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
        Absolute,
    }

    // Whole animation configurations
    public enum TimeMatchMethod
    {
        TimeFit,
        HoldLastFrame
    }

    public enum MergeMethod
    {
        Replace
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

            public PackedFile SourceAnimationFile { get; set; }   // ExternalSkeleton.SelectedItem
            public Skeleton SourceSkeleton { get; set; }   //_targetSkeletonNode.Skeleton

            public PackedFile OtherSkeletonFile { get; set; }   // ExternalSkeleton.SelectedItem

            public PackedFile OtherAnimationFile { get; set; }   // ExternalAnimation.SelectedItem

            public IEnumerable<MappableSkeletonBone> BoneSettings { get; set; }

            public MainAnimation SelectedMainAnimation { get; set; }

        }

        bool Validate(AnimationBuilderSettings settings)
        {
            if (settings.SourceSkeleton == null)
                return false;

            if (settings.OtherSkeletonFile == null)
                return false;

            if (settings.OtherAnimationFile == null)
                return false;

            if (settings.BoneSettings == null || settings.BoneSettings.Count() == 0)
                return false;
            return true;
        }

        AnimationClip CreateAnimation(PackedFile file)
        {
            if (file == null)
                return null;
            var anim = AnimationFile.Create(new ByteChunk(file.Data));
            var animClip = new AnimationClip(anim);
            return animClip;
        }

        Skeleton CreateSkeleton(PackedFile file)
        {
            var skeletonFile = AnimationFile.Create(new ByteChunk(file.Data));
            var skeleton = new Skeleton(skeletonFile);
            return skeleton;
        }



        public AnimationClip CreateMergedAnimation(AnimationBuilderSettings settings)
        {
            // Validate
            if (Validate(settings) == false)
                return null;

            var sourceAnimationClip = CreateAnimation(settings.SourceAnimationFile);
            var sourceSkeleton = settings.SourceSkeleton;
            var otherSkeleton = CreateSkeleton(settings.OtherSkeletonFile);
            var otherAnimationClip = CreateAnimation(settings.OtherAnimationFile);
            var outputAnimationFile = CreateOutputAnimation(sourceSkeleton, otherAnimationClip);

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
                        var mappedBondeIndex = boneToGetAnimDataFrom.MappedBone.BoneIndex;
                        var otherBoneLength = GetBoneLength(otherSkeleton, mappedBondeIndex);

                        GetSkeletonTransform(sourceSkeleton, boneIndex, out Quaternion sourceSkeletonRotation, out Vector3 sourceSkeletonPosition);
                        GetSkeletonTransform(otherSkeleton, mappedBondeIndex, out Quaternion otherSkeletonRotation, out Vector3 otherSkeletonPosition);

                        var newPosition = Vector3.Zero;
                        var newRotation = Quaternion.Identity;

                        if (HasAnimationData(mappedBondeIndex, otherAnimationClip))
                        {
                            GetAnimationTransform(otherAnimationClip, frameIndex, otherSkeleton, mappedBondeIndex, out Quaternion otherAnimationRotation, out Vector3 otherAnimationPosition);

                            newPosition = otherAnimationPosition;
                            newRotation = otherAnimationRotation;
                        }
                        else
                        {
                            newPosition = otherSkeletonPosition;
                            newRotation = otherSkeletonRotation;
                        }

                        if (boneToGetAnimDataFrom.BoneCopyMethod == BoneCopyMethod.Ratio)
                        {
                            var ratio = sourceBoneLength / otherBoneLength;
                            if (float.IsNaN(ratio))
                                ratio = 1;

                            var skeletonRotationDifference = sourceSkeletonRotation * Quaternion.Inverse(otherSkeletonRotation);

                            position = newPosition * ratio;
                            rotation = newRotation * skeletonRotationDifference;
                        }
                        else if (boneToGetAnimDataFrom.BoneCopyMethod == BoneCopyMethod.Absolute)
                        {
                            position = newPosition;
                            rotation = newRotation;
                        }
                        else if (boneToGetAnimDataFrom.BoneCopyMethod == BoneCopyMethod.Relative)
                        {
                            var relativeRotation = newRotation * Quaternion.Inverse(otherSkeletonRotation);
                            var relativePosition = newPosition - otherSkeletonPosition;
                            rotation = (relativeRotation * sourceSkeletonRotation);
                            position = relativePosition + sourceSkeletonPosition;
                        }
                        else
                        {
                            throw new Exception($"Unsupported BoneCopyMethod - { boneToGetAnimDataFrom.BoneCopyMethod }");
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

           //for (int boneIndex = 0; boneIndex < currentsSkeltonBoneCount; boneIndex++)
           //{
           //    for (int frameIndex = 0; frameIndex < outputAnimationFile.DynamicFrames.Count() -1; frameIndex++)
           //    {
           //       
           //
           //        float ratio = 1;
           //       var boneToGetAnimDataFrom = GetMappedBone(settings.BoneSettings, boneIndex);
           //       if (HasValidMapping(boneToGetAnimDataFrom))
           //       {
           //           if (boneToGetAnimDataFrom.OriginalBone.BoneName.Contains("lowerleg_left"))
           //           { 
           //           }
           //       
           //           var mappedBondeIndex = boneToGetAnimDataFrom.MappedBone.BoneIndex;
           //       
           //           var boneLength = GetBoneLength(sourceSkeleton, boneIndex);
           //           var otherBoneLength = GetBoneLength(otherSkeleton, mappedBondeIndex);
           //       
           //           ratio =  boneLength / otherBoneLength;
           //           if (float.IsNaN(ratio))
           //               ratio = 1;
           //       
           //           if (boneToGetAnimDataFrom.UseConstantOffset)
           //               ratio = 1;
           //           else
           //           {
           //                ratio =  ratio;// 1 / ratio;// 1 - ratio;// 0.5f;
           //           }
           //       }
           //
           //
           //        var current = outputAnimationFile.DynamicFrames[frameIndex].Rotation[boneIndex];
           //        var next = outputAnimationFile.DynamicFrames[frameIndex + 1].Rotation[boneIndex];
           //        outputAnimationFile.DynamicFrames[frameIndex + 1].Rotation[boneIndex] = Quaternion.Slerp(current, next, ratio);  
           //    }
           //}

            return outputAnimationFile;
        }

        AnimationClip CreateOutputAnimation(Skeleton sourceSkeleton, AnimationClip otherAnimationClip)
        {
            var outputAnimationFile = new AnimationClip();
            for (int i = 0; i < sourceSkeleton.BoneCount; i++)
            {
                outputAnimationFile.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
                outputAnimationFile.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
            }

            for (int frameIndex = 0; frameIndex < otherAnimationClip.DynamicFrames.Count(); frameIndex++)
                outputAnimationFile.DynamicFrames.Add(new AnimationClip.KeyFrame());

            return outputAnimationFile;
        }

        bool HasAnimationData(int boneIndex, AnimationClip animation)
        {
            if (animation == null)
                return false;

            var remappedId = animation.RotationMappings[boneIndex].Id;
            if (remappedId != -1)
            {
                return true;
            }
              
            remappedId = animation.TranslationMappings[boneIndex].Id;
            if (remappedId != -1)
            {
                return true;
            }

            return true;
        }

        void GetAnimationTransform(AnimationClip animation, int frameIndex, Skeleton skeleton, int boneIndex, out Quaternion out_rotation, out Vector3 out_position)
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

        void GetSkeletonTransform(Skeleton skeleton, int boneIndex, out Quaternion out_rotation, out Vector3 out_position)
        {
            out_rotation = skeleton.Rotation[boneIndex];
            out_position = skeleton.Translation[boneIndex];
        }

        void ComputeMappedBoneAttributeContributions(MappableSkeletonBone bone, ref Quaternion out_rotation, ref Vector3 out_position)
        {
            if (bone.UseConstantOffset)
            {
                out_position += MathConverter.ToVector(bone.ContantTranslationOffset);
                out_rotation = out_rotation * MathConverter.ToQuaternion(bone.ContantRotationOffset);
            }
        }

        void ProcessFrame(AnimationClip.KeyFrame frame, int boneIndex, 
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

        bool HasValidMapping(MappableSkeletonBone bone)
        {
            return bone.MappedBone != null && bone.UseMapping && bone.MappedBone.BoneIndex != -1;
        }

        int GetAnimationFrameCount(AnimationClip source, AnimationClip other, MainAnimation mainAnimationSetting)
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

        MappableSkeletonBone GetMappedBone(IEnumerable<MappableSkeletonBone> nodes, int boneIndex)
        {
            foreach (var node in nodes)
            {
                if (node.OriginalBone.BoneIndex == boneIndex)
                    return node;
                var res = GetMappedBone(node.Children, boneIndex);
                if (res != null)
                    return res;
            }
            return null;
        }

        float GetBoneLength(Skeleton skeleton, int boneId)
        {
            var parentBoneId = skeleton.ParentBoneId[boneId];
            if (parentBoneId == -1)
                return 0;

            var p0 = Vector3.Transform(Vector3.Zero, skeleton.WorldTransform[boneId]);
            var p1 = Vector3.Transform(Vector3.Zero, skeleton.WorldTransform[parentBoneId]);

            var dist = Vector3.Distance(p0, p1);

            return Math.Abs(dist);
        }

    }

   
}