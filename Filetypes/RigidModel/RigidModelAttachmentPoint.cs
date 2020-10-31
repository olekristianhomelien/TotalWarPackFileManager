using Filetypes.ByteParsing;
using System.Linq;

namespace Filetypes.RigidModel
{
    public class RigidModelAttachmentPoint
    {
        public string Name { get; set; }
        public static RigidModelAttachmentPoint Create(ByteChunk chunk)
        {
            
            return new RigidModelAttachmentPoint()
            {
                Name = Util.SanatizeFixedString(chunk.ReadFixedLength(84)), // More data then just the name? 3 x matrix?
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }


   
}
