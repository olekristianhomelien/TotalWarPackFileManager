using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;

namespace Viewer.GraphicModels
{
    public interface IRenderableContent : IDisposable
    {
        void Render(GraphicsDevice device);
        Vector3 Pivot { get; set; }
        Matrix ModelMatrix { get; set; }
    }


    public struct VertexPositionNormalTextureCustom : IVertexType
    {
        public Vector4 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 BiNormal;
       public Vector4 BlendWeights;
          public Vector4 BlendIndices;

        public readonly static VertexDeclaration VertexDeclaration
            = new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                    new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                    new VertexElement(48, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                    new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
                    new VertexElement(76, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0)
                );

        public VertexPositionNormalTextureCustom(Vector3 pos, Vector3 normal, Vector2 tex, Vector3 tangent = new Vector3(), Vector3 biNormal = new Vector3())
        {
            Position = new Vector4(pos, 1);
            Normal = normal;
            TextureCoordinate = tex;
            Tangent = tangent;
            BiNormal = biNormal;
            BlendWeights = Vector4.One;
             BlendIndices = Vector4.Zero; //; new short[4] { 0, 0, 0, 0 };
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    public class MeshModel : IRenderableContent
    {
        public VertexDeclaration _vertexDeclaration;
        public VertexBuffer _vertexBuffer;
        public AnimationPlayer _animationPlayer;
        public IndexBuffer _indexBuffer;
        public Vector3 Pivot { get; set; } = Vector3.Zero;
        public Matrix ModelMatrix { get; set; } = Matrix.Identity;

        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, VertexPositionNormalTextureCustom[] vertexMesh, ushort[] indices)
        {
            _animationPlayer = animationPlayer;
            _vertexDeclaration = VertexPositionNormalTextureCustom.VertexDeclaration;
            
            _indexBuffer = new IndexBuffer(device, typeof(short), indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            _vertexBuffer = new VertexBuffer(device, _vertexDeclaration, vertexMesh.Length, BufferUsage.None);
            _vertexBuffer.SetData(vertexMesh);
        }

        public virtual void Render(GraphicsDevice device)
        {
            device.Indices = _indexBuffer;
            device.SetVertexBuffer(_vertexBuffer);
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indexBuffer.IndexCount);
        }

        public void Dispose()
        {
            if (_indexBuffer != null)
                _indexBuffer.Dispose();
            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();
        }

    }
}
