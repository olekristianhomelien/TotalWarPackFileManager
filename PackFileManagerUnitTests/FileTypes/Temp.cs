using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackFileManagerUnitTests.FileTypes
{
    [TestClass]
    public class Temp
    {



        [TestMethod]
        public void ConvertTabele_validDefinition()
        {
            var bytes = File.ReadAllBytes(@"C:\temp\datafiles\vmp_black_coach_01.rigid_model_v2");
            ByteChunk chunk = new ByteChunk(bytes);


            Rmv2RigidModel model = Rmv2RigidModel.Create(chunk, out var error);
        }
    }
}
