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
        Relative,
        Absolute
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
                    // Transforms, can be relative?
                    var rotation = Quaternion.Identity;
                    var position = Vector3.Zero;

                    var boneToGetAnimDataFrom = GetMappedBone(settings.BoneSettings, boneIndex);

                    if (HasValidMapping(boneToGetAnimDataFrom))
                    {
                        var mappedBondeIndex = boneToGetAnimDataFrom.MappedBone.BoneIndex;
                        if (HasAnimationData(mappedBondeIndex, otherAnimationClip.RotationMappings, otherAnimationClip.TranslationMappings))
                        {
                           if (boneToGetAnimDataFrom.BoneCopyMethod == BoneCopyMethod.Absolute)
                           {
                               ComputeAnimationContribution(frameIndex, otherSkeleton, otherAnimationClip, mappedBondeIndex, ref rotation, ref position);
                               float scaleValue = 1; ;
                           
                               position.X *= scaleValue;
                               position.Y *= scaleValue;
                               //position.Z *= scaleValue;
                           }
                           else
                            {
                                var otherSkeletonRotation = Quaternion.Identity;
                                var otherSkeletonTranslation = Vector3.Zero;

                                ComputeSkeletonContribution(otherSkeleton, mappedBondeIndex, ref otherSkeletonRotation, ref otherSkeletonTranslation);

                                var otherAnimationRotation = Quaternion.Identity;
                                var otherAnimationTranslation = Vector3.Zero;
                                ComputeAnimationContribution(frameIndex, otherSkeleton, otherAnimationClip, mappedBondeIndex, ref otherAnimationRotation, ref otherAnimationTranslation);

                                var relativeRotation = otherAnimationRotation* Quaternion.Inverse(otherSkeletonRotation);
                                var relativeTranslation = otherAnimationTranslation - otherSkeletonTranslation;


                                var sourceSkeletonRotation = Quaternion.Identity;
                                var sourceSkeletonTranslation = Vector3.Zero;

                                ComputeSkeletonContribution(sourceSkeleton, boneIndex, ref sourceSkeletonRotation, ref sourceSkeletonTranslation);

                                ////if (boneToGetAnimDataFrom.BoneCopyMethod == BoneCopyMethod.Absolute)
                                ////{
                                ////    var temp = Quaternion.Multiply(relativeRotation, 0.1f);
                                ////    relativeRotation = Quaternion.Multiply(temp, relativeRotation);
                                ////}
                                //
                                //
                                //
                                //
                                //
                                //if (boneToGetAnimDataFrom.BoneCopyMethod == BoneCopyMethod.Absolute)
                                //{
                                //
                                //    relativeRotation = Quaternion.Slerp(relativeRotation, Quaternion.Identity, (float)boneToGetAnimDataFrom.ContantTranslationOffset.X.Value);
                                //
                                //    //Vector3 t = rotation.ToAxisAngleDegrees()* (float)boneToGetAnimDataFrom.ContantTranslationOffset.X.Value;
                                //    //rotation = t.FromAxisAngleDegrees();
                                //}

                                rotation = (relativeRotation * sourceSkeletonRotation);


                                position = relativeTranslation + sourceSkeletonTranslation;
                            }
                        }
                        else
                        {
                            // No animation, how to handle relative stuff?
                            ComputeSkeletonContribution(otherSkeleton, mappedBondeIndex, ref rotation, ref position);
                            
                        }
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


        bool HasAnimationData(int boneIndex,
            List<AnimationFile.AnimationBoneMapping> rotationMapping,
            List<AnimationFile.AnimationBoneMapping> translationMapping)
        {
            var remappedId = rotationMapping[boneIndex].Id;
            if (remappedId != -1)
            {
                return true;
            }
              
            remappedId = translationMapping[boneIndex].Id;
            if (remappedId != -1)
            {
                return true;
            }

            return true;
        }

        void ComputeAnimationContribution(int frameIndex, Skeleton skeleton, AnimationClip animation, int boneIndex, ref Quaternion out_rotation, ref Vector3 out_position)
        {
            var safeFameIndex = frameIndex % animation.DynamicFrames.Count();

            // Copy the skeleton transform as default
            out_rotation = skeleton.Rotation[boneIndex];
            out_position = skeleton.Translation[boneIndex];

            // Static
            ProcessFrame(animation.StaticFrame, boneIndex, animation.RotationMappings, animation.TranslationMappings, 
                AnimationFile.AnimationBoneMappingType.Static, skeleton, ref out_rotation, ref out_position);

            // Dynamic
            ProcessFrame(animation.DynamicFrames[safeFameIndex], boneIndex, animation.RotationMappings, animation.TranslationMappings,
                AnimationFile.AnimationBoneMappingType.Dynamic, skeleton, ref out_rotation, ref out_position);
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
            Skeleton skeleton,
            ref Quaternion out_rotation, ref Vector3 out_position)
        {
            if (frame != null)
            {
                if (rotationMapping[boneIndex].MappingType == mappingType)
                {
                    var otherRemappedId = rotationMapping[boneIndex].Id;
                    if (otherRemappedId != -1)
                    {
                        out_rotation = frame.Rotation[otherRemappedId];
                    }
                }

                if (translationMapping[boneIndex].MappingType == mappingType)
                {
                    var otherRemappedId = translationMapping[boneIndex].Id;
                    if (otherRemappedId != -1)
                    {
                        out_position = frame.Translation[otherRemappedId];
                    }
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

    public static class VetorHelper
    {
        /// <summary>
        /// Converts from degrees to radians. A present from the PlazSoft team.
        /// </summary>
        public static float ToRadians(this float angle)
        {
            return MathHelper.ToRadians(angle);
        }
        /// <summary>
        /// Converts from radians to degrees.
        /// </summary>
        public static float ToDegrees(this float angle)
        {
            return MathHelper.ToDegrees(angle);
        }

        /// <summary>
        /// Normalizes the vector and returns its length. A present from the PlazSoft team.
        /// If the vector is zero, the vector itself is returned.
        /// </summary>
        /// <param name="v">Vector to normalize</param>
        /// <param name="length">Length of vector before normalization</param>
        /// <returns>The normalized vector or (0,0,0)</returns>
        public static Vector3 NormalizeSafelyAndLength(this Vector3 v, out float length)
        {
            if (v.X == 0 && v.Y == 0 && v.Z == 0)
            {
                length = 0;
                return v;
            }
            length = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if (length != 0)
            {
                float isqrt = 1.0f / (float)Math.Sqrt(length);
                length *= isqrt;
                v.X *= isqrt;
                v.Y *= isqrt;
                v.Z *= isqrt;
            }
            return v;
        }

        /// <summary>
        /// Converts a quaternion into a human readable format. A present from the PlazSoft team.
        /// This will not correctly convert quaternions with length
        /// above 1.
        /// 
        /// Reversable with FromAxisAngleDegrees(Vector3)
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3 ToAxisAngleDegrees(this Quaternion q)
        {
            Vector3 axis;
            float angle;
            q.ToAxisAngle(out axis, out angle);
            return axis * angle.ToDegrees();
        }

        /// <summary>
        /// Converts from human readable axis angle degree representation
        /// to a quaternion. A present from the PlazSoft team.
        /// 
        /// Reversable with ToAxisAngleDegrees(this Quaternion)
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        public static Quaternion FromAxisAngleDegrees(this Vector3 deg)
        {
            float length;
            deg = deg.NormalizeSafelyAndLength(out length);
            return Quaternion.CreateFromAxisAngle(deg, length.ToRadians());
        }

        /// <summary>
        /// Converts a quaternion to axis/angle representation. A present from the PlazSoft team.
        /// 
        /// Reversable with Quaternion.CreateFromAxisAngle(Vector3, float)
        /// </summary>
        /// <param name="q"></param>
        /// <param name="axis">The axis that is rotated around, or (0,0,0)</param>
        /// <param name="angle">Angle around axis, in radians</param>
        public static void ToAxisAngle(this Quaternion q, out Vector3 axis, out float angle)
        {
            //From
            //http://social.msdn.microsoft.com/Forums/en-US/c482c19a-c566-4a64-aa9c-7a79ba7564d6/the-reverse-of-quaternioncreatefromaxisangle?forum=xnaframework
            //Modified to return 0,0,0 when it would have returned NaN
            //due to divide by zero.
            angle = (float)Math.Acos(q.W);
            float sa = (float)Math.Sin(angle);
            float ooScale = 0f;
            if (sa != 0)
                ooScale = 1.0f / sa;
            angle *= 2.0f;

            axis = new Vector3(q.X, q.Y, q.Z) * ooScale;
        }

    }
}