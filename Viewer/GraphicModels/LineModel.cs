using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Viewer.GraphicModels
{
    public class LineModel : IRenderableContent
    {
        VertexPosition[] _originalVertecies;
        public Vector3 Pivot { get; set; } = Vector3.Zero;
        public Matrix ModelMatrix { get; set; } = Matrix.Identity;

        public void CreateLineList(List<(Vector3, Vector3)> lines)
        {
            _originalVertecies = new VertexPosition[lines.Count * 2];
            for (int i = 0; i < lines.Count; i++)
            {
                _originalVertecies[i * 2] = new VertexPosition(lines[i].Item1);
                _originalVertecies[i * 2+1] = new VertexPosition(lines[i].Item2);
            }
        }

        public virtual void Render( GraphicsDevice device)
        {
            device.DrawUserPrimitives(PrimitiveType.LineList, _originalVertecies, 0, _originalVertecies.Count() / 2);
        }

        public void Dispose()
        {

        }
    }
}
