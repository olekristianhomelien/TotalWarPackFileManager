using Common;
using GalaSoft.MvvmLight.CommandWpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class RootElement : FileSceneElement
    {
        ILogger _logger = Logging.Create<RootElement>();


        BaseViewModel _baseViewModel;

        public override FileSceneElementEnum Type => FileSceneElementEnum.Root;

        Scene3d _virtualWorld;
        ResourceLibary _resourceLibary;

        public ICommand LoadNewModelCommand { get; set; }
        public ICommand RemoveModelCommand { get; set; }

        public RootElement(BaseViewModel baseViewModel) : base(null, "", "", "Root")
        {
            IsChecked = true;
            _baseViewModel = baseViewModel;

            LoadNewModelCommand = new RelayCommand(OnLoadNewModel);
            RemoveModelCommand = new RelayCommand<FileSceneElement>(OnRemoveModel);
        }

        public void SelectNode(FileSceneElement node)
        {
            _baseViewModel.SceneGraph.SelectedNode = node;
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
       {
            _virtualWorld = virtualWorld;
            _resourceLibary = resourceLibary;
        }

        void OnLoadNewModel()
        {
            string filePath = "variantmeshes\\variantmeshdefinitions\\brt_royal_pegasus.variantmeshdefinition";
            var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, filePath);
            LoadModel(file, _resourceLibary, _virtualWorld);
        }

        public FileSceneElement LoadModel(PackedFile path, ResourceLibary resourceLibary, Scene3d virtualWorld)
        {
            _logger.Here().Information($"Loading model - {path}");

            SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            var modelElement = sceneLoader.Load(path, null);

            // Remove empty slots
            var toDelete = new List<FileSceneElement>();
            var slots = SceneElementHelper.GetFirstChild<SlotsElement>(modelElement);
            for (int i = 0; i < slots.Children.Count(); i++)
            {
                if (slots.Children[i].Children.Count == 0)
                    toDelete.Add(slots.Children[i]);
            }

            foreach (var itemToDelete in toDelete)
                slots.Children.Remove(itemToDelete);

            modelElement.CreateContent(_virtualWorld, _resourceLibary);
            modelElement.IsChecked = true;

            Children.Add(modelElement);
            modelElement.Parent = this;

            _logger.Here().Information($"Loading Done - {path}");

            return modelElement;
        }

        void OnRemoveModel(FileSceneElement instance)
        {
        }


     }

}
