using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Viewer.GraphicModels;

namespace Viewer.Animation
{
    public class AnimationClip
    {
        public class BoneKeyFrame
        {
            public int BoneIndex { get; set; }
            public int ParentBoneIndex { get; set; }
            public Matrix Transform { get; set; }

            public BoneKeyFrame Clone()
            {
                var newItem = new BoneKeyFrame()
                {
                    BoneIndex = BoneIndex,
                    ParentBoneIndex = ParentBoneIndex,
                    Transform = Transform,
                };
                return newItem;
            }
        }

        AnimationFile[] _animations;
        Skeleton _skeleton;

        public List<AnimationFrame> KeyFrameCollection = new List<AnimationFrame>();

        public class AnimationFrame
        {
            public List<BoneKeyFrame> BoneTransforms = new List<BoneKeyFrame>();

            public AnimationFrame Clone()
            {
                var newItem = new AnimationFrame();
                foreach (var transform in BoneTransforms)
                    newItem.BoneTransforms.Add(transform.Clone());
                return newItem;
            }
        }

        void ApplyFrame(AnimationFile.Frame frame, List<int> translationMappings, List<int> rotationMapping, AnimationFrame currentFrame)
        {
            if (frame == null)
                return;
           
            for (int i = 0; i < frame.Transforms.Count(); i++)
            {
                var dynamicIndex = translationMappings[i];
                if (dynamicIndex != -1)
                {
                    var pos = frame.Transforms[i];
                    var temp = currentFrame.BoneTransforms[dynamicIndex].Transform;
                    temp.Translation = new Vector3(pos[0], pos[1], pos[2]);
                    currentFrame.BoneTransforms[dynamicIndex].Transform = temp;
                }
            }

            for (int i = 0; i < frame.Quaternion.Count(); i++)
            {
                var dynamicIndex = rotationMapping[i];
                if (dynamicIndex != -1)
                {
                    var animQ = frame.Quaternion[i];
                    var quaternion = new Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                    quaternion.Normalize();
                    var translation = currentFrame.BoneTransforms[dynamicIndex].Transform.Translation;
                    currentFrame.BoneTransforms[dynamicIndex].Transform = Matrix.CreateFromQuaternion(quaternion) * Matrix.CreateTranslation(translation);
                }
            }
        }

        public void ReCreate(bool animateInPlace, bool applyDynamicFrames, bool applyStaticFrames)
        {
            KeyFrameCollection.Clear();

            var defaultFrame = new AnimationFrame();
            for (int i = 0; i < _skeleton.BoneCount; i++)
            {
                defaultFrame.BoneTransforms.Add(new BoneKeyFrame()
                {
                    Transform = _skeleton.Transform[i],
                    BoneIndex = i,
                    ParentBoneIndex = _skeleton.ParentBoneId[i],
                });
            }

            if (applyStaticFrames)
            {
                foreach(var animation in _animations)
                    ApplyFrame(animation.StaticFrame, animation.StaticTranslationMappingID, animation.StaticRotationMappingID, defaultFrame);
            
                if (_animations[0].DynamicFrames.Count() == 0 || applyDynamicFrames == false)
                    KeyFrameCollection.Add(defaultFrame);
            }
            
            if(applyDynamicFrames)
            {
                for (int frameIndex = 0; frameIndex < _animations[0].DynamicFrames.Count(); frameIndex++)
                {
                    var animationKeyFrameData = _animations[0].DynamicFrames[frameIndex];
            
                    var currentFrame = defaultFrame.Clone();
                    ApplyFrame(animationKeyFrameData, _animations[0].DynamicTranslationMappingID, _animations[0].DynamicRotationMappingID, currentFrame);
                    KeyFrameCollection.Add(currentFrame);
                }
            }

            if (animateInPlace)
            {
                foreach (var keyframe in KeyFrameCollection)
                {
                    Vector3 rootOfset = new Vector3(0);
                    Vector3 animRootOffset = new Vector3(0);
                    foreach (var boneTransform in keyframe.BoneTransforms)
                    {
                        if (boneTransform.BoneIndex == 0)
                        {
                            var matrix = boneTransform.Transform;
                            animRootOffset += boneTransform.Transform.Translation;
                            matrix.Translation = new Vector3(0, 0, 0);
                            boneTransform.Transform = matrix;
                        }

                        if (boneTransform.BoneIndex == 7)
                        {
                            var matrix = boneTransform.Transform;
                            rootOfset += boneTransform.Transform.Translation;
                            matrix.Translation = new Vector3(0, 0, 0);
                            boneTransform.Transform = matrix;
                        }
                    }

                    foreach (var boneTransform in keyframe.BoneTransforms)
                    {
                        if (boneTransform.ParentBoneIndex == 0 && boneTransform.BoneIndex != 7)
                        {
                            var matrix = boneTransform.Transform;
                            matrix.Translation -=  rootOfset ;
                            boneTransform.Transform = matrix;
                        }
                    }
                }
            }

            for (int frameIndex = 0; frameIndex < KeyFrameCollection.Count(); frameIndex++)
            {
                var currentFrame = KeyFrameCollection[frameIndex];

                // Move into world space
                for (int i = 0; i < currentFrame.BoneTransforms.Count(); i++)
                {
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
                for (int i = 0; i < _skeleton.BoneCount; i++)
                {
                    var inv = Matrix.Invert(_skeleton.WorldTransform[i]);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }
            }
        }

        public static AnimationClip Create(AnimationFile[] animations, Skeleton skeleton)
        {
            AnimationClip model = new AnimationClip();
            model._skeleton = skeleton;
            model._animations = animations;
            model.ReCreate(false, true, true);
            return model;
        }
    }
}
