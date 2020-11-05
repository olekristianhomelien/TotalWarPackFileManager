using Common;
using Filetypes.ByteParsing;
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
       

        static bool Validate(ByteChunk chunk, out string errorMessage)
        {
            if (chunk.BytesLeft != 0)
                throw new Exception("Data left!");
            errorMessage = "";
            return true;
        }

        public static Rmv2RigidModel Create(ByteChunk chunk, out string errorMessage)
        {
            Rmv2RigidModel model = new Rmv2RigidModel
            {
                FileType = chunk.ReadFixedLength(4),
                Version = chunk.ReadUInt32(),
                LodCount = chunk.ReadUInt32(),
                BaseSkeleton = Util.SanatizeFixedString(chunk.ReadFixedLength(128))
            };

            if (model.FileType != "RMV2")
            {
                errorMessage = "Unsupported model format. Not Rmv2";
                return null;
            }

            for (int i = 0; i < model.LodCount; i++)
                model.LodHeaders.Add(LodHeader.Create(chunk, model.Version));

            for (int i = 0; i < model.LodCount; i++)
                for(int j = 0; j < model.LodHeaders[i].MeshCount; j++)
                    model.LodHeaders[i].LodModels.Add(LodModel.Create(chunk));
           
            Validate(chunk, out errorMessage);
           
            return model;
        }

    }
}
