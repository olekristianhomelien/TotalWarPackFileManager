using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.AnimationPack
{
    public class AnimationFragmentCollection
    {
        public class StringArrayTable
        {
            public List<string> Data { get; set; }
            public StringArrayTable(ByteChunk data)
            {
                var count = data.ReadInt32();
                Data = new List<string>(count);
                for (int i = 0; i < count; i++)
                    Data.Add(data.ReadString());
            }
        }

        public string FileName { get; set; }
        public string[] Skeletons { get; set; }
        public int MinSlotId { get; set; }
        public int MaxSlotId { get; set; }
        public List<AnimationFragmentItem> AnimationFragments { get; set; } = new List<AnimationFragmentItem>();
        public AnimationFragmentCollection(string fileName, ByteChunk data)
        {
            FileName = fileName;
            Skeletons = (new StringArrayTable(data)).Data.ToArray();
            MinSlotId = data.ReadInt32();
            MaxSlotId = data.ReadInt32();
            var numFragItems = data.ReadInt32();
            for (int i = 0; i < numFragItems; i++)
                AnimationFragments.Add(new AnimationFragmentItem(data));
        }
    }
}
