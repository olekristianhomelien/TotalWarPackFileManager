using Filetypes.ByteParsing;

namespace Filetypes.AnimationPack
{
    public class AnimationFragmentItem
    {
        public int Id { get; set; }
        public int Slot { get; set; }
        public string AnimationFile { get; set; }
        public string MetaDataFile { get; set; }
        public string SoundMetaDataFile { get; set; }
        public string Skeleton { get; set; }
        public float Blend { get; set; }
        public float Wight { get; set; }
        public int Unknown0 { get; set; }
        public int Unknown1 { get; set; }
        public string Unknown3 { get; set; }
        public bool Unknown4 { get; set; }

        public AnimationFragmentItem(ByteChunk data)
        {
            Id = data.ReadInt32();
            Slot = data.ReadInt32();
            AnimationFile = data.ReadString();
            MetaDataFile = data.ReadString();
            SoundMetaDataFile = data.ReadString();
            Skeleton = data.ReadString();
            Blend = data.ReadSingle();
            Wight = data.ReadSingle();
            Unknown0 = data.ReadInt32();
            Unknown1 = data.ReadInt32();
            Unknown3 = data.ReadString();
            Unknown4 = data.ReadBool();
        }
    }
}
