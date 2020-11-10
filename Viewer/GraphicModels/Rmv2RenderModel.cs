using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Viewer.Animation;
using Viewer.Scene;

namespace Viewer.GraphicModels
{
    public class Rmv2RenderModel : MeshModel
    {
        Rmv2LodModel _model;
        VertexPositionNormalTexture[] _bufferArray;
        
        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, Rmv2LodModel lodModel)
        {
            _animationPlayer = animationPlayer;
            _model = lodModel;
            _bufferArray = new VertexPositionNormalTexture[_model.VertexArray.Length];
            for (int i = 0; i < _model.VertexArray.Length; i++)
            {
                var vertex = _model.VertexArray[i];
                _bufferArray[i].TextureCoordinate = new Vector2(vertex.Uv0, vertex.Uv1);
            }

            Pivot = new Vector3(_model.Transformation.Pivot.X, _model.Transformation.Pivot.Y, _model.Transformation.Pivot.Z);

            Create(animationPlayer, device, _bufferArray, _model.IndicesBuffer);
        }

        public Dictionary<TexureType, Texture2D> ResolveTextures(ResourceLibary textureLibary, GraphicsDevice device)
        {
            var textures = new Dictionary<TexureType, Texture2D>();
            foreach (var material in _model.Textures)
                textures[material.Type] = textureLibary.LoadTexture(material.Name, device);
            return textures;
        }

        public override void Render(GraphicsDevice device)
        {
            UpdateVertexBuffer();
            base.Render(device);
        }

        private void UpdateVertexBuffer()
        {
            for (int index = 0; index < _model.VertexArray.Length; index++)
            {
                var vertex = _model.VertexArray[index];
                var transformSum = GetAnimationVertex(vertex);

                _bufferArray[index].Normal = ApplyAnimation(vertex.Normal, transformSum, true);
                _bufferArray[index].Position = ApplyAnimation(vertex.Position, transformSum);
            }

            _vertexBuffer.SetData(_bufferArray);
        }

        Vector3 ApplyAnimation(FileVector3 vertex, Matrix animationTransform, bool normalize = false)
        {
            Vector3 vector = new Vector3
            {
                X = vertex.X * animationTransform.M11 + vertex.Y * animationTransform.M21 + vertex.Z * animationTransform.M31 + animationTransform.M41,
                Y = vertex.X * animationTransform.M12 + vertex.Y * animationTransform.M22 + vertex.Z * animationTransform.M32 + animationTransform.M42,
                Z = vertex.X * animationTransform.M13 + vertex.Y * animationTransform.M23 + vertex.Z * animationTransform.M33 + animationTransform.M43
            };
            if (normalize)
                vector.Normalize();
            return vector;
        }


        Matrix GetAnimationVertex(Vertex vertex)
        {
            var transformSum = Matrix.Identity;
            var animationData = _animationPlayer?.GetCurrentFrame();
            if (animationData != null)
            {
                if (_model.VertexFormat == VertexFormat.Cinematic)
                {
                    int b0 = vertex.BoneInfos[0].BoneIndex;
                    int b1 = vertex.BoneInfos[1].BoneIndex;
                    int b2 = vertex.BoneInfos[2].BoneIndex;
                    int b3 = vertex.BoneInfos[3].BoneIndex;

                    float w1 = vertex.BoneInfos[0].BoneWeight;
                    float w2 = vertex.BoneInfos[1].BoneWeight;
                    float w3 = vertex.BoneInfos[2].BoneWeight;
                    float w4 = vertex.BoneInfos[3].BoneWeight;

                    Matrix m1 = animationData.BoneTransforms[b0].Transform;
                    Matrix m2 = animationData.BoneTransforms[b1].Transform;
                    Matrix m3 = animationData.BoneTransforms[b2].Transform;
                    Matrix m4 = animationData.BoneTransforms[b3].Transform;
                    transformSum.M11 = (m1.M11 * w1) + (m2.M11 * w2) + (m3.M11 * w3) + (m4.M11 * w4);
                    transformSum.M12 = (m1.M12 * w1) + (m2.M12 * w2) + (m3.M12 * w3) + (m4.M12 * w4);
                    transformSum.M13 = (m1.M13 * w1) + (m2.M13 * w2) + (m3.M13 * w3) + (m4.M13 * w4);
                    transformSum.M21 = (m1.M21 * w1) + (m2.M21 * w2) + (m3.M21 * w3) + (m4.M21 * w4);
                    transformSum.M22 = (m1.M22 * w1) + (m2.M22 * w2) + (m3.M22 * w3) + (m4.M22 * w4);
                    transformSum.M23 = (m1.M23 * w1) + (m2.M23 * w2) + (m3.M23 * w3) + (m4.M23 * w4);
                    transformSum.M31 = (m1.M31 * w1) + (m2.M31 * w2) + (m3.M31 * w3) + (m4.M31 * w4);
                    transformSum.M32 = (m1.M32 * w1) + (m2.M32 * w2) + (m3.M32 * w3) + (m4.M32 * w4);
                    transformSum.M33 = (m1.M33 * w1) + (m2.M33 * w2) + (m3.M33 * w3) + (m4.M33 * w4);
                    transformSum.M41 = (m1.M41 * w1) + (m2.M41 * w2) + (m3.M41 * w3) + (m4.M41 * w4);
                    transformSum.M42 = (m1.M42 * w1) + (m2.M42 * w2) + (m3.M42 * w3) + (m4.M42 * w4);
                    transformSum.M43 = (m1.M43 * w1) + (m2.M43 * w2) + (m3.M43 * w3) + (m4.M43 * w4);
                }

                if (_model.VertexFormat == VertexFormat.Weighted)
                {
                    int b0 = vertex.BoneInfos[0].BoneIndex;
                    float w1 = vertex.BoneInfos[0].BoneWeight;
                    Matrix m1 = animationData.BoneTransforms[b0].Transform;

                    transformSum.M11 = (m1.M11 * w1);
                    transformSum.M12 = (m1.M12 * w1);
                    transformSum.M13 = (m1.M13 * w1);
                    transformSum.M21 = (m1.M21 * w1);
                    transformSum.M22 = (m1.M22 * w1);
                    transformSum.M23 = (m1.M23 * w1);
                    transformSum.M31 = (m1.M31 * w1);
                    transformSum.M32 = (m1.M32 * w1);
                    transformSum.M33 = (m1.M33 * w1);
                    transformSum.M41 = (m1.M41 * w1);
                    transformSum.M42 = (m1.M42 * w1);
                    transformSum.M43 = (m1.M43 * w1);
                }
            }
            return transformSum;
        }

            
        
    }
}
