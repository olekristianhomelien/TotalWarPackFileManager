using Common;
using Microsoft.Xna.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Filetypes.RigidModel.AnimationFile;

namespace Viewer.Animation
{
    class AnimationSampler
    {
        public static AnimationFrame Sample(float t, Skeleton skeleton, List<AnimationClip> animationClips, bool applyStaticFrame, bool applyDynamicFrames)
        {
            try
            {
                if (skeleton == null)
                    return null;

                // Make sure it its in the 0-1 range
                t = EnsureRange(t, 0, 1);

                var currentFrame = new AnimationFrame();
                for (int i = 0; i < skeleton.BoneCount; i++)
                {
                    currentFrame.BoneTransforms.Add(new AnimationFrame.BoneKeyFrame()
                    {
                        Transform = skeleton.Transform[i],
                        Translation = skeleton.Translation[i],
                        Rotation = skeleton.Rotation[i],
                        BoneIndex = i,
                        ParentBoneIndex = skeleton.ParentBoneId[i],
                    });
                }

                if (animationClips != null)
                {
                    if (applyStaticFrame)
                    {
                        foreach (var animation in animationClips)
                        {
                            if (animation.UseStaticFrame)
                                ApplyAnimation(animation.StaticFrame, null, 0, currentFrame, animation.RotationMappings, animation.TranslationMappings, AnimationBoneMappingType.Static);
                        }
                    }

                    if (applyDynamicFrames)
                    {
                        int maxFrames = animationClips[0].DynamicFrames.Count() - 1;
                        float frame = maxFrames * t;

                        int frameIndex = (int)(frame);
                        float frameIterpolation = frame - frameIndex;

                        if (animationClips.Any() && animationClips[0].UseDynamicFames)
                        {
                            if (animationClips[0].DynamicFrames.Count > frameIndex)
                            {
                                var currentFrameKeys = GetKeyFrameFromIndex(animationClips[0].DynamicFrames, frameIndex);
                                var nextFrameKeys = GetKeyFrameFromIndex(animationClips[0].DynamicFrames, frameIndex + 1);
                                ApplyAnimation(currentFrameKeys, nextFrameKeys, (float)frameIterpolation, currentFrame, animationClips[0].RotationMappings, animationClips[0].TranslationMappings, AnimationBoneMappingType.Dynamic);
                            }
                        }
                    }
                }
                //for (int i = 0; i < currentFrame.BoneTransforms.Count() - 1; i++)
                //{
                //    Quaternion rotation = Quaternion.Slerp(currentFrame.BoneTransforms[i].Rotation, currentFrame.BoneTransforms[i].Rotation, 0.5f);
                //
                //    currentFrame.BoneTransform[i + 1].rotation = rotation;
                //}

                for (int i = 0; i < currentFrame.BoneTransforms.Count(); i++)
                {
                    Quaternion rotation = currentFrame.BoneTransforms[i].Rotation;
                    Vector3 translation = currentFrame.BoneTransforms[i].Translation;
                    //  translation.X = -translation.X;
                    currentFrame.BoneTransforms[i].Transform = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation);

                    var parentindex = currentFrame.BoneTransforms[i].ParentBoneIndex;
                    if (parentindex == -1)
                    {
                        var scale = Matrix.CreateScale(-1, 1, 1);
                        currentFrame.BoneTransforms[i].Transform = (scale * currentFrame.BoneTransforms[i].Transform);
                        continue;
                    }

                    currentFrame.BoneTransforms[i].Transform = currentFrame.BoneTransforms[i].Transform * currentFrame.BoneTransforms[parentindex].Transform;
                }

                // Mult with inverse bind matrix, in worldspace
                for (int i = 0; i < skeleton.BoneCount; i++)
                {
                    var inv = Matrix.Invert(skeleton.WorldTransform[i]);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }
                return currentFrame;
            }
            catch (Exception e)
            {
                ILogger logger = Logging.Create<AnimationSampler>();
                logger.Error(e.Message);
                throw;
            }
        }

        static float EnsureRange(float value, float min, float max)
        {
            if (value > max)
                return max;
            else if (value < min)
                return min;
            return value;
        }

        static AnimationClip.KeyFrame GetKeyFrameFromIndex(List<AnimationClip.KeyFrame> keyframes, int frameIndex)
        {
            int count = keyframes.Count();
            if (frameIndex >= count)
                return null;

            return keyframes[frameIndex];
        }

        static Quaternion ComputeRotationsCurrentFrame(int boneIndex, AnimationClip.KeyFrame currentFrame, AnimationClip.KeyFrame nextFrame, float animationInterpolation)
        {
            var animationValueCurrentFrame = currentFrame.Rotation[boneIndex];
            if (nextFrame != null)
            {
                var animationValueNextFrame = nextFrame.Rotation[boneIndex];
                animationValueCurrentFrame = Quaternion.Slerp(animationValueCurrentFrame, animationValueNextFrame, animationInterpolation);
            }
            animationValueCurrentFrame.Normalize();
            return animationValueCurrentFrame;
        }

        static Vector3 ComputeTranslationCurrentFrame(int boneIndex, AnimationClip.KeyFrame currentFrame, AnimationClip.KeyFrame nextFrame, float animationInterpolation)
        {
            var animationValueCurrentFrame = currentFrame.Position[boneIndex];
            if (nextFrame != null)
            {
                var animationValueNextFrame = nextFrame.Position[boneIndex];
                animationValueCurrentFrame = Vector3.Lerp(animationValueCurrentFrame, animationValueNextFrame, animationInterpolation);
            }

            return animationValueCurrentFrame;
        }

        static void ApplyAnimation(AnimationClip.KeyFrame currentFrame, AnimationClip.KeyFrame nextFrame, float animationInterpolation,
            AnimationFrame finalAnimationFrame, List<AnimationBoneMapping> rotMapping, List<AnimationBoneMapping> transMapping, AnimationBoneMappingType boneMappingMode)
        {
            if (currentFrame == null)
                return;

            for (int i = 0; i < finalAnimationFrame.BoneTransforms.Count(); i++)
            {
                if (transMapping[i].MappingType == boneMappingMode)
                    finalAnimationFrame.BoneTransforms[i].Translation = ComputeTranslationCurrentFrame(transMapping[i].Id, currentFrame, nextFrame, animationInterpolation);

                if (rotMapping[i].MappingType == boneMappingMode)
                    finalAnimationFrame.BoneTransforms[i].Rotation = ComputeRotationsCurrentFrame(rotMapping[i].Id, currentFrame, nextFrame, animationInterpolation);
            }
        }



    }

   
}
