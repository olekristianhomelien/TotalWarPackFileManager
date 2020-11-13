using Filetypes.ByteParsing;
using System.Linq;

namespace Filetypes.RigidModel
{
    public class RigidModelAttachmentPoint
    {
        public string Name { get; set; }
        public FileMatrix3x4 Transform { get; set; } = new FileMatrix3x4();
        public int BoneIndex { get; set; }

        public static RigidModelAttachmentPoint Create(ByteChunk chunk)
        {
            var output = new RigidModelAttachmentPoint();
            var nameData = chunk.ReadFixedLength(32);
            output.Name = Util.SanatizeFixedString(nameData);

            for (int row = 0; row < 3; row++)
            {
                output.Transform.Matrix[row].X = chunk.ReadSingle();
                output.Transform.Matrix[row].Y = chunk.ReadSingle();
                output.Transform.Matrix[row].Z = chunk.ReadSingle();
                output.Transform.Matrix[row].W = chunk.ReadSingle();
            }

            output.BoneIndex = chunk.ReadInt32();

            return output;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
