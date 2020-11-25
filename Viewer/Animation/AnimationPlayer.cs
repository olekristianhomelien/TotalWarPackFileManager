using Filetypes.RigidModel;
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
        public Matrix Transform { get; set; } = Matrix.Identity;

        public void UpdateNode(GameTime time)
        {
            if (!HasAnimation || ExternalPlayer == null)
            {
                Transform = Matrix.Identity;
                return;
            }

            // Update if needed
            if(time != null)
                ExternalPlayer.Update(time);

        
            var bonePos = ExternalPlayer._skeleton.WorldTransform[ExternalBoneIndex];

            var animPos = ExternalPlayer.GetCurrentFrame().BoneTransforms[ExternalBoneIndex].Transform;
            Transform =  Matrix.Multiply(bonePos, animPos);
        }
    }

    public class AnimationFrame
    {
        public class BoneKeyFrame
        {
            public int BoneIndex { get; set; }
            public int ParentBoneIndex { get; set; }
            public Quaternion Rotation { get; set; }
            public Vector3 Translation { get; set; }
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

        //AnimationFile[] _animations;
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
                
                if(_newAnimations != null)
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
            if (_newAnimations != null && IsPlaying)
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

                if (ExternalAnimationRef != null)
                    ExternalAnimationRef.UpdateNode(gameTime);
                ComputeCurrentFrame();
            }
        }

        List<AnimationClip> _newAnimations;

        public void SetAnimation(List<AnimationClip> animation, Skeleton skeleton)
        {
            _skeleton = skeleton;
            IsPlaying = true;
            _currentFrame = 0;
            _animationInterpolation = 0;

            _newAnimations = animation;

            //_animations = animation;
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
            if (_newAnimations != null)
                return _newAnimations[0].DynamicFrames.Count();
            return 0;
        }


        void ComputeCurrentFrame()
        {
            var currentFrame = new AnimationFrame();
            for (int i = 0; i < _skeleton.BoneCount; i++)
            {
                currentFrame.BoneTransforms.Add(new AnimationFrame.BoneKeyFrame()
                {
                    Transform = _skeleton.Transform[i],
                    Translation = _skeleton.Translation[i],
                    Rotation = _skeleton.Rotation[i],
                    BoneIndex = i,
                    ParentBoneIndex = _skeleton.ParentBoneId[i],
                });
            }

            if (ApplyStaticFrame)
            {
                foreach (var animation in _newAnimations)
                    ApplyAnimation(animation.StaticFrame, null, (float)_animationInterpolation, animation.StaticTranslationMappingID, animation.StaticRotationMappingID, currentFrame);
            }

            if (ApplyDynamicFrames)
            {
                if (_newAnimations[0].DynamicFrames.Count > _currentFrame)
                {
                    var currentFrameKeys = _newAnimations[0].DynamicFrames[_currentFrame];
                    var nextFrameKeys = _newAnimations[0].DynamicFrames[_currentFrame + 1];
                    ApplyAnimation(currentFrameKeys, nextFrameKeys, (float)_animationInterpolation, _newAnimations[0].DynamicTranslationMappingID, _newAnimations[0].DynamicRotationMappingID, currentFrame);
                }
            }











            HandleFreezeAnimation(currentFrame);
            HandleSnapToExternalAnimation(currentFrame);
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

        void HandleSnapToExternalAnimation(AnimationFrame currentFrame)
        {
            if (ExternalAnimationRef.HasAnimation && Settings.UseAnimationSnap)
            {
                var refTransform = ExternalAnimationRef.Transform;
                currentFrame.BoneTransforms[0].Transform = Matrix.CreateTranslation(refTransform.Translation); ;// * currentFrame.BoneTransforms[0].Transform ;
            }
        }

        Quaternion ComputeRotationsCurrentFrame(int boneIndex, AnimationClip.KeyFrame currentFrame, AnimationClip.KeyFrame nextFrame, float animationInterpolation)
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

        Vector3 ComputeTranslationCurrentFrame(int boneIndex, AnimationClip.KeyFrame currentFrame, AnimationClip.KeyFrame nextFrame, float animationInterpolation)
        {
            var animationValueCurrentFrame = currentFrame.Translation[boneIndex];
            if (nextFrame != null)
            {
                var animationValueNextFrame = nextFrame.Translation[boneIndex];
                animationValueCurrentFrame = Vector3.Lerp(animationValueCurrentFrame, animationValueNextFrame, animationInterpolation);
            }

            return animationValueCurrentFrame;
        }

        void ApplyAnimation(AnimationClip.KeyFrame currentFrame , AnimationClip.KeyFrame nextFrame, float animationInterpolation, List<int> translationMappings, List<int> rotationMapping, AnimationFrame finalAnimationFrame)
        {
            if (currentFrame == null)
                return;

            for (int i = 0; i < finalAnimationFrame.BoneTransforms.Count(); i++)
            {
                Quaternion rotation = finalAnimationFrame.BoneTransforms[i].Rotation;
                Vector3 translation = finalAnimationFrame.BoneTransforms[i].Translation;

                var translationIndex = translationMappings.IndexOf(i);
                if (translationIndex != -1)
                    translation = ComputeTranslationCurrentFrame(translationIndex, currentFrame, nextFrame, animationInterpolation);

                var rotationIndex = rotationMapping.IndexOf(i);
                if (rotationIndex != -1)
                    rotation = ComputeRotationsCurrentFrame(rotationIndex, currentFrame, nextFrame, animationInterpolation);

                finalAnimationFrame.BoneTransforms[i].Transform = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation);
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
