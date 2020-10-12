
using Common;
using Filetypes.ByteParsing;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using Viewer.Scene;
using WpfTest.Scenes;
using Game = Common.Game;

namespace VariantMeshEditor.Controls
{
    class EditorMainController
    {
        FileSceneElement _rootElement;
        SceneTreeViewController _treeViewController;
        Scene3d _scene3d;
        Panel _editorPanel;
        ResourceLibary _resourceLibary;
        string _modelToLoad;

        public EditorMainController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel editorPanel)
        {
            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _editorPanel = editorPanel;

            List<PackFile> loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);

            _resourceLibary = new ResourceLibary(loadedContent);

            _treeViewController.SceneElementSelectedEvent += _treeViewController_SceneElementSelectedEvent;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvent;
            _scene3d.LoadScene += Create3dWorld;
        }

        public void LoadModel(string path)
        {
            _modelToLoad = path;
        }

        private void _treeViewController_VisabilityChangedEvent(FileSceneElement element, bool isVisible)
        {
            element.Visible = isVisible;
        }

        private void _treeViewController_SceneElementSelectedEvent(FileSceneElement element)
        {
            _editorPanel.Children.Clear();
            if (element.EditorViewModel != null)
                _editorPanel.Children.Add(element.EditorViewModel);
        }

        void Create3dWorld(GraphicsDevice device)
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
            {
                AnimPackLoader loader = new AnimPackLoader();
                loader.Load(new ByteChunk(file.Data));
            }
            catch (Exception e)
            {

            }*/



            _scene3d.SetResourceLibary(_resourceLibary);
            SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            _rootElement = sceneLoader.Load(_modelToLoad, new RootElement());
            _rootElement.CreateContent(_scene3d, _resourceLibary);

            _scene3d.SceneGraphRootNode = _rootElement;
            _treeViewController.SetRootItem(_rootElement);
            SceneElementHelper.SetInitialVisability(_rootElement, true);
        }



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
