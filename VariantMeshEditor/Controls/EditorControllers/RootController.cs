using CommonDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.Controls.EditorControllers
{
    class RootController
    {
        RootElement _rootElement;
        RootEditorView _viewModel;
        ResourceLibary _resourceLibary;
        Scene3d _virtualWorld;

        public RootController(RootEditorView viewModel, RootElement rootElement, ResourceLibary resourceLibary, Scene3d virtualWorld)
        {
            _viewModel = viewModel;
            _rootElement = rootElement;
            _resourceLibary = resourceLibary;
            _virtualWorld = virtualWorld;
            CreateMeshList();
            _viewModel.AddButton.Click += AddButton_Click;
        }


        void CreateMeshList()
        {
           //_viewModel.MeshStackPanel.Children.Clear();
           //foreach (var child in _referenceElements)
           //{
           //    if (child.Type == FileSceneElementEnum.VariantMesh ||
           //        child.Type == FileSceneElementEnum.WsModel ||
           //        child.Type == FileSceneElementEnum.RigidModel)
           //    {
           //        var meshReference = new BrowsableItemView();
           //        meshReference.LabelName.Content = "Mesh:";
           //        meshReference.PathTextBox.Text = child.FullPath;
           //        meshReference.Tag = child;
           //
           //        //meshReference.RemoveButton.Click += RemoveButton_Click;
           //        meshReference.RemoveButton.Tag = child;
           //
           //        _viewModel.MeshStackPanel.Children.Add(meshReference);
           //    }
           // }
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            using (LoadedPackFileBrowser loadedPackFileBrowser = new LoadedPackFileBrowser(_resourceLibary.PackfileContent.First()))
            {
                var res = loadedPackFileBrowser.ShowDialog();
            }
      
           ////def_armoured_cold_one.variantmeshdefinition
           ////brt_pegasus.variantmeshdefinition
           //SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
           //var element = sceneLoader.Load("variantmeshes\\variantmeshdefinitions\\brt_royal_pegasus.variantmeshdefinition", new RootElement());
           //element.CreateContent(_virtualWorld, _resourceLibary);
           //
           //var mesh = element.Children.First();
           //mesh.Parent = null;
           //_rootElement.AddChild(mesh);
           //
           //CreateMeshList();
        }

    }
}
