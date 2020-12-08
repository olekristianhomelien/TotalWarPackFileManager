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
using static Filetypes.RigidModel.AnimationFile;

namespace VariantMeshEditor.Services
{
    public class AnimationBuilderService
    {

        public class AnimationBuilderSettings
        {

            public PackedFile SourceAnimationFile { get; set; }   // ExternalSkeleton.SelectedItem
            public Viewer.Animation.Skeleton SourceSkeleton { get; set; }   //_targetSkeletonNode.Skeleton

            public PackedFile ExternalSkeletonFile { get; set; }   // ExternalSkeleton.SelectedItem

            public PackedFile ExternalAnimationFile { get; set; }   // ExternalAnimation.SelectedItem

            public IEnumerable<MappableSkeletonBone> TargetSkeletonBones { get; set; }

        }

        bool Validate(AnimationBuilderSettings settings)
        {
            if (settings.SourceAnimationFile == null)
                return false;

            if (settings.SourceSkeleton == null)
                return false;

            if (settings.ExternalSkeletonFile == null)
                return false;

            if (settings.ExternalAnimationFile == null)
                return false;

            if (settings.TargetSkeletonBones == null || settings.TargetSkeletonBones.Count() == 0)
                return false;
            return true;
        }

        AnimationClip CreateAnimation(PackedFile file)
        {
            var anim = AnimationFile.Create(new ByteChunk(file.Data));
            var animClip = new AnimationClip(anim);
            return animClip;
        }

        Viewer.Animation.Skeleton CreateSkeleton(PackedFile file)
        {
            var skeletonFile = AnimationFile.Create(new ByteChunk(file.Data));
            Viewer.Animation.Skeleton skeleton = new Viewer.Animation.Skeleton(skeletonFile);
            return skeleton;
        }


        public AnimationClip CreateMergedAnimation(AnimationBuilderSettings settings)
        {
            // Validate
            if (Validate(settings) == false)
                return null;

            var currentsSkeltonBoneCount = settings.SourceSkeleton.BoneCount;
            Viewer.Animation.Skeleton externalSkeleton = CreateSkeleton(settings.ExternalSkeletonFile);
            var externalAnimationClip = CreateAnimation(settings.ExternalAnimationFile);


            var sourceAnimationClip = CreateAnimation(settings.SourceAnimationFile);

            var outputAnimationFile = new AnimationClip();
            for (int i = 0; i < currentsSkeltonBoneCount; i++)
            {
                outputAnimationFile.RotationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
                outputAnimationFile.TranslationMappings.Add(new AnimationFile.AnimationBoneMapping(i));
            }

            for (int frameIndex = 0; frameIndex < externalAnimationClip.DynamicFrames.Count(); frameIndex++)
            {
                var currentOutputFrame = new AnimationClip.KeyFrame();
                outputAnimationFile.DynamicFrames.Add(currentOutputFrame);

                for (int boneIndex = 0; boneIndex < currentsSkeltonBoneCount; boneIndex++)
                {
                    var rotation = Quaternion.Identity;
                    var position = Vector3.Zero;
                    var boneToGetAnimDataFrom = GetMappedBone(settings.TargetSkeletonBones, boneIndex);

                    if (HasValidMapping(boneToGetAnimDataFrom))
                    {
                        ComputeAnimationContribution(frameIndex, externalSkeleton, externalAnimationClip, boneToGetAnimDataFrom.MappedBone.BoneIndex, ref rotation, ref position);
                        ComputeMappedBoneAttributeContributions(boneToGetAnimDataFrom, ref rotation, ref position);
                    }
                    else
                    {
                        ComputeAnimationContribution(frameIndex, settings.SourceSkeleton, sourceAnimationClip, boneIndex, ref rotation, ref position);
                    }

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
            ProcessFrame(animation.StaticFrame, boneIndex, animation.RotationMappings, animation.TranslationMappings, AnimationBoneMappingType.Static, ref out_rotation, ref out_position);

            // Dynamic
            ProcessFrame(animation.DynamicFrames[safeFameIndex], boneIndex, animation.RotationMappings, animation.TranslationMappings, AnimationBoneMappingType.Dynamic, ref out_rotation, ref out_position);
        }

        void ComputeMappedBoneAttributeContributions(MappableSkeletonBone bone, ref Quaternion out_rotation, ref Vector3 out_position)
        {
            if (bone.UseConstantOffset)
            {
                out_position += MathConverter.ToVector(bone.ContantTranslationOffset);
                out_rotation = out_rotation * MathConverter.ToQuaternion(bone.ContantRotationOffset);
            }
        }

        void ProcessFrame(AnimationClip.KeyFrame frame, int boneIndex, List<AnimationBoneMapping> rotationMapping, List<AnimationBoneMapping> translationMapping, AnimationBoneMappingType mappingType, 
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