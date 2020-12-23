using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using Viewer.Animation;
using WpfTest.Scenes;


namespace Viewer.GraphicModels
{
    public class SkeletonRender : RenderItem
    {
        AnimationPlayer _animationPlayer;
        GameSkeleton _skeleton;
        Matrix[] _drawPositions;
        LineBox _lineBox;


        public Vector3 NodeColour = new Vector3(.25f, 1, .25f);
        public Vector3 SelectedNodeColour = new Vector3(1, 0, 0);
        public Vector3 LineColour = new Vector3(0, 0, 0);

        public int? SelectedBoneIndex { get; set; }
        public SkeletonRender(Effect shader) : base(null, shader)
        {
        }

        public void Create(AnimationPlayer animationPlayer, GameSkeleton skeleton)
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

                Vector3 drawColour = NodeColour;
                if (SelectedBoneIndex.HasValue && SelectedBoneIndex.Value == i)
                    drawColour = SelectedNodeColour;

                var vertices = new[]
                {
                    new VertexPosition(Vector3.Transform(_drawPositions[i].Translation, world)),
                    new VertexPosition(Vector3.Transform(_drawPositions[parentIndex].Translation, world))
                };

                foreach (var pass in _shader.CurrentTechnique.Passes)
                {
                    ApplyCommonShaderParameters(commonShaderParameters, Matrix.Identity);
                    _shader.Parameters["Color"].SetValue(LineColour);
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                }

                DrawCube(device, commonShaderParameters, Matrix.CreateScale(0.05f) * _drawPositions[i] * world, drawColour);
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
