using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.Util
{
    class AnimMeta_temp
    {

        void start2()
        {

            //_scene3d = scene3d;
            //RootViewModel = rootViewModel;
            //
            //List<PackFile> loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
            //
            //_resourceLibary = new ResourceLibary(loadedContent);
            //
            //try
            //{
            //
            //
            //    var data = File.ReadAllBytes(@"C:\Users\ole_k\Desktop\ModelDecoding\ModelsWithMaterials\foobar.rigid_model_");
            //    ByteChunk chunk = new ByteChunk(data);
            //    var model3d = RigidModel.Create(chunk, out string errorMessage);
            //}
            //catch (Exception e)
            //{
            //
            //
            //}

            // 
            // 
            // 
            //  Dictionary<string, string> errorList = new Dictionary<string, string>();
            //  List<string> completedList = new List<string>();
            //  var files = PackFileLoadHelper.GetAllWithExtention(loadedContent, "rigid_model_v2");
            // 
            // using (FileStream output = new FileStream(@"c:\temp\allModelDataHeaderInfo_3.csv", FileMode.OpenOrCreate))
            // {
            //     CreateHeader(output);
            // 
            //     int counter = 0;
            //     foreach (var file in files)
            //     {
            //         var result = Export(file, output);
            //         if (result.Item1 == false)
            //             errorList.Add(file.FullPath, result.Item2);
            //         else
            //             completedList.Add(file.FullPath);
            //     
            //         counter++;
            // 
            //     }
            // }
            //   
        }

        void Start()
        {
            /*var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\animation_tables\animation_tables.animpack");

    var res = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "meta").Where(x => x.Name.Contains("anm.meta")).ToList();



    try
    {
        AnmMetaParser parser = new AnmMetaParser();
        parser.ParesFiles(res);
    }
    catch (Exception e)
    {

    }


    try
    {var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\animation_tables\animation_tables.animpack");
        AnimPackLoader loader = new AnimPackLoader();
        loader.Load(new ByteChunk(file.Data));
    }
    catch (Exception e)
    {

    }*/
        }

        #region Export, clean later
        void WriteSeperator(FileStream fileStream)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(";");
            fileStream.Write(info, 0, info.Length);
        }

        void WriteString(FileStream fileStream, string str, bool seperator = true)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(str);
            fileStream.Write(info, 0, info.Length);
            if (seperator)
                WriteSeperator(fileStream);
        }

        void WriteNewLine(FileStream fileStream)
        {
            byte[] info = new UTF8Encoding(true).GetBytes("\n");
            fileStream.Write(info, 0, info.Length);
        }

        void Writebool(FileStream fileStream, bool value)
        {
            var str = value ? "1" : "0";
            byte[] info = new UTF8Encoding(true).GetBytes(str);
            fileStream.Write(info, 0, info.Length);
            WriteSeperator(fileStream);
        }

        void WriteArray(FileStream fileStream, byte[] array)
        {
            WriteString(fileStream, "\"", false);
            fileStream.Write(array, 0, array.Length);
            WriteString(fileStream, "\"", true);
        }



        (bool, string) Export(PackedFile file, FileStream stream)
        {
            try
            {
                ByteChunk chunk = new ByteChunk(file.Data);
                var model3d = Rmv2RigidModel.Create(chunk, out string errorMessage);
                if (model3d == null)
                    return (false, errorMessage);

                foreach (var lod in model3d.LodHeaders)
                {
                    int modelId = 0;
                    foreach (var model in lod.LodModels)
                    {
                        Serialize(stream, model, file.FullPath, modelId, (int)lod.LodLevel);
                        modelId++;
                    }
                }


                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }

        bool CheckIfAllZero(byte[] array, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (array[i] != 0)
                    return false;
            }
            return true;
        }

        void CreateHeader(FileStream fileStream)
        {
            WriteString(fileStream, "Path");
            WriteString(fileStream, "LodId");
            WriteString(fileStream, "GroupId");
            WriteString(fileStream, "ModelName");
            WriteString(fileStream, "ModelSize");
            WriteString(fileStream, "MaterialId");

            WriteString(fileStream, "VertexCount");
            WriteString(fileStream, "IndexCount");

            WriteString(fileStream, "ShaderName");
            WriteString(fileStream, "ShaderProperties");

            WriteString(fileStream, "VertexFormat");

            WriteString(fileStream, "Unknown2");
            WriteString(fileStream, "TransformationIdentity");
            WriteString(fileStream, "Transformation");
            WriteString(fileStream, "AnimationFrameForDestructableBodies");


            WriteString(fileStream, "MaterialCount");
            WriteString(fileStream, "BoneCount");

            WriteString(fileStream, "AlphaValue");
            WriteString(fileStream, "AlphaMode");

            WriteString(fileStream, "Flag_alwaysNegativeOne");
            WriteString(fileStream, "Flag_alwaysOne");

            WriteNewLine(fileStream);
        }

        void Serialize(FileStream fileStream, Rmv2LodModel model, string fillPath, int groupId, int lodLvl)
        {
            WriteString(fileStream, fillPath);
            WriteString(fileStream, lodLvl.ToString());
            WriteString(fileStream, groupId.ToString());
            WriteString(fileStream, model.ModelName);
            WriteString(fileStream, model.ModelSize.ToString());
            WriteString(fileStream, model.MaterialId.ToString());

            WriteString(fileStream, model.VertexCount.ToString());
            WriteString(fileStream, model.FaceCount.ToString());

            WriteString(fileStream, model.ShaderName.ToString());
            var shaderPrmStr = string.Join("\t", model.Unknown_Shaderarameters.Select(x => (int)x));
            WriteString(fileStream, shaderPrmStr);

            WriteString(fileStream, model.VertexFormat.ToString());

            WriteString(fileStream, model.Unknown2.ToString());
            WriteString(fileStream, model.Transformation.IsIdentity().ToString());
            WriteString(fileStream, model.Transformation.GetAsDebugStr());
            WriteString(fileStream, model.AnimationFrameForDestructableBodies.ToString());

            WriteString(fileStream, model.TextureCount.ToString());
            WriteString(fileStream, model.AttachmentPointCount.ToString());

            WriteString(fileStream, model.AlphaKeyValue.ToString());
            WriteString(fileStream, model.AlphaMode.ToString());

            WriteString(fileStream, model.Flag_alwaysNegativeOne.ToString());
            WriteString(fileStream, model.Flag_alwaysOne.ToString());

            WriteNewLine(fileStream);
        }
        #endregion
    }

    public abstract class AnimMetaObject
    {
        public string TagName { get; set; } = "unknown";
        public override string ToString()
        {
            return TagName;
        }
    }

    public class DockEquipmentHand : AnimMetaObject
    {
        public static AnimMetaObject Parse(ByteChunk data, string tagName)
        {
            data.Index += 30;
            return new DockEquipmentHand()
            {
                TagName = tagName
            };
        }

        public override string ToString()
        {
            return "";
        }
    }


    public class HandPose : AnimMetaObject
    {

        public int HeaderVersion { get; set; }
        public float AnimationStart { get; set; }
        public float AnimationEnd { get; set; }
        public short HandPoseId_If_No_Timing { get; set; }
        public int Unknown0 { get; set; }
        public short HandPoseId_If_Timing { get; set; }
        public short Unknown1 { get; set; }
        public float Weight { get; set; }

        public static AnimMetaObject Parse(ByteChunk data, string tagName)
        {
            var headerVersion = data.ReadInt32();
            if (headerVersion == 10)
            {
                return new HandPose()
                {
                    TagName = tagName,
                    HeaderVersion = headerVersion,
                    AnimationStart = data.ReadSingle(),
                    AnimationEnd = data.ReadSingle(),
                    HandPoseId_If_No_Timing = data.ReadShort(),
                    Unknown0 = data.ReadInt32(),
                    HandPoseId_If_Timing = data.ReadShort(),
                    Unknown1 = data.ReadShort(),
                    Weight = data.ReadSingle(),
                };
            }
            return new HandPose()
            {
                TagName = tagName + "_unkown",
                HeaderVersion = headerVersion,
            };
        }

        public override string ToString()
        {
            return "";
        }
    }



    class AnmMetaParser
    {
        public delegate AnimMetaObject ParseAnimMeta(ByteChunk data, string tagName);

        public Dictionary<string, List<AnimMetaObject>> MetaList = new Dictionary<string, List<AnimMetaObject>>();
        public Dictionary<string, ParseAnimMeta> ParserList = new Dictionary<string, ParseAnimMeta>();
        public List<string> UnkownTags = new List<string>();

        public void ParesFiles(List<PackedFile> files)
        {
            //ParserList.Add("DOCK_EQPT_RHAND", DockEquipmentHand.Parse);
            //ParserList.Add("DOCK_EQPT_LHAND", DockEquipmentHand.Parse);
            ParserList.Add("LHAND_POSE", HandPose.Parse);
            ParserList.Add("RHAND_POSE", HandPose.Parse);


            var index = 0;
            var filedFiles = 0;
            foreach (var file in files)
            {
                index++;
                try
                {
                    ParseFile(file);
                }
                catch (Exception e)
                {
                    filedFiles++;
                }

            }


            var data = MetaList.SelectMany(x => x.Value.Select(item => x.Value.Where(itemvalue => (itemvalue as HandPose).Unknown0 != 0)).ToList()).ToList();

            var distinctUnknowns = UnkownTags.Distinct().ToList();
        }



        void ParseFile(PackedFile file)
        {
            //if (file.FullPath.Contains("ce1_2hg_attack_01.anm.meta"))
            //    return;

            var bytes = file.Data;
            string fileContent = System.Text.Encoding.ASCII.GetString(bytes);

            var data = new ByteChunk(bytes);
            var anmMetaHeader = new AnmMeta()
            {
                Version = data.ReadInt32(),
                ItemCount = data.ReadInt32(),
            };


            foreach (var type in ParserList)
            {
                var tags = GetAllOfType(type.Key, fileContent);
                foreach (var tagStartIndex in tags)
                {
                    data.Index = tagStartIndex;
                    var tagName = data.ReadString();
                    var metaObject = ParserList[tagName](data, tagName);
                    if (MetaList.ContainsKey(file.FullPath) == false)
                        MetaList.Add(file.FullPath, new List<AnimMetaObject>());
                    MetaList[file.FullPath].Add(metaObject);
                }
            }
        }


        List<int> GetAllOfType(string tagName, string fileContent)
        {
            List<int> output = new List<int>();
            var length = tagName.Length;
            var currentIndex = fileContent.IndexOf(tagName, StringComparison.InvariantCulture);
            while (currentIndex != -1)
            {
                output.Add(currentIndex - 2);
                currentIndex = fileContent.IndexOf(tagName, currentIndex + length, StringComparison.InvariantCulture);
            }
            return output;
        }

        public class AnmMeta
        {
            public int Version { get; set; }
            public int ItemCount { get; set; }
        }

    }
}
