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
        public class AnimationKeyFrame
        {
            public int BoneIndex { get; set; }
            public int ParentBoneIndex { get; set; }
            public Matrix Transform { get; set; }

            public Quaternion Rotation { get; set; } = new Quaternion();
            public Vector3 Translation { get; set; } = new Vector3();

            public AnimationKeyFrame Clone()
            {
                var newItem = new AnimationKeyFrame()
                {
                    BoneIndex = BoneIndex,
                    ParentBoneIndex = ParentBoneIndex,
                    Transform = Transform,
                    Rotation = Rotation,
                    Translation = Translation
                };
                return newItem;
            }
        }

        AnimationFile[] _animations;
        Skeleton _skeleton;

        public List<AnimationFrame> KeyFrameCollection = new List<AnimationFrame>();

        public class AnimationFrame
        {
            public List<AnimationKeyFrame> BoneTransforms = new List<AnimationKeyFrame>();

            public AnimationFrame Clone()
            {
                var newItem = new AnimationFrame();
                foreach (var transform in BoneTransforms)
                    newItem.BoneTransforms.Add(transform.Clone());
                return newItem;
            }
        }

        void ApplyFrame(bool animateInPlace, AnimationFile.Frame frame, List<int> translationMappings, List<int> rotationMapping, AnimationFrame currentFrame)
        {
            if (frame == null)
                return;
           
            for (int i = 0; i < frame.Transforms.Count(); i++)
            {
                var dynamicIndex = translationMappings[i];
                if (dynamicIndex != -1)
                {
                    var pos = frame.Transforms[i];
                    if (animateInPlace && dynamicIndex == 0)
                        pos = new float[] { 0, 0, 0 };

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
                defaultFrame.BoneTransforms.Add(new AnimationKeyFrame()
                {
                    Transform = (_skeleton.Transform[i]),
                    BoneIndex = i,
                    ParentBoneIndex = _skeleton.ParentBoneId[i],
                    Translation = _skeleton.Translation[i],
                    Rotation = _skeleton.Rotation[i]
                });
            }

            if (applyStaticFrames)
            {
                foreach(var animation in _animations)
                  ApplyFrame(animateInPlace, animation.StaticFrame, animation.StaticTranslationMappingID, animation.StaticRotationMappingID, defaultFrame);
            
                if (_animations[0].DynamicFrames.Count() == 0 || applyDynamicFrames == false)
                {
                  KeyFrameCollection.Add(defaultFrame);
                }
            }
            
            if(applyDynamicFrames)
            {
                for (int frameIndex = 0; frameIndex < _animations[0].DynamicFrames.Count(); frameIndex++)
                {
                    var animationKeyFrameData = _animations[0].DynamicFrames[frameIndex];
            
                    var currentFrame = defaultFrame.Clone();
                    ApplyFrame(animateInPlace, animationKeyFrameData, _animations[0].DynamicTranslationMappingID, _animations[0].DynamicRotationMappingID, currentFrame);
                    KeyFrameCollection.Add(currentFrame);
                }
            }

            for (int frameIndex = 0; frameIndex < KeyFrameCollection.Count(); frameIndex++)
            {
                var currentFrame = KeyFrameCollection[frameIndex];


                // Build transform
                //for (int i = 0; i < currentFrame.BoneTransforms.Count(); i++)
                //{
                //    currentFrame.BoneTransforms[i].Rotation.Normalize();
                //    currentFrame.BoneTransforms[i].Transform = Matrix.CreateFromQuaternion(currentFrame.BoneTransforms[i].Rotation) * Matrix.CreateTranslation(currentFrame.BoneTransforms[i].Translation);
                //}
                //
                
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
