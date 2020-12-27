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
    public class ExternalSkeletonVisualizationHelper : NotifyPropertyChangedImpl
    {
        public class ViewModel : NotifyPropertyChangedImpl
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
        }

        public ViewModel ViewModelInstance { get; set; } = new ViewModel();

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

        public void SetAnimation(AnimationClip animationClip)
        {
            _animationPlayer?.SetAnimation(animationClip, _externalElement.GameSkeleton);
        }

        public void UpdateNode(GameTime time)
        {
            _externalElement?.Update(time);
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            if (ViewModelInstance.DrawSkeleton)
            {
                var matrix = Matrix.CreateTranslation((float)ViewModelInstance.SkeletonOffset.X.Value, (float)ViewModelInstance.SkeletonOffset.Y.Value, (float)ViewModelInstance.SkeletonOffset.Z.Value);
                _externalElement?.Render(device, matrix, commonShaderParameters);
            }
        }
    }
}
