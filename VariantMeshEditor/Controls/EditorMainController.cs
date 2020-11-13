
using Common;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using Viewer.Scene;
using WpfTest.Scenes;
using Game = Common.Game;

namespace VariantMeshEditor.Controls
{
    class EditorMainController
    {
        Scene3d _scene3d;
        ResourceLibary _resourceLibary;
        PackedFile _modelToLoad;
        bool _is3dWorldCreated = false;

        public BaseViewModel RootViewModel { get; set; }

        public EditorMainController(Scene3d scene3d, BaseViewModel rootViewModel, List<PackFile> packFile)
        {
            _scene3d = scene3d;
            RootViewModel = rootViewModel;

            _resourceLibary = new ResourceLibary(packFile);
            _scene3d.LoadScene += Create3dWorld;



            //DumpRmv2Files dumper = new DumpRmv2Files();
            //dumper.Dump(_resourceLibary, @"C:\temp\DataDump\");
        }

        public void LoadModel(PackedFile model)
        {
            _modelToLoad = model;
            if(_is3dWorldCreated)
                LoadModelAfterWorldCreated();
        }

        void Create3dWorld(GraphicsDevice device)
        {
            _scene3d.SetResourceLibary(_resourceLibary);
            _is3dWorldCreated = true;
            LoadModelAfterWorldCreated();
        }

        void LoadModelAfterWorldCreated()
        {
            var existingNode = RootViewModel.SceneGraph.SceneGraphRootNodes.FirstOrDefault();
            if (existingNode != null)
            {
                existingNode.Dispose();
                RootViewModel.SceneGraph.SceneGraphRootNodes.Clear();
                _scene3d.SceneGraphRootNode = null;
            }

           SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            var rootElement = sceneLoader.Load(_modelToLoad, new RootElement());
            rootElement.CreateContent(_scene3d, _resourceLibary);

            _scene3d.SceneGraphRootNode = rootElement;

            RootViewModel.SceneGraph.SceneGraphRootNodes.Add(rootElement);
        }
    }
}
