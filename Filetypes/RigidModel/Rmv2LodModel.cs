using Common;
using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Filetypes.RigidModel
{


    public enum AlphaMode : Int32
    {
        Opaque = 0,
        Alpha_Test = 1,
        Alpha_Blend = -1
    };

    public enum VertexFormat : UInt32
    {
        Unknown = 99,
        Default = 0,
        Weighted = 3,
        Cinematic = 4
    };

    public class Transformation
    {
        public FileVector3 Pivot { get; set; } = new FileVector3();
        public FileMatrix3x4[] Matrices { get; set; } = new FileMatrix3x4[] { new FileMatrix3x4(), new FileMatrix3x4(), new FileMatrix3x4() };

        public string GetAsDebugStr()
        {
            var str = $"Piv:{Pivot.X},{Pivot.Y},{Pivot.Z} ";

            str += $"M00:{Matrices[0].Matrix[0].X},{Matrices[0].Matrix[0].Y},{Matrices[0].Matrix[0].Z},{Matrices[0].Matrix[0].W} ";
            str += $"M01:{Matrices[0].Matrix[1].X},{Matrices[0].Matrix[1].Y},{Matrices[0].Matrix[1].Z},{Matrices[0].Matrix[1].W} ";
            str += $"M02:{Matrices[0].Matrix[2].X},{Matrices[0].Matrix[2].Y},{Matrices[0].Matrix[2].Z},{Matrices[0].Matrix[2].W} ";

            str += $"M10:{Matrices[1].Matrix[0].X},{Matrices[1].Matrix[0].Y},{Matrices[1].Matrix[0].Z},{Matrices[1].Matrix[0].W} ";
            str += $"M11:{Matrices[1].Matrix[1].X},{Matrices[1].Matrix[1].Y},{Matrices[1].Matrix[1].Z},{Matrices[1].Matrix[1].W} ";
            str += $"M12:{Matrices[1].Matrix[2].X},{Matrices[1].Matrix[2].Y},{Matrices[1].Matrix[2].Z},{Matrices[1].Matrix[2].W} ";

            str += $"M20:{Matrices[2].Matrix[0].X},{Matrices[2].Matrix[0].Y},{Matrices[2].Matrix[0].Z},{Matrices[2].Matrix[0].W} ";
            str += $"M21:{Matrices[2].Matrix[1].X},{Matrices[2].Matrix[1].Y},{Matrices[2].Matrix[1].Z},{Matrices[2].Matrix[1].W} ";
            str += $"M22:{Matrices[2].Matrix[2].X},{Matrices[2].Matrix[2].Y},{Matrices[2].Matrix[2].Z},{Matrices[2].Matrix[2].W} ";
            return str;
        }

        public bool IsIdentityPivot()
        {
            if (!Pivot.IsAllZero())
                return false;
            return true;
        }

        public bool IsIdentityMatrices()
        {
            foreach (var matrix in Matrices)
            {
                if (!matrix.IsIdentity())
                    return false;
            }
            return true;
        }

    }

    public class FileMatrix3x4
    {
        public FileVector4[] Matrix { get; set; } = new FileVector4[3] { new FileVector4(), new FileVector4(), new FileVector4() };

        public bool IsIdentity()
        {
            var row0 = Matrix[0];
            if (row0.X == 1 && row0.Y == 0 && row0.Z == 0 && row0.W == 0)
            {
                var row1 = Matrix[1];
                if (row1.X == 0 && row1.Y == 1 && row1.Z == 0 && row1.W == 0)
                {
                    var row2 = Matrix[2];
                    if (row2.X == 0 && row2.Y == 0 && row2.Z == 1 && row2.W == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetAsDebugStr()
        {
            var str = "";
            str += $"{Matrix[0].X},{Matrix[0].Y},{Matrix[0].Z},{Matrix[0].W} ";
            str += $"{Matrix[1].X},{Matrix[1].Y},{Matrix[1].Z},{Matrix[1].W} ";
            str += $"{Matrix[2].X},{Matrix[2].Y},{Matrix[2].Z},{Matrix[2].W} ";
            return str;
        }


    }

    public class Rmv2LodModel
    {
        public GroupTypeEnum MaterialId { get; set; }

        public List<RigidModelAttachmentPoint> AttachmentPoint { get; set; } = new List<RigidModelAttachmentPoint>();
        public uint AttachmentPointCount { get; set; }
        public uint VertexCount { get; set; }
        public uint VertexOffset { get; set; }
        public uint FaceCount { get; set; }
        public uint FaceOffset { get; set; }
        public uint VertexFormatValue { get; set; }
        public int VertexSize { get; set; }
        public VertexFormat VertexFormat { get; set; } = VertexFormat.Unknown;
        public string ShaderName { get; set; }
        public byte[] Unknown_Shaderarameters { get; set; }
        public byte[] AllZero_Shaderarameters { get; set; }
        public string ModelName { get; set; }
        
        public string TextureDirectory { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public Transformation Transformation { get; set; }

        public uint TextureCount { get; set; }
        public List<RigidModelTexture> Textures { get; set; } = new List<RigidModelTexture>();

        public byte[] ZeroPadding0;
        public byte[] ZeroPadding1;
        public byte Flag_alwaysOne;
        public byte[] ZeroPadding2;

        public uint ModelSize;

        public byte Unknown2_val0;
        public byte Unknown2_val1;

        public int LinkDirectlyToBoneIndex;
        public int Flag_alwaysNegativeOne;
        public int AlphaKeyValue;

        public AlphaMode AlphaMode { get; set; }

        public Vertex[] VertexArray;
        public ushort[] IndicesBuffer;

        public static Rmv2LodModel Create(ByteChunk chunk)
        {
            var lodModel = new Rmv2LodModel();

            lodModel.MaterialId = (GroupTypeEnum)chunk.ReadUInt32();
            lodModel.ModelSize = chunk.ReadUInt32();

            lodModel.VertexOffset = chunk.ReadUInt32() ;
            lodModel.VertexCount = chunk.ReadUInt32();
            lodModel.FaceOffset = chunk.ReadUInt32();
            lodModel.FaceCount = chunk.ReadUInt32();
            lodModel.VertexSize = (int)((lodModel.FaceOffset - lodModel.VertexOffset) / lodModel.VertexCount);

            lodModel.BoundingBox = BoundingBox.Create(chunk);

            var materialInfo = chunk.CreateSub(32);
            lodModel.ShaderName = Util.SanatizeFixedString(materialInfo.ReadFixedLength(12));
            lodModel.Unknown_Shaderarameters = materialInfo.ReadBytes(10);
            lodModel.AllZero_Shaderarameters = materialInfo.ReadBytes(10);

            lodModel.VertexFormatValue = chunk.ReadUShort();
            lodModel.VertexFormat = (VertexFormat)lodModel.VertexFormatValue;
            lodModel.ModelName = Util.SanatizeFixedString(chunk.ReadFixedLength(32));   
                                                                                        
            lodModel.TextureDirectory = Util.SanatizeFixedString(chunk.ReadFixedLength(256));  
            lodModel.ZeroPadding0 = chunk.ReadBytes(256);

            lodModel.Unknown2_val0 = chunk.ReadByte();
            lodModel.Unknown2_val1 = chunk.ReadByte();
            lodModel.Transformation = LoadTransformations(chunk);
            lodModel.LinkDirectlyToBoneIndex = chunk.ReadInt32();       // Keep -1 for most stuff. Might not be the frame, but the value is only there for things that can be destroyed
            lodModel.Flag_alwaysNegativeOne = chunk.ReadInt32();
            lodModel.AttachmentPointCount = chunk.ReadUInt32();
            lodModel.TextureCount = chunk.ReadUInt32();

            // A groupd of data (140 bytes) that is all zeroes with a single one
            lodModel.ZeroPadding1 = chunk.ReadBytes(8);
            lodModel.Flag_alwaysOne = chunk.ReadByte();
            lodModel.ZeroPadding2 = chunk.ReadBytes(131);

            for (int i = 0; i < lodModel.AttachmentPointCount; i++)
                lodModel.AttachmentPoint.Add(RigidModelAttachmentPoint.Create(chunk));

            for (int i = 0; i < lodModel.TextureCount; i++)
                lodModel.Textures.Add(RigidModelTexture.Create(chunk));

            lodModel.AlphaKeyValue = chunk.ReadInt32();
            lodModel.AlphaMode = (AlphaMode)chunk.ReadInt32();

            lodModel.VertexArray = CreateVertexArray(lodModel, chunk, lodModel.VertexCount, lodModel.VertexFormatValue);
            lodModel.IndicesBuffer = CreateIndexArray(chunk, (int)lodModel.FaceCount);
     
            return lodModel;
        }

        static ushort[] CreateIndexArray(ByteChunk chunk, int indexCount)
        {
            ushort[] output = new ushort[indexCount];
            for (int i = 0; i < indexCount; i++)
                output[i] = chunk.ReadUShort();
            return output;
        }

        static Transformation LoadTransformations(ByteChunk chunk)
        {
            var output = new Transformation();
            output.Pivot.X= chunk.ReadSingle();
            output.Pivot.Y = chunk.ReadSingle();
            output.Pivot.Z = chunk.ReadSingle();

            for (int i = 0; i < 3; i++)
            {
                var matrix = output.Matrices[i];
                for (int row = 0; row < 3; row++)
                {
                    matrix.Matrix[row].X = chunk.ReadSingle();
                    matrix.Matrix[row].Y = chunk.ReadSingle();
                    matrix.Matrix[row].Z = chunk.ReadSingle();
                    matrix.Matrix[row].W = chunk.ReadSingle();
                }
            }
            
            return output;
        }

        static Vertex[] CreateDefaultVertex(ByteChunk chunk, uint count)
        {
            Vertex[] output = new Vertex[count];

            for (int i = 0; i < count; i++)
            {
                var bytes = chunk.ReadBytes(32);
                var subChucnk = new ByteChunk(bytes);
                var vertex = new Vertex();

                vertex.Position.X = subChucnk.ReadFloat16();   //0
                vertex.Position.Y = subChucnk.ReadFloat16();    //2
                vertex.Position.Z = subChucnk.ReadFloat16();    //4

                var u0 = subChucnk.ReadFloat16();       // 6
                vertex.Uv0 = subChucnk.ReadFloat16();       // 8        uv0
                vertex.Uv1 = subChucnk.ReadFloat16();       // 10
                var u3 = subChucnk.ReadFloat16();       // 12
                var u4 = subChucnk.ReadFloat16();       // 14

                var b0 = subChucnk.ReadByte();          //15
                var t0 = (b0 / 255.0f * 2.0f) - 1.0f;

                var b1 = subChucnk.ReadByte();          //16
                var t1 = (b1 / 255.0f * 2.0f) - 1.0f;

                var b2 = subChucnk.ReadByte();          //17
                var t2 = (b2 / 255.0f * 2.0f) - 1.0f;

                vertex.Normal.X = t0;
                vertex.Normal.Y = t1;
                vertex.Normal.Z = t2;


                output[i] = vertex;
            }

            return output;
        }

        static Vertex[] CreateWeighthedVertex(ByteChunk chunk, uint count)
        {
            Vertex[] output = new Vertex[count];
            for (int i = 0; i < count; i++)
            {
                var bytes = chunk.ReadBytes(28);
                var subChucnk = new ByteChunk(bytes);
                var vertex = new Vertex();

                vertex.Position.X = subChucnk.ReadFloat16();   //0
                vertex.Position.Y = subChucnk.ReadFloat16();    //2
                vertex.Position.Z = subChucnk.ReadFloat16();    //4

                var u = subChucnk.ReadByte();           // 6
                var u0 = subChucnk.ReadByte();          // 7      
                var boneIndex = subChucnk.ReadByte();   // 8
                var u1 = subChucnk.ReadByte();          // 9

                var boneWeight0 = subChucnk.ReadByte(); // 10
                vertex.BoneInfos.Add(new Vertex.BoneInfo() { BoneIndex = boneIndex, BoneWeight = boneWeight0 / 255.0f });

                var u2 = subChucnk.ReadByte();          // 11
                vertex.Normal.X = ((subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f);    //12
                vertex.Normal.Y = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //13
                vertex.Normal.Z = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //14
                var u3 = subChucnk.ReadByte();          // 15
                vertex.Uv0 = subChucnk.ReadFloat16();      // 16
                vertex.Uv1 = subChucnk.ReadFloat16();      // 18
                var u4 = subChucnk.ReadBytes(8);       // 20

                output[i] = vertex;
            }
            return output;
  
        }

        static Vertex[] CreateCinematicVertex(ByteChunk chunk, uint count)
        {
            Vertex[] output = new Vertex[count];

                for (int i = 0; i < count; i++)
                {
                    var bytes = chunk.ReadBytes(32);
                    var subChucnk = new ByteChunk(bytes);
                    var vertex = new Vertex();

                    vertex.Position.X = subChucnk.ReadFloat16();   //0
                    vertex.Position.Y = subChucnk.ReadFloat16();    //2
                    vertex.Position.Z = subChucnk.ReadFloat16();    //4

                    var ukn = subChucnk.ReadFloat16();  //6
                    var bone0 = subChucnk.ReadByte();   //8
                    var bone1 = subChucnk.ReadByte();   //9
                    var bone2 = subChucnk.ReadByte();   //10
                    var bone3 = subChucnk.ReadByte();   //11


                    var weight0 = subChucnk.ReadByte(); //12
                    var weight1 = subChucnk.ReadByte(); //13
                    var weight2 = subChucnk.ReadByte(); //14
                    var weight3 = subChucnk.ReadByte(); //15

                    vertex.Normal.X = ((subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f);    //16
                    vertex.Normal.Y = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //17
                    vertex.Normal.Z = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //18

                    var x = 255.0f;
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone0,
                        BoneWeight = (float)weight0 / x
                    });
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone1,
                        BoneWeight = (float)weight1 / x
                    });
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone2,
                        BoneWeight = (float)weight2 / x
                    });
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone3,
                        BoneWeight = (float)weight3 / x
                    });

                    var ukn1 = subChucnk.ReadByte();           // 19
                    vertex.Uv0 = subChucnk.ReadFloat16();      // 20
                    vertex.Uv1 = subChucnk.ReadFloat16();      // 22
                    output[i] = vertex;
                }
            return output;
        }

        static Vertex[] CreateVertexArray(Rmv2LodModel model, ByteChunk chunk, uint count, uint vertexType)
        {
            bool loadVertexData = true;
            if (loadVertexData)
            {
                switch (vertexType)
                {
                    case 0:
                        return CreateDefaultVertex(chunk, count);
                    case 3:
                        return CreateWeighthedVertex(chunk, count);
                    case 4:
                        return CreateCinematicVertex(chunk, count);
                    default:
                        chunk.Index += (int)(model.FaceOffset - model.VertexOffset);
                        return null;
                }
            }
            else
            {
                switch (vertexType)
                {
                    case 0:
                        chunk.Index += (int)count * 32;
                        return null;
                    case 3:
                        chunk.Index += (int)count * 28;
                        return null;
                    case 4:
                        chunk.Index += (int)count * 32;
                        return null;
                    default:
                        chunk.Index += (int)(model.FaceOffset - model.VertexOffset);
                        return null;
                }
            }


        }
    }


    public class FileVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public FileVector3(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsAllZero()
        {
            if (X == 0 && Y == 0 && Z == 0)
                return true;
            return false;
        }
    }


    public class FileVector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public FileVector4(float x = 0, float y = 0, float z = 0, float w = 0)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public bool IsAllZero()
        {
            if (X == 0 &&  Y == 0 && Z == 0 && W == 0)
                return true;
            return false;
        }
    }

    public class Vertex
    {
        public class BoneInfo
        { 
            public byte BoneIndex { get; set; }
            public float BoneWeight { get; set; }

            public override string ToString()
            {
                return $"I:{ BoneIndex} - W:{BoneWeight}";
            }
        }



        public FileVector3 Position { get; set; } = new FileVector3();
        public FileVector3 Normal { get; set; } = new FileVector3();


        public float Uv0 { get; set; }
        public float Uv1 { get; set; }

        public List<BoneInfo> BoneInfos { get; set; } = new List<BoneInfo>();
    }
}
