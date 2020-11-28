using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.GraphicModels
{
    class CubeModel : MeshModel
    {
        public void Create(GraphicsDevice device)
        {
            var mesh = CreateBuffer();
            this.Create(null, device, mesh, new ushort[] { 1,2,3});
        }

        VertexPositionNormalTextureCustom[] CreateBuffer()
        {
            var cubeVertices = new VertexPositionNormalTextureCustom[36];

            Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, 1.0f);
            Vector3 bottomLeftFront = new Vector3(-1.0f, -1.0f, 1.0f);
            Vector3 topRightFront = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 bottomRightFront = new Vector3(1.0f, -1.0f, 1.0f);
            Vector3 topLeftBack = new Vector3(-1.0f, 1.0f, -1.0f);
            Vector3 topRightBack = new Vector3(1.0f, 1.0f, -1.0f);
            Vector3 bottomLeftBack = new Vector3(-1.0f, -1.0f, -1.0f);
            Vector3 bottomRightBack = new Vector3(1.0f, -1.0f, -1.0f);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

            // Front face.
            cubeVertices[0] = new VertexPositionNormalTextureCustom(topLeftFront, frontNormal, textureTopLeft);
            cubeVertices[1] = new VertexPositionNormalTextureCustom(bottomLeftFront, frontNormal, textureBottomLeft);
            cubeVertices[2] = new VertexPositionNormalTextureCustom(topRightFront, frontNormal, textureTopRight);
            cubeVertices[3] = new VertexPositionNormalTextureCustom(bottomLeftFront, frontNormal, textureBottomLeft);
            cubeVertices[4] = new VertexPositionNormalTextureCustom(bottomRightFront, frontNormal, textureBottomRight);
            cubeVertices[5] = new VertexPositionNormalTextureCustom(topRightFront, frontNormal, textureTopRight);

            // Back face.
            cubeVertices[6] = new VertexPositionNormalTextureCustom(topLeftBack, backNormal, textureTopRight);
            cubeVertices[7] = new VertexPositionNormalTextureCustom(topRightBack, backNormal, textureTopLeft);
            cubeVertices[8] = new VertexPositionNormalTextureCustom(bottomLeftBack, backNormal, textureBottomRight);
            cubeVertices[9] = new VertexPositionNormalTextureCustom(bottomLeftBack, backNormal, textureBottomRight);
            cubeVertices[10] = new VertexPositionNormalTextureCustom(topRightBack, backNormal, textureTopLeft);
            cubeVertices[11] = new VertexPositionNormalTextureCustom(bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            cubeVertices[12] = new VertexPositionNormalTextureCustom(topLeftFront, topNormal, textureBottomLeft);
            cubeVertices[13] = new VertexPositionNormalTextureCustom(topRightBack, topNormal, textureTopRight);
            cubeVertices[14] = new VertexPositionNormalTextureCustom(topLeftBack, topNormal, textureTopLeft);
            cubeVertices[15] = new VertexPositionNormalTextureCustom(topLeftFront, topNormal, textureBottomLeft);
            cubeVertices[16] = new VertexPositionNormalTextureCustom(topRightFront, topNormal, textureBottomRight);
            cubeVertices[17] = new VertexPositionNormalTextureCustom(topRightBack, topNormal, textureTopRight);

            // Bottom face.
            cubeVertices[18] = new VertexPositionNormalTextureCustom(bottomLeftFront, bottomNormal, textureTopLeft);
            cubeVertices[19] = new VertexPositionNormalTextureCustom(bottomLeftBack, bottomNormal, textureBottomLeft);
            cubeVertices[20] = new VertexPositionNormalTextureCustom(bottomRightBack, bottomNormal, textureBottomRight);
            cubeVertices[21] = new VertexPositionNormalTextureCustom(bottomLeftFront, bottomNormal, textureTopLeft);
            cubeVertices[22] = new VertexPositionNormalTextureCustom(bottomRightBack, bottomNormal, textureBottomRight);
            cubeVertices[23] = new VertexPositionNormalTextureCustom(bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            cubeVertices[24] = new VertexPositionNormalTextureCustom(topLeftFront, leftNormal, textureTopRight);
            cubeVertices[25] = new VertexPositionNormalTextureCustom(bottomLeftBack, leftNormal, textureBottomLeft);
            cubeVertices[26] = new VertexPositionNormalTextureCustom(bottomLeftFront, leftNormal, textureBottomRight);
            cubeVertices[27] = new VertexPositionNormalTextureCustom(topLeftBack, leftNormal, textureTopLeft);
            cubeVertices[28] = new VertexPositionNormalTextureCustom(bottomLeftBack, leftNormal, textureBottomLeft);
            cubeVertices[29] = new VertexPositionNormalTextureCustom(topLeftFront, leftNormal, textureTopRight);

            // Right face.
            cubeVertices[30] = new VertexPositionNormalTextureCustom(topRightFront, rightNormal, textureTopLeft);
            cubeVertices[31] = new VertexPositionNormalTextureCustom(bottomRightFront, rightNormal, textureBottomLeft);
            cubeVertices[32] = new VertexPositionNormalTextureCustom(bottomRightBack, rightNormal, textureBottomRight);
            cubeVertices[33] = new VertexPositionNormalTextureCustom(topRightBack, rightNormal, textureTopRight);
            cubeVertices[34] = new VertexPositionNormalTextureCustom(topRightFront, rightNormal, textureTopLeft);
            cubeVertices[35] = new VertexPositionNormalTextureCustom(bottomRightBack, rightNormal, textureBottomRight);


            return cubeVertices;
        }
    }
}
