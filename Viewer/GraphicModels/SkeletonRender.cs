using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Viewer.Animation;
using WpfTest.Scenes;

namespace Viewer.GraphicModels
{
    public class SkeletonRender : RenderItem
    {
        GameSkeleton _gameSkeleton;
        LineBox _lineBox;

        public Vector3 NodeColour = new Vector3(.25f, 1, .25f);
        public Vector3 SelectedNodeColour = new Vector3(1, 0, 0);
        public Vector3 LineColour = new Vector3(0, 0, 0);

        public int? SelectedBoneIndex { get; set; }
        public SkeletonRender(Effect shader, GameSkeleton skeleton) : base(null, shader)
        {
            _lineBox = new LineBox();
            _gameSkeleton = skeleton;
        }

        public override void Draw(GraphicsDevice device, Matrix world, CommonShaderParameters commonShaderParameters)
        {
            if (!Visible)
                return;

            for (int i = 0; i < _gameSkeleton.BoneCount; i++)
            {
                var parentIndex = _gameSkeleton.GetParentBone(i);
                if (parentIndex == -1)
                    continue;

                Vector3 drawColour = NodeColour;
                if (SelectedBoneIndex.HasValue && SelectedBoneIndex.Value == i)
                    drawColour = SelectedNodeColour;

                var boneMatrix = _gameSkeleton.GetAnimatedWorldTranform(i);
                var parentBoneMatrix = _gameSkeleton.GetAnimatedWorldTranform(parentIndex);

                var vertices = new[]
                {
                    new VertexPosition(Vector3.Transform(boneMatrix.Translation, world)),
                    new VertexPosition(Vector3.Transform(parentBoneMatrix.Translation, world))
                };

                foreach (var pass in _shader.CurrentTechnique.Passes)
                {
                    ApplyCommonShaderParameters(commonShaderParameters, Matrix.Identity);
                    _shader.Parameters["Color"].SetValue(LineColour);
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                }

                DrawCube(device, commonShaderParameters, Matrix.CreateScale(0.05f) * boneMatrix * world, drawColour);
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
