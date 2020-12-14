using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Viewer.GraphicModels
{
    public class LineBox : LineModel
    {
        public LineBox()
        {
            var offset = new Vector3(-0.5f, -0.5f, -0.5f);
            var a0 = new Vector3(0, 0, 0) + offset;
            var b0 = new Vector3(1, 0, 0) + offset;
            var c0 = new Vector3(1, 1, 0) + offset;
            var d0 = new Vector3(0, 1, 0) + offset;

            var a1 = new Vector3(0, 0, 1) + offset;
            var b1 = new Vector3(1, 0, 1) + offset;
            var c1 = new Vector3(1, 1, 1) + offset;
            var d1 = new Vector3(0, 1, 1) + offset;

            var list = new List<(Vector3, Vector3)>();
            list.Add((a0, b0));
            list.Add((c0, d0));
            list.Add((b0, c0));
            list.Add((a0, d0));

            list.Add((a1, b1));
            list.Add((c1, d1));
            list.Add((b1, c1));
            list.Add((a1, d1));

            list.Add((a0, a1));
            list.Add((b0, b1));
            list.Add((c0, c1));
            list.Add((d0, d1));

            CreateLineList(list);
        }
    }
}
