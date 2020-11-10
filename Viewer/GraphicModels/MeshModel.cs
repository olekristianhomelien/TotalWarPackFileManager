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
    }


    public class MeshModel : IRenderableContent
    {
        public VertexDeclaration _vertexDeclaration;
        public VertexBuffer _vertexBuffer;
        public AnimationPlayer _animationPlayer;
        public IndexBuffer _indexBuffer;
        public Vector3 Pivot { get; set; } = Vector3.Zero;
 
        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, VertexPositionNormalTexture[] vertexMesh, ushort[] indices)
        {
            _animationPlayer = animationPlayer;
            _vertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

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
            if(_vertexDeclaration != null)
                _vertexDeclaration.Dispose();
            if (_indexBuffer != null)
                _indexBuffer.Dispose();
            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();
        }

    }
}
