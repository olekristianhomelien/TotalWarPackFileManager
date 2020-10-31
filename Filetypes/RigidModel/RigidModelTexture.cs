using Common;
using Filetypes.ByteParsing;

namespace Filetypes.RigidModel
{

    public enum TexureType
    {
        Diffuse = 0,
        Normal = 1,
        Mask = 3,
        Ambient_occlusion = 5,
        Tiling_dirt_uv2 = 7,
        Skin_mask = 10,
        Specular = 11,
        Gloss = 12,
        Decal_dirtmap = 13,
        Decal_dirtmask = 14,
        Decal_mask = 15,
        Diffuse_damage = 17
    }

    public class RigidModelTexture
    {
        public string Name { get; set; }
        public TexureType Type { get { return (TexureType)TypeRaw; } }
        public int TypeRaw { get; set; }
        public static RigidModelTexture Create(ByteChunk chunk)
        {
            return new RigidModelTexture()
            {
                TypeRaw = chunk.ReadInt32(),
                Name = Util.SanatizeFixedString(chunk.ReadFixedLength(256)),
            };
        }

        public override string ToString()
        {
            return Type + " " + Name;
        }
    }
}
