﻿using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Viewer.Animation
{

    public class ExternalAnimationAttachmentResolver
    {
        public AnimationPlayer ExternalPlayer { get; set; }
        public int ExternalBoneIndex { get; set; } = -1;
        public bool HasAnimation { get { return ExternalPlayer != null && ExternalBoneIndex != -1; } }

        public Matrix UpdateNode(GameTime time)
        {
            if (!HasAnimation)
                return Matrix.Identity;

            // Update if needed
            if(time != null)
                ExternalPlayer.Update(time);

        
            var bonePos = ExternalPlayer._skeleton.WorldTransform[ExternalBoneIndex];

            var animPos = ExternalPlayer.GetCurrentFrame().BoneTransforms[ExternalBoneIndex].Transform;
            return Matrix.Multiply(bonePos, animPos);
        }

 
    }

    public class AnimationFrame
    {
        public class BoneKeyFrame
        {
            public int BoneIndex { get; set; }
            public int ParentBoneIndex { get; set; }
            public Matrix Transform { get; set; }
        }

        public List<BoneKeyFrame> BoneTransforms = new List<BoneKeyFrame>();
    }


    public class AnimationPlayerSettings
    {

        public bool UseTranslationOffset { get; set; } = false;
        public float TranslationOffsetX { get; set; } = 0;
        public float TranslationOffsetY { get; set; } = 0;
        public float TranslationOffsetZ { get; set; } = 0;


        public bool UseRotationOffset { get; set; } = false;
        public float RotationOffsetX { get; set; } = 0;
        public float RotationOffsetY { get; set; } = 0;
        public float RotationOffsetZ { get; set; } = 0;


        public bool FreezeAnimationRoot { get; set; } = false;
        public bool FreezeAnimationBone { get; set; } = false;
        public int FreezeAnimationBoneIndex { get; set; } = -1;


        public bool UseAnimationSnap { get; set; } = false;
        public bool OnlySnapTranslations { get; set; } = false;
    }

    public class AnimationPlayer
    {
        public AnimationPlayerSettings Settings { get; set; } = new AnimationPlayerSettings();
        public ExternalAnimationAttachmentResolver ExternalAnimationRef { get; set; } = new ExternalAnimationAttachmentResolver();

        AnimationFile[] _animations;
        public Skeleton _skeleton;
        int _currentFrame;
        TimeSpan _timeAtCurrentFrame;
        double _animationInterpolation = 0;
        AnimationFrame _currentAnimFrame;


        public bool IsPlaying { get; private set; } = false;
        public double FrameRate { get; set; } = 20.0 / 1000.0;
        public bool ApplyStaticFrame { get; set; } = true;
        public bool ApplyDynamicFrames { get; set; } = true;

        public int CurrentFrame
        {
            get { return _currentFrame; }
            set 
            {
                if (value < 0)
                    value = 0;
                
                if(_animations != null)
                {
                    if (value >= FrameCount() - 1)
                        value = FrameCount() - 2;
                }
                
                _currentFrame = value;
                _animationInterpolation = 0;
                ComputeCurrentFrame();
            }
        }


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

                    if (_currentFrame >= FrameCount() - 1 )
                        _currentFrame = 0;
                }

                ComputeCurrentFrame(gameTime);
            }
        }


        public void SetAnimation(AnimationFile[] animation, Skeleton skeleton)
        {
            _skeleton = skeleton;
            IsPlaying = true;
            _currentFrame = 0;
            _animationInterpolation = 0;
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
            if (_animations != null)
                return _animations[0].DynamicFrames.Count();
            return 0;
        }


        void ComputeCurrentFrame(GameTime time = null)
        {
            var currentFrame = new AnimationFrame();
            for (int i = 0; i < _skeleton.BoneCount; i++)
            {
                currentFrame.BoneTransforms.Add(new AnimationFrame.BoneKeyFrame()
                {
                    Transform = _skeleton.Transform[i],
                    BoneIndex = i,
                    ParentBoneIndex = _skeleton.ParentBoneId[i],
                });
            }

            if (ApplyStaticFrame)
            {
                foreach (var animation in _animations)
                    ApplyFrame(animation.StaticFrame, null, (float)_animationInterpolation, animation.StaticTranslationMappingID, animation.StaticRotationMappingID, currentFrame);
            }

            if (ApplyDynamicFrames)
            {
                if (_animations[0].DynamicFrames.Count > _currentFrame)
                {
                    var currentFrameKeys = _animations[0].DynamicFrames[_currentFrame];
                    var nextFrameKeys = _animations[0].DynamicFrames[_currentFrame + 1];
                    ApplyFrame(currentFrameKeys, nextFrameKeys, (float)_animationInterpolation, _animations[0].DynamicTranslationMappingID, _animations[0].DynamicRotationMappingID, currentFrame);
                }
            }

           
            HandleFreezeAnimation(currentFrame);
           
            HandleSnapToExternalAnimation(currentFrame, time);
            OffsetAnimation(currentFrame);

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

            _currentAnimFrame = currentFrame;
        }

        void HandleFreezeAnimation(AnimationFrame currentFrame)
        {
            if (Settings.FreezeAnimationRoot)
            {
                Vector3 rootOfset = new Vector3(0);
                Vector3 animRootOffset = new Vector3(0);
                foreach (var boneTransform in currentFrame.BoneTransforms)
                {
                    if (boneTransform.BoneIndex == 0)
                    {
                        var matrix = boneTransform.Transform;
                        animRootOffset += boneTransform.Transform.Translation;
                        matrix.Translation = new Vector3(0, 0, 0);
                        boneTransform.Transform = Matrix.Identity;
                    }

                    if (Settings.FreezeAnimationBone)
                    {
                        if (boneTransform.BoneIndex == 7)
                        {
                            var matrix = boneTransform.Transform;
                            rootOfset += boneTransform.Transform.Translation;
                            matrix.Translation = new Vector3(0, 0, 0);
                            boneTransform.Transform = Matrix.Identity;
                        }
                    }
                }

                foreach (var boneTransform in currentFrame.BoneTransforms)
                {
                    bool test = Settings.FreezeAnimationBone && boneTransform.BoneIndex != 7;
                    if (boneTransform.ParentBoneIndex == 0 && test)
                    {
                        var matrix = boneTransform.Transform;
                        matrix.Translation -= rootOfset;
                        boneTransform.Transform = Matrix.Identity;
                    }
                }
            }
        }

        void OffsetAnimation(AnimationFrame currentFrame)
        {
            var translationMatrix = Matrix.Identity;
            var roationMatrix = Matrix.Identity;

            if (Settings.UseTranslationOffset)
                translationMatrix = Matrix.CreateTranslation(new Vector3(Settings.TranslationOffsetX, Settings.TranslationOffsetY, Settings.TranslationOffsetZ));

            if (Settings.UseRotationOffset)
                roationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(Settings.RotationOffsetX)) * Matrix.CreateRotationY(MathHelper.ToRadians(Settings.RotationOffsetY)) * Matrix.CreateRotationZ(MathHelper.ToRadians(Settings.RotationOffsetZ));

            var matrix = currentFrame.BoneTransforms[0].Transform;
            matrix = roationMatrix * translationMatrix * matrix;
            currentFrame.BoneTransforms[0].Transform = matrix;
        }

        void HandleSnapToExternalAnimation(AnimationFrame currentFrame, GameTime time)
        {
            if (ExternalAnimationRef.HasAnimation && Settings.UseAnimationSnap)
            {
                var refTransform = ExternalAnimationRef.UpdateNode(time);
                //foreach (var transform in currentFrame.BoneTransforms)
                {
                    currentFrame.BoneTransforms[0].Transform = Matrix.CreateTranslation(refTransform.Translation); ;// * currentFrame.BoneTransforms[0].Transform ;
                }
            }
        }

        void ApplyFrame(AnimationFile.Frame currentFrame, AnimationFile.Frame nextFrame, float animationInterpolation, List<int> translationMappings, List<int> rotationMapping, AnimationFrame finalAnimationFrame)
        {
            if (currentFrame == null)
                return;

            for (int i = 0; i < currentFrame.Transforms.Count(); i++)
            {
                var dynamicIndex = translationMappings[i];
                if (dynamicIndex != -1)
                {
                    var animationValueCurrentFrame = VectorFromFloatArray(currentFrame.Transforms[i]);
                    if (nextFrame != null)
                    {   
                        var animationValueNextFrame = VectorFromFloatArray(nextFrame.Transforms[i]);
                        animationValueCurrentFrame = Vector3.Lerp(animationValueCurrentFrame, animationValueNextFrame, animationInterpolation);
                    }

                    var temp = finalAnimationFrame.BoneTransforms[dynamicIndex].Transform;
                    temp.Translation = animationValueCurrentFrame;
                    finalAnimationFrame.BoneTransforms[dynamicIndex].Transform = temp;
                }
            }

            for (int i = 0; i < currentFrame.Quaternion.Count(); i++)
            {
                var dynamicIndex = rotationMapping[i];
                if (dynamicIndex != -1)
                {
                    var animationValueCurrentFrame = QuaternionFromFloatArray(currentFrame.Quaternion[i]);
                    if (nextFrame != null)
                    {
                        var animationValueNextFrame = QuaternionFromFloatArray(nextFrame.Quaternion[i]);
                        animationValueCurrentFrame = Quaternion.Slerp(animationValueCurrentFrame, animationValueNextFrame, animationInterpolation);
                    }
                    animationValueCurrentFrame.Normalize();
                    var translation = finalAnimationFrame.BoneTransforms[dynamicIndex].Transform.Translation;
                    finalAnimationFrame.BoneTransforms[dynamicIndex].Transform = Matrix.CreateFromQuaternion(animationValueCurrentFrame) * Matrix.CreateTranslation(translation);
                }
            }
        }

        Vector3 VectorFromFloatArray(float[] array)
        {
            return new Vector3(array[0], array[1], array[2]);
        }

        Quaternion QuaternionFromFloatArray(short[] array)
        {
            return new Quaternion(array[0], array[1], array[2], array[3]);
        }
    }
}
