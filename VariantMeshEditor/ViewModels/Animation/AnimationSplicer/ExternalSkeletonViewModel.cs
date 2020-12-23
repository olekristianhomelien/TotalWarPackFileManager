using CommonDialogs.Common;
using CommonDialogs.MathViews;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;
using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class ExternalSkeletonViewModel : NotifyPropertyChangedImpl
    {
        Vector3ViewModel _skeletonOffset = new Vector3ViewModel(3, 0, 0);
        public Vector3ViewModel SkeletonOffset
        {
            get { return _skeletonOffset; }
            set { SetAndNotify(ref _skeletonOffset, value); }
        }

        bool _drawSkeleton = true;
        public bool DrawSkeleton
        {
            get { return _drawSkeleton; }
            set { SetAndNotify(ref _drawSkeleton, value); }
        }

        bool _drawLine = false;
        public bool DrawLine
        {
            get { return _drawLine; }
            set { SetAndNotify(ref _drawLine, value); }
        }

        SkeletonElement _externalElement;
        AnimationPlayer _animationPlayer;
        public void Create(ResourceLibary resourceLibary, string skeletonName)
        {
            if (_externalElement != null)
                _externalElement.Dispose();

            _externalElement = new SkeletonElement(null, "");
            _externalElement.IsChecked = true;
            _animationPlayer = new AnimationPlayer();
            _externalElement.Create(_animationPlayer, resourceLibary, skeletonName);

            _externalElement.SkeletonRenderer.LineColour = new Vector3(1, 0, 0);
            _externalElement.SkeletonRenderer.NodeColour = new Vector3(1, 1, 1);
        }

        public void SetSelectedBone(int index)
        {
            if (_externalElement != null)
            {
                _externalElement.ViewModel.SelectedBone = _externalElement.ViewModel.GetBoneFromIndex(index, _externalElement.ViewModel.Bones);
            }
        }


        public void SetFrame(int currentFrame)
        {
            if (_externalElement != null)
                _animationPlayer.CurrentFrame = currentFrame;
        }

        public void SetAnimation(PackedFile animationFile)
        {
            if (animationFile == null)
            {
                _animationPlayer?.SetAnimation(null, _externalElement?.GameSkeleton);
            }
            else
            {
                var externalAnim = AnimationFile.Create(animationFile);
                var externalAnimationClip = new AnimationClip(externalAnim);
                _animationPlayer.SetAnimation(externalAnimationClip, _externalElement.GameSkeleton);
            }
        }

        public void UpdateNode(GameTime time)
        {
            _externalElement?.Update(time);
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            var matrix = Matrix.CreateTranslation((float)_skeletonOffset.X.Value, (float)_skeletonOffset.Y.Value, (float)_skeletonOffset.Z.Value);
            _externalElement?.Render(device, matrix, commonShaderParameters);
        }
    }
}
