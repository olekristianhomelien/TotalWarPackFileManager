using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.AnimationPack
{
    public class AnimationPackLoader
    {
        public class File
        {
            public string Name { get; set; }
            public int StartOffset { get; set; }
            public int Size { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public List<AnimationTableEntry> AnimationTableEntries { get; set; } = new List<AnimationTableEntry>();
        public List<MatchedAnimationTableEntry> MatchedAnimationTableEntries { get; set; } = new List<MatchedAnimationTableEntry>();
        public List<AnimationFragmentCollection> AnimationFragments { get; set; } = new List<AnimationFragmentCollection>();


        delegate void ProcessFileDelegate(File file, ByteChunk data);
        Dictionary<string, ProcessFileDelegate> _processMap = new Dictionary<string, ProcessFileDelegate>();

        public void Load(ByteChunk data)
        {
            data.Reset();
            _processMap["attila_generated.bin"] = ProcessMatchCombatFile;
            _processMap["animation_tables.bin"] = ProcessAnimationTableFile;
            _processMap[".frg"] = ProcessFragmentFile;

            var files = FindAllSubFiles(data);
            foreach (var file in files)
            {
                bool isProcessed = false;
                foreach (var process in _processMap)
                {
                    if (file.Name.Contains(process.Key))
                    {
                        process.Value(file, data);
                        isProcessed = true;
                        break;
                    }
                }
                if(!isProcessed)
                    throw new Exception($"Unknown file - {file.Name}");
            }
        }

        List<File> FindAllSubFiles(ByteChunk data)
        {
            var toalFileCount = data.ReadInt32();
            var fileList = new List<File>(toalFileCount);
            for (int i = 0; i < toalFileCount; i++)
            {
                var file = new File()
                {
                    Name = data.ReadString(),
                    Size = data.ReadInt32(),
                    StartOffset = data.Index
                };
                fileList.Add(file);
                data.Index += file.Size;
            }
            return fileList;
        }

        void ProcessFragmentFile(File file, ByteChunk data)
        {
            data.Index = file.StartOffset;
            AnimationFragments.Add(new AnimationFragmentCollection(file.Name, data));
        }

        void ProcessMatchCombatFile(File file, ByteChunk data)
        {
            data.Index = file.StartOffset;
            var tableVersion = data.ReadInt32();
            var rowCount = data.ReadInt32();
            MatchedAnimationTableEntries = new List<MatchedAnimationTableEntry>(rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                var entry = new MatchedAnimationTableEntry(data);
                MatchedAnimationTableEntries.Add(entry);
            }
        }

        void ProcessAnimationTableFile(File file, ByteChunk data)
        {
            data.Index = file.StartOffset;
            var tableVersion = data.ReadInt32();
            var rowCount = data.ReadInt32();
            AnimationTableEntries = new List<AnimationTableEntry>(rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                var entry = new AnimationTableEntry(data);
                AnimationTableEntries.Add(entry);
            }
        }



        



    }
}
