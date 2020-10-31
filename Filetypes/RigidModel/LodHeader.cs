using Filetypes.ByteParsing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Filetypes.RigidModel
{

    /*unsafe struct MyData
    {

        public fixed int Name2[20];

        public static MyData FromBytes(byte[] bytes)
        {
            GCHandle gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var data = (MyData)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(MyData));
            gcHandle.Free();
            return data;
        }
    }*/

    /*
     * FROM CA: https://discord.com/channels/373745291289034763/448884160094797834/766300715294523423
        struct rigid_lod_header 
        {
          unit32_t mesh_count;             // Number of meshes in LOD
          unit32_t total_lod_vertex_size;  // Number of vertices for all meshes in LOD
          unit32_t total_lod_index_size;   // Number of indices for all meshes in LOD
          unit32_t first_mesh_offset;      // Offset in bytes from start of file to the first instance of mesh header in LOD
          float32_t lod_distance;          // Distance until which this LOD should be displayed
          unit32_t authored_lod_index;     // Numerical indexz of this LOD as made by the artist
          unit8_t quality_level;           // The lowest graphics quality level that this mesh LOD will be active. Zero is the lowest graphics quality, meaning LODs flagged with quality level zero will be visible on graphics settings. 
        } 
     */

    public class LodHeader
    {
        public uint MeshCount { get; set; }
        public uint TotalLodVertexSize  { get; set; }
        public uint TotalLodIndexSize  { get; set; }
        public uint FirstMeshOffset { get; set; }
        public float LodCameraDistance { get; set; }
        public uint LodLevel { get; set; }
        public byte QualityLvl { get; set; }
        public byte[] Padding { get; set; }

     

        public List<LodModel> LodModels = new List<LodModel>();

        public static LodHeader Create(ByteChunk chunk, uint version)
        {
            var data = new LodHeader()
            {
                MeshCount = chunk.ReadUInt32(),
                TotalLodVertexSize  = chunk.ReadUInt32(),
                TotalLodIndexSize  = chunk.ReadUInt32(),
                FirstMeshOffset = chunk.ReadUInt32(),       
                LodCameraDistance = chunk.ReadSingle(),
            
            };

            if (version == 7)
            {
                data.LodLevel = chunk.ReadUInt32();
                data.QualityLvl = chunk.ReadByte();
                data.Padding = chunk.ReadBytes(3);
            }

            return data;
        }

    }
}
