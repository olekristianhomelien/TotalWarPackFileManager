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
    public enum FrameTypes
    {
        Both,
        Static,
        Dynamic,
    }

    public enum TransformTypes
    {
        Both,
        Rotation,
        Translation,
    }

    public enum MatchMethod
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
            var currentsSkeltonBoneCount = sourceSkeleton.BoneCount;
            var otherSkeleton = CreateSkeleton(settings.OtherSkeletonFile);
            var otherAnimationClip = CreateAnimation(settings.OtherAnimationFile);


            int frameCount = GetAnimationFrameCount(sourceAnimationClip, otherAnimationClip, settings.SelectedMainAnimation);

            var outputAnimationFile = new AnimationClip();
            for (int i = 0; i < currentsSkeltonBoneCount; i++)
            {
                outputAnimationFile.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
                outputAnimationFile.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
            }

            for (int frameIndex = 0; frameIndex < otherAnimationClip.DynamicFrames.Count(); frameIndex++)
            {
                var currentOutputFrame = new AnimationClip.KeyFrame();
                outputAnimationFile.DynamicFrames.Add(currentOutputFrame);

                for (int boneIndex = 0; boneIndex < currentsSkeltonBoneCount; boneIndex++)
                {
                    var rotation = Quaternion.Identity;
                    var position = Vector3.Zero;
                    var boneToGetAnimDataFrom = GetMappedBone(settings.BoneSettings, boneIndex);

                    if (HasValidMapping(boneToGetAnimDataFrom))
                    {
                        var mappedBondeIndex = boneToGetAnimDataFrom.MappedBone.BoneIndex;
                        ComputeSkeletonContribution(otherSkeleton, mappedBondeIndex, ref rotation, ref position);
                        ComputeAnimationContribution(frameIndex, otherSkeleton, otherAnimationClip, mappedBondeIndex, ref rotation, ref position);
                    }
                    else
                    {
                        ComputeSkeletonContribution(sourceSkeleton, boneIndex, ref rotation, ref position);

                        // Source animation is not mandatary
                        if (sourceAnimationClip != null)
                            ComputeAnimationContribution(frameIndex, sourceSkeleton, sourceAnimationClip, boneIndex, ref rotation, ref position);
                    }

                    ComputeMappedBoneAttributeContributions(boneToGetAnimDataFrom, ref rotation, ref position);

                    rotation.Normalize();
                    outputAnimationFile.DynamicFrames[frameIndex].Rotation.Add(rotation);
                    outputAnimationFile.DynamicFrames[frameIndex].Translation.Add(position);
                }
            }

            return outputAnimationFile;
        }

        void ComputeAnimationContribution(int frameIndex, Skeleton skeleton, AnimationClip animation, int boneIndex, ref Quaternion out_rotation, ref Vector3 out_position)
        {
            var safeFameIndex = frameIndex % animation.DynamicFrames.Count();

            // Copy the skeleton transform as default
            out_rotation = skeleton.Rotation[boneIndex];
            out_position = skeleton.Translation[boneIndex];

            // Static
            ProcessFrame(animation.StaticFrame, boneIndex, animation.RotationMappings, animation.TranslationMappings, 
                AnimationFile.AnimationBoneMappingType.Static, ref out_rotation, ref out_position);

            // Dynamic
            ProcessFrame(animation.DynamicFrames[safeFameIndex], boneIndex, animation.RotationMappings, animation.TranslationMappings,
                AnimationFile.AnimationBoneMappingType.Dynamic, ref out_rotation, ref out_position);
        }

        void ComputeSkeletonContribution(Skeleton skeleton, int boneIndex, ref Quaternion out_rotation, ref Vector3 out_position)
        {
            // Copy the skeleton transform as default
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
                        out_position = frame.Translation[otherRemappedId];
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

    }
}