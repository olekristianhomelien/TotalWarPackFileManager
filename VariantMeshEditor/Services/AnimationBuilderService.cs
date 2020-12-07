using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
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


        public AnimationClip CreateMergedAnimation(AnimationBuilderSettings  settings)
        {
            throw new NotImplementedException();
           //return null;
           //// Validate
           //if (Validate(settings) == false)
           //    return null;
           //
           //var currentsSkeltonBoneCount = settings.SourceSkeleton.BoneCount;
           //Viewer.Animation.Skeleton externalSkeleton = CreateSkeleton(settings.ExternalSkeletonFile);
           //var externalAnimationClip = CreateAnimation(settings.ExternalAnimationFile);
           //
           //
           //var sourceAnimationClip = CreateAnimation(settings.SourceAnimationFile);
           //
           //var outputAnimationFile = new AnimationClip();
           //for (int i = 0; i < currentsSkeltonBoneCount; i++)
           //{
           //    outputAnimationFile.DynamicRotationMappingID.Add(i);
           //    outputAnimationFile.DynamicTranslationMappingID.Add(i);
           //}
           //
           //for (int frameIndex = 0; frameIndex < externalAnimationClip.DynamicFrames.Count(); frameIndex++)
           //{
           //    var currentOutputFrame = new AnimationClip.KeyFrame();
           //
           //    for (int boneIndex = 0; boneIndex < currentsSkeltonBoneCount; boneIndex++)
           //    {
           //        var boneToGetAnimDataFrom = GetMappedBone(settings.TargetSkeletonBones, boneIndex);
           //
           //        var skeletonPosRotation = settings.SourceSkeleton.Rotation[boneIndex];
           //        var skeletonPosTranslation = settings.SourceSkeleton.Translation[boneIndex];
           //
           //        if (boneToGetAnimDataFrom.MappedBone != null && boneToGetAnimDataFrom.UseMapping && boneToGetAnimDataFrom.MappedBone.BoneIndex != -1)
           //        {
           //            if (true)   // Animation skeleton scale?
           //            {
           //                skeletonPosRotation = externalSkeleton.Rotation[boneToGetAnimDataFrom.MappedBone.BoneIndex];
           //                skeletonPosTranslation = externalSkeleton.Translation[boneToGetAnimDataFrom.MappedBone.BoneIndex];
           //            }
           //
           //            var boneRotationIndexInExternal = externalAnimationClip.DynamicRotationMappingID.IndexOf(boneToGetAnimDataFrom.MappedBone.BoneIndex);
           //            bool hasBoneRotationInExternal = boneRotationIndexInExternal != -1;
           //            if (hasBoneRotationInExternal)
           //            {
           //                skeletonPosRotation = externalAnimationClip.DynamicFrames[frameIndex].Rotation[boneRotationIndexInExternal];
           //            }
           //
           //            var boneTranslationIndexInExternal = externalAnimationClip.DynamicTranslationMappingID.IndexOf(boneToGetAnimDataFrom.MappedBone.BoneIndex);
           //            bool hasBoneTranslationInExternal = boneTranslationIndexInExternal != -1;
           //            if (hasBoneTranslationInExternal)
           //            {
           //                skeletonPosTranslation = externalAnimationClip.DynamicFrames[frameIndex].Translation[boneTranslationIndexInExternal];
           //            }
           //        }
           //        else
           //        {
           //
           //            var localAnimFrameIndex = frameIndex;
           //            if (localAnimFrameIndex > sourceAnimationClip.DynamicFrames.Count() - 1)
           //                localAnimFrameIndex = sourceAnimationClip.DynamicFrames.Count() - 1;
           //
           //            var boneRotationIndexInExternal = sourceAnimationClip.DynamicRotationMappingID.IndexOf(boneIndex);
           //            bool hasBoneRotationInExternal = boneRotationIndexInExternal != -1;
           //            if (hasBoneRotationInExternal)
           //            {
           //                skeletonPosRotation = sourceAnimationClip.DynamicFrames[localAnimFrameIndex].Rotation[boneRotationIndexInExternal];
           //            }
           //
           //            var boneTranslationIndexInExternal = sourceAnimationClip.DynamicTranslationMappingID.IndexOf(boneIndex);
           //            bool hasBoneTranslationInExternal = boneTranslationIndexInExternal != -1;
           //            if (hasBoneTranslationInExternal)
           //            {
           //                skeletonPosTranslation = sourceAnimationClip.DynamicFrames[localAnimFrameIndex].Translation[boneTranslationIndexInExternal];
           //            }
           //        }
           //
           //        if (boneToGetAnimDataFrom.UseConstantOffset)
           //        {
           //            skeletonPosTranslation += MathConverter.ToVector(boneToGetAnimDataFrom.ContantTranslationOffset);
           //            var rot = MathConverter.ToQuaternion(boneToGetAnimDataFrom.ContantRotationOffset);
           //            skeletonPosRotation *= rot;
           //            skeletonPosRotation.Normalize();
           //        }
           //
           //        currentOutputFrame.Translation.Add(skeletonPosTranslation);
           //        currentOutputFrame.Rotation.Add(skeletonPosRotation);
           //    }
           //    outputAnimationFile.DynamicFrames.Add(currentOutputFrame);
           //}
           //
           //return outputAnimationFile;
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
