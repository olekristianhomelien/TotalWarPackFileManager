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
       /* protected override void UpdateNode(GameTime time)
        {
            if (ViewModel.SelectedMesh != null && ViewModel.SelectedBone != null)
            {
                var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(ViewModel.SelectedMesh);
                var animation = SceneElementHelper.GetFirstChild<AnimationElement>(ViewModel.SelectedMesh);
                if (skeleton != null && animation != null)
                {
                    var bonePos = skeleton.Skeleton.WorldTransform[ViewModel.SelectedBone.Id];
                    WorldTransform = Matrix.Identity;

                    WorldTransform = Matrix.CreateTranslation(Matrix.Multiply(bonePos, GetAnimatedBone(ViewModel.SelectedBone.Id, animation)).Translation + new Vector3(0.0f, 0.8f, -0.15f));
                    //WorldTransform = bonePos;
                    return;
                }
            }

            WorldTransform = Matrix.Identity;
        }*/
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

    }

    public class AnimationPlayer
    {
        public AnimationPlayerSettings Settings { get; set; } = new AnimationPlayerSettings();


        AnimationFile[] _animations;
        Skeleton _skeleton;
        int _currentFrame;

        bool _animateInPlace = false;
        public bool AnimateInPlace 
        { 
            get { return _animateInPlace; } 
            set { _animateInPlace = value; } 
        }

        bool _applyStaticFrame = true;
        public bool ApplyStaticFrame
        {
            get { return _applyStaticFrame; }
            set {_applyStaticFrame = value; }
        }

        bool _applyDynamicFrames = true;
        public bool ApplyDynamicFrames
        {
            get { return _applyDynamicFrames; }
            set { _applyDynamicFrames = value; }
        }

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

                    if (_currentFrame >= FrameCount() - 1 )
                        _currentFrame = 0;
                }

                ComputeCurrentFrame();
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
            if (_animations != null)
                return _animations[0].DynamicFrames.Count();
            return 0;
        }


        public void ComputeCurrentFrame()
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

           
                EnsureSaticRootNode(currentFrame);

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

        void EnsureSaticRootNode(AnimationFrame currentFrame)
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
                        boneTransform.Transform = matrix;
                    }

                    if (Settings.FreezeAnimationBone)
                    {
                        if (boneTransform.BoneIndex == 7)
                        {
                            var matrix = boneTransform.Transform;
                            rootOfset += boneTransform.Transform.Translation;
                            matrix.Translation = new Vector3(0, 0, 0);
                            boneTransform.Transform = matrix;
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
                        boneTransform.Transform = matrix;
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
