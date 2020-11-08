
using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using Viewer.Scene;
using WpfTest.Scenes;
using Game = Common.Game;
using Panel = System.Windows.Controls.Panel;

namespace VariantMeshEditor.Controls
{
    class EditorMainController
    {
        Scene3d _scene3d;
        ResourceLibary _resourceLibary;
        string _modelToLoad;

        public BaseViewModel RootViewModel { get; set; }

        public EditorMainController(Scene3d scene3d, BaseViewModel rootViewModel)
        {
            _scene3d = scene3d;
            RootViewModel = rootViewModel;

            List<PackFile> loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);

            _resourceLibary = new ResourceLibary(loadedContent);
            _scene3d.LoadScene += Create3dWorld;
        }

        public void LoadModel(string path)
        {
            _modelToLoad = path;
        }

        void Create3dWorld(GraphicsDevice device)
        {
            _scene3d.SetResourceLibary(_resourceLibary);
            SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            var rootElement = sceneLoader.Load(_modelToLoad, new RootElement());
            rootElement.CreateContent(_scene3d, _resourceLibary);

            _scene3d.SceneGraphRootNode = rootElement;

            RootViewModel.SceneGraph.SceneGraphRootNodes.Add(rootElement);
        }
    }
}
