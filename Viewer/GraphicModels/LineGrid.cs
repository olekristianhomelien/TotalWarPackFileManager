using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Viewer.GraphicModels
{
    public class LineGrid : LineModel
    {
        public LineGrid()
        {
            int lineCount = 10;
            float spacing = 1;
            float length = 10;
            float offset = (lineCount * spacing) / 2;

            var list = new List<(Vector3, Vector3)>();
            for (int i = 0; i <= lineCount; i++)
            {
                var start = new Vector3((i * spacing) - offset, 0, -length  * 0.5f);
                var stop = new Vector3((i * spacing) - offset, 0, length * 0.5f);
                list.Add((start, stop));
            }

            for (int i = 0; i <= lineCount; i++)
            {
                var start = new Vector3(-length * 0.5f, 0 ,(i * spacing) - offset);
                var stop = new Vector3(length * 0.5f, 0, (i * spacing) - offset);
                list.Add((start, stop));
            }

            CreateLineList(list);
        }
    }
}
