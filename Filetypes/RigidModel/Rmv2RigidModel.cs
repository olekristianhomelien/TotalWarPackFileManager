using Common;
using Filetypes.ByteParsing;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Filetypes.RigidModel
{
    public class Rmv2RigidModel
    {
        public string FileType { get; set; }
        public uint Version { get; set; }
        public uint LodCount { get; set; }
        public string BaseSkeleton { get; set; }
        public List<LodHeader> LodHeaders = new List<LodHeader>();
       

        static bool Validate(ByteChunk chunk)
        {
            if (chunk.BytesLeft != 0)
                throw new Exception("Data left after parsing model");
            return true;
        }

        public static Rmv2RigidModel Create(PackedFile file)
        {
            ILogger logger = Logging.Create<Rmv2RigidModel>();
            ByteChunk chunk = new ByteChunk(file.Data);

            logger.Here().Information($"Loading Rmv2RigidModel: {file}");
            if (chunk.BytesLeft == 0)
                throw new Exception("Trying to load Rmv2RigidModel with no data, chunk size = 0");

            Rmv2RigidModel model = new Rmv2RigidModel
            {
                FileType = chunk.ReadFixedLength(4),
                Version = chunk.ReadUInt32(),
                LodCount = chunk.ReadUInt32(),
                BaseSkeleton = Util.SanatizeFixedString(chunk.ReadFixedLength(128))
            };

            if (model.FileType != "RMV2")
                throw new Exception($"Unsupported model format. Not Rmv2 - {model.FileType }");

            for (int i = 0; i < model.LodCount; i++)
                model.LodHeaders.Add(LodHeader.Create(chunk, model.Version));

            for (int i = 0; i < model.LodCount; i++)
                for(int j = 0; j < model.LodHeaders[i].MeshCount; j++)
                    model.LodHeaders[i].LodModels.Add(Rmv2LodModel.Create(chunk));
           
            Validate(chunk);

            logger.Here().Information("Loading done");
            return model;
        }

    }
}
