using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using Viewer.Animation;
using WpfTest.Scenes;
using static Viewer.Animation.AnimationClip;

namespace Viewer.GraphicModels
{
    public class SkeletonModel : RenderItem
    {
        AnimationPlayer _animationPlayer;
        Skeleton _skeleton;
        Matrix[] _drawPositions;
        LineBox _lineBox;


        Vector3 _defaultColour = new Vector3(.25f, 1, .25f);
        Vector3 _selectedColour = new Vector3(1, 0, 0);

        public int? SelectedBoneIndex { get; set; }
        public SkeletonModel(Effect shader) : base(null, shader)
        {
        }

        public void Create(AnimationPlayer animationPlayer, Skeleton skeleton)
        {
            _lineBox = new LineBox();
            _skeleton = skeleton;
            _animationPlayer = animationPlayer;
            _drawPositions = new Matrix[_skeleton.BoneCount];
        }

        public override void Update(GameTime time)
        {
            AnimationFrame frame = _animationPlayer.GetCurrentFrame();

            for (int i = 0; i < _skeleton.BoneCount; i++)
            {
                var parentIndex = _skeleton.ParentBoneId[i];
                if (parentIndex == -1)
                    continue;

                _drawPositions[i] = _skeleton.WorldTransform[i];
                if (frame != null)
                {
                    var currentBoneAnimationoffset = frame.BoneTransforms[i].Transform;
                    _drawPositions[i] = _drawPositions[i] * currentBoneAnimationoffset;
                }
            }
        }

        public override void Draw(GraphicsDevice device, Matrix world, CommonShaderParameters commonShaderParameters)
        {
            if (!Visible)
                return;

            for (int i = 0; i < _skeleton.BoneCount; i++)
            {
                var parentIndex = _skeleton.ParentBoneId[i];
                if (parentIndex == -1)
                    continue;

                Vector3 drawColour = _defaultColour;
                if (SelectedBoneIndex.HasValue && SelectedBoneIndex.Value == i)
                    drawColour = _selectedColour;

                var vertices = new[]
                {
                    new VertexPosition(Vector3.Transform(_drawPositions[i].Translation, world)),
                    new VertexPosition(Vector3.Transform(_drawPositions[parentIndex].Translation, world))
                };

                foreach (var pass in _shader.CurrentTechnique.Passes)
                {
                    ApplyCommonShaderParameters(commonShaderParameters, Matrix.Identity);
                    _shader.Parameters["Color"].SetValue(new Vector3(0,0,0));
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                }

                DrawCube(device, commonShaderParameters, world * Matrix.CreateScale(0.05f) * _drawPositions[i], drawColour);
            }
        }

        void DrawCube(GraphicsDevice device, CommonShaderParameters commonShaderParameters, Matrix world, Vector3 colour)
        {
            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                ApplyCommonShaderParameters(commonShaderParameters, world);
                _shader.Parameters["Color"].SetValue(colour);
                pass.Apply();
                _lineBox.Render(device);
            }
        }
    }
}
