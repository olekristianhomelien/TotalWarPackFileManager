using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Viewer.Animation.AnimationClip;

namespace Viewer.Animation
{
    public class AnimationPlayer
    {
        int _currentFrame;

        bool _animateInPlace = false;
        public bool AnimateInPlace 
        { 
            get { return _animateInPlace; } 
            set 
            { 
                _animateInPlace = value;
                Recreate();
            } 
        }

        bool _applyStaticFrame = true;
        public bool ApplyStaticFrame
        {
            get { return _applyStaticFrame; }
            set
            {
                _applyStaticFrame = value;
                Recreate();
            }
        }

        bool _applyDynamicFrames = true;
        public bool ApplyDynamicFrames
        {
            get { return _applyDynamicFrames; }
            set
            {
                _applyDynamicFrames = value;
                Recreate();
            }
        }

        public int CurrentFrame
        {
            get { return _currentFrame; }
            set 
            {
                //if (value < 0)
                //    value = 0;
                //
                //if(_currentFrame != null)
                //{
                //    if (value > FrameCount() - 1)
                //        value = FrameCount() - 1;
                //}
                //
                //_currentFrame = value; 
            }
        }

        TimeSpan _timeAtCurrentFrame;
        public double FrameRate { get; set; } = 20.0 / 1000.0;
        double _animationInterpolation = 0;
        public bool IsPlaying { get; private set; } = false;


        AnimationFrame _currentAnimFrame;

        public void Update(GameTime gameTime)
        {
            if (_animations != null && IsPlaying)
            {



                _timeAtCurrentFrame += gameTime.ElapsedGameTime;
                _animationInterpolation = _timeAtCurrentFrame.TotalMilliseconds / (FrameRate * 1000);
                if (_timeAtCurrentFrame.TotalMilliseconds >= FrameRate * 1000)
                {
                    _animationInterpolation = 0;
                    _timeAtCurrentFrame = TimeSpan.FromSeconds(0);
                    _currentFrame++;

                    if (_currentFrame >= _animations[0].DynamicFrames.Count() -1 )
                        _currentFrame = 0;

                  


                }


                _currentAnimFrame = ComputeCurrentFrame(false, true, true);

            }





        }


        public void SetAnimation(AnimationFile[] animation, Skeleton skeleton)
        {
            _skeleton = skeleton;
            IsPlaying = true;
            _currentFrame = 0;
            _animations = animation;
            _timeAtCurrentFrame = TimeSpan.FromSeconds(0);
        }

        public void Play() { IsPlaying = true; }

        public void Pause() { IsPlaying = false; }

        public AnimationFrame GetCurrentFrame()
        {
            return _currentAnimFrame;
        }

        public int FrameCount()
        {
            //if (_animations != null)
            //    return _currentAnimation.KeyFrameCollection.Count();
            return 0;
        }


        void Recreate()
        {
            //_currentAnimation.ReCreate(AnimateInPlace, ApplyDynamicFrames, ApplyStaticFrame);
            //var totalFrames = _currentAnimation.KeyFrameCollection.Count();
            //if (totalFrames == 1 || totalFrames == 0)
            //    _currentFrame = 0;
        }











        AnimationFile[] _animations;
        Skeleton _skeleton;



        public AnimationFrame ComputeCurrentFrame(bool animateInPlace, bool applyDynamicFrames, bool applyStaticFrames)
        {

            //_animationInterpolation

            int currentFrameIndex = _currentFrame;
            //int nextFrameIndex = 2;
            //float lerpValue = 0.5f;


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
                foreach (var animation in _animations)
                    ApplyFrame(animation.StaticFrame, animation.StaticTranslationMappingID, animation.StaticRotationMappingID, defaultFrame);
            }

            if (applyDynamicFrames)
            {
                var animationKeyFrameData = _animations[0].DynamicFrames[currentFrameIndex];
                ApplyFrame(animationKeyFrameData, _animations[0].DynamicTranslationMappingID, _animations[0].DynamicRotationMappingID, defaultFrame);

            }

            var currentFrame = defaultFrame;

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
            return defaultFrame;
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
    }
}
