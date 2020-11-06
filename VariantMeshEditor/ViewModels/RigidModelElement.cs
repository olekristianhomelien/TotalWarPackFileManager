using CommonDialogs.Common;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{

    public class LodHeaderViewModel : NotifyPropertyChangedImpl
    {
        public LodHeaderViewModel(LodHeader header, string name)
        {
            LodHeader = header;
            LodName = name;
        }
        public LodHeader LodHeader { get; private set; }
        public string LodName { get; private set; }

        public byte QualityLvl { get { return LodHeader.QualityLvl; } set { LodHeader.QualityLvl = value; NotifyPropertyChanged(); } }
        public float LodCameraDistance{ get { return LodHeader.LodCameraDistance; } set { LodHeader.LodCameraDistance = value; NotifyPropertyChanged();} }
        public ObservableCollection<ModelViewModel> Models { get; set; } = new ObservableCollection<ModelViewModel>();
    }

    public class TextureViewModel : NotifyPropertyChangedImpl
    {
        // Actual data
        // ------------------------
        public TextureMeshRenderItem RenderInstance { get; set; }


        // Commands
        // ------------------------
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand PreviewCommand { get; set; }


        // View Model properties
        // ------------------------
        public TexureType Type { get; set; }
        public string Path { get; set; }


        public TextureViewModel()
        {
            BrowseCommand = new RelayCommand<RigidModelElement>(OnBrowseCommand);
            RemoveCommand = new RelayCommand<RigidModelElement>(OnRemoveCommand);
            PreviewCommand = new RelayCommand<RigidModelElement>(OnPreviewCommand);
        }



        void OnRemoveCommand(RigidModelElement element)
        {
        }

        void OnBrowseCommand(RigidModelElement element)
        {
        }


        void OnPreviewCommand(RigidModelElement element)
        {
        }
    }

    public class ModelViewModel : NotifyPropertyChangedImpl
    {

        // Actual data
        // ------------------------
        public LodModel LodModelInstance { get; set; }
        public TextureMeshRenderItem RenderInstance { get; set; }

        // Commands
        // ------------------------
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand PreviewCommand { get; set; }


        // View Model properties
        // ------------------------
        public string TextureDirectory { get; set; }
        public TextureViewModel DiffuseTexture { get; set; }
        public TextureViewModel SpecularTexture { get; set; }
        public TextureViewModel GlossTexture { get; set; }
        public TextureViewModel NormalTexture { get; set; }
        public TextureViewModel MaskTexture { get; set; }


        //public string Name { get { return LodModelInstance.ModelName; } set { LodModelInstance.ModelName = value; NotifyPropertyChanged(); } }
        public string Name { get; set; }
        public int VertexCount { get; set; }

        public ModelViewModel()
        {
            BrowseCommand = new RelayCommand<RigidModelElement>(OnBrowseCommand);
            RemoveCommand = new RelayCommand<RigidModelElement>(OnRemoveCommand);
            PreviewCommand = new RelayCommand<RigidModelElement>(OnPreviewCommand);
        }

        void OnRemoveCommand(RigidModelElement element)
        {

        }

        void OnBrowseCommand(RigidModelElement element)
        {

        }


        void OnPreviewCommand(RigidModelElement element)
        {

        }
    }


    //{Binding Path=UpdateCommand}" CommandParameter="{Binding ElementName=lstPerson, Path=SelectedItem.Address}">  


    public class RigidModelElement : FileSceneElement
    {
        public Rmv2RigidModel Model { get; set; }
        // Render item


        public string SomeDbgStr{ get; set; }
        public ObservableCollection<LodHeaderViewModel> Lods { get; set; } = new ObservableCollection<LodHeaderViewModel>();

        RigidModelController Controller { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }


        void OnRemoveCommand(RigidModelElement element)
        { 
        
        }

        void OnBrowseCommand(RigidModelElement element)
        {

        }

        public override UserControl EditorViewModel { get => Controller.GetView(); protected set => throw new System.Exception(); }
        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;


        public RigidModelElement(FileSceneElement parent, Rmv2RigidModel model, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {

            CheckBoxGroupingName = parent.CheckBoxGroupingName + "_RigidModel";
            IsChecked = parent.Children.Count() == 0;

            Model = model;
            DisplayName = $"RigidModel - {FileName}";
            BrowseCommand = new RelayCommand<RigidModelElement>(OnBrowseCommand);
            RemoveCommand = new RelayCommand<RigidModelElement>(OnRemoveCommand);

            for(int i = 0; i < model.LodHeaders.Count(); i++) 
            {
                var modelLodHeader = model.LodHeaders[i];
                var currentLoad = new LodHeaderViewModel(modelLodHeader, $"Lod {i + 1}");

                currentLoad.Models.Add(new ModelViewModel() { Name = "Mesh0", VertexCount = 1789 });
                currentLoad.Models.Add(new ModelViewModel() { Name = "Mesh1", VertexCount = 2789 });
                currentLoad.Models.Add(new ModelViewModel() { Name = "Mesh2", VertexCount = 3789 });

                Lods.Add(currentLoad);
            }

            //var lod0 = new LodHeaderViewModel() { LodName = "Lod0" };
            //lod0.Models.Add(new ModelViewModel() { Name = "Mesh0", VertexCount = 1789 });
            //lod0.Models.Add(new ModelViewModel() { Name = "Mesh1", VertexCount = 2789 });
            //lod0.Models.Add(new ModelViewModel() { Name = "Mesh2", VertexCount = 3789 });
            //
            //var lod1 = new LodHeaderViewModel() { LodName = "Lod1",QualityLvl=3,LodCameraDistance=3000 };
            //lod1.Models.Add(new ModelViewModel() { Name = "Mesh0", VertexCount = 4789 });
            //
            //
            //Lods.Add(lod1);
        }

       

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {

            Controller = new RigidModelController(this, resourceLibary, virtualWorld);

          // var animation = SceneElementHelper.GetAllOfTypeInSameVariantMesh<AnimationElement>(this).FirstOrDefault();
          //
          // for (int lodIndex = 0; lodIndex < Model.LodHeaders.Count; lodIndex++)
          // {
          //     for (int modelIndex = 0; modelIndex < Model.LodHeaders[lodIndex].LodModels.Count(); modelIndex++)
          //     {
          //
          //
          //         Rmv2Model meshModel = new Rmv2Model();
          //         meshModel.Create(animation?.AnimationPlayer, virtualWorld.GraphicsDevice, Model, lodIndex, modelIndex);
          //
          //         TextureMeshRenderItem meshRenderItem = new TextureMeshRenderItem(meshModel, resourceLibary.GetEffect(ShaderTypes.Mesh))
          //         {
          //             Visible = lodIndex == 0,
          //             Textures = meshModel.ResolveTextures(resourceLibary, virtualWorld.GraphicsDevice)
          //         };
          //
          //         MeshInstances[lodIndex].Add(meshRenderItem);
          //         AssignModel(meshRenderItem, lodIndex, modelIndex);
          //     }
          // }


            /*
           

            for (int lodIndex = 0; lodIndex < _element.Model.LodHeaders.Count; lodIndex++)
            {
                MeshInstances.Add(new List<MeshRenderItem>());

                
            }
             
             
             */
        }


        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            Controller.DrawNode(device, parentTransform, commonShaderParameters);
        }
    }
}
