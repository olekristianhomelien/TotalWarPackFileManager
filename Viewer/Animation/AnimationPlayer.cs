using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Filetypes.RigidModel.AnimationFile;

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

        public Skeleton _skeleton;
        TimeSpan _timeSinceStart;
        AnimationFrame _currentAnimFrame;
        List<AnimationClip> _animationClips;

        public bool IsPlaying { get; private set; } = false;
        public double SpeedMultiplication { get; set; }
        public bool ApplyStaticFrame { get; set; } = true;
        public bool ApplyDynamicFrames { get; set; } = true;

        public int CurrentFrame
        {
            get { return (int)Math.Round(_timeSinceStart.TotalSeconds * 20); }
            set 
            {
                if(_animationClips != null)
                {
                    var frameCount = FrameCount();
                    if (frameCount > 0)
                    {
                        int newFrame = value;
                        _timeSinceStart = TimeSpan.FromMilliseconds(newFrame * (1f / 20f) * 1000);
                    }
                }

                UpdateAnimationFrame();
            }
        }

        public void SetAnimation(AnimationClip animation, Skeleton skeleton)
        {
            if (animation == null)
                SetAnimationArray(null, skeleton);
            else
                SetAnimationArray(new List<AnimationClip>() { animation }, skeleton);
        }

        public void SetAnimationArray(List<AnimationClip> animation, Skeleton skeleton)
        {
            _skeleton = skeleton;

            if (_animationClips == null)
                IsPlaying = true;

            _animationClips = animation;
            _timeSinceStart = TimeSpan.FromSeconds(0);
        }

        float GetAnimationLengthMs()
        {
            if (_animationClips != null && _animationClips.Any())
            {
                return _animationClips[0].DynamicFrames.Count() * (1f / 20f) * 1000;
            }
            return 0;
        }


        public void Update(GameTime gameTime)
        {
            float animationLengthMs = GetAnimationLengthMs();

            if (animationLengthMs != 0 && IsPlaying)
            {
                _timeSinceStart += gameTime.ElapsedGameTime;
                if (_timeSinceStart.TotalMilliseconds >= animationLengthMs)
                {
                    _timeSinceStart = TimeSpan.FromSeconds(0);
                }

                if (ExternalAnimationRef != null)
                    ExternalAnimationRef.UpdateNode(gameTime);
            }

            UpdateAnimationFrame();
        }

        void UpdateAnimationFrame()
        {
            float sampleT = 0;
            float animationLengthMs = GetAnimationLengthMs();
            if (animationLengthMs != 0)
                sampleT = (float)(_timeSinceStart.TotalMilliseconds / animationLengthMs);
            _currentAnimFrame = AnimationSampler.Sample(sampleT, _skeleton, _animationClips, ApplyStaticFrame, ApplyDynamicFrames);
        }



        public void Play() { IsPlaying = true; }

        public void Pause() { IsPlaying = false; }

        public AnimationFrame GetCurrentFrame()
        {
            return _currentAnimFrame;
        }

        public int FrameCount()
        {
            if (_animationClips != null)
                return _animationClips[0].DynamicFrames.Count();
            return 0;
        }



        

        //-------------Move to somewhere else
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
        ////
    }
}
