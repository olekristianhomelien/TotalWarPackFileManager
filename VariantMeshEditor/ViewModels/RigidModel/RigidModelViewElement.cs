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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.RigidModel
{
    public class RigidModelElement : FileSceneElement
    {
        public Rmv2RigidModel Model { get; set; }
        public ObservableCollection<LodHeaderViewModel> Lods { get; set; } = new ObservableCollection<LodHeaderViewModel>();

        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }


        void OnRemoveCommand(RigidModelElement element)
        {

        }

        void OnBrowseCommand(RigidModelElement element)
        {

        }

        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;


        public RigidModelElement(FileSceneElement parent, Rmv2RigidModel model, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            CheckBoxGroupingName = parent.CheckBoxGroupingName + "_RigidModel";

            Model = model;
            DisplayName = $"RigidModel - {FileName}";
            BrowseCommand = new RelayCommand<RigidModelElement>(OnBrowseCommand);
            RemoveCommand = new RelayCommand<RigidModelElement>(OnRemoveCommand);
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var parentAnimationNode = SceneElementHelper.GetAllOfTypeInSameVariantMesh<AnimationElement>(this).FirstOrDefault();

            for (int lodIndex = 0; lodIndex < Model.LodHeaders.Count; lodIndex++)
            {
                var currentLoad = new LodHeaderViewModel(Model.LodHeaders[lodIndex], $"Lod {lodIndex + 1}", lodIndex == 0);
                
                foreach (var lodModel in Model.LodHeaders[lodIndex].LodModels)
                {             
                    Rmv2RenderModel meshModel = new Rmv2RenderModel();
                    meshModel.Create(parentAnimationNode?.AnimationPlayer, virtualWorld.GraphicsDevice, lodModel);

                    TextureMeshRenderItem meshRenderItem = new TextureMeshRenderItem(meshModel, resourceLibary.GetEffect(ShaderTypes.Mesh))
                    {
                        Visible = true,
                        Textures = meshModel.ResolveTextures(resourceLibary, virtualWorld.GraphicsDevice)
                    };

                    var model = new ModelViewModel() { LodModelInstance = lodModel, RenderInstance = meshRenderItem };
                    currentLoad.Models.Add(model);
                }

                Lods.Add(currentLoad);
            }

        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            foreach (var lods in Lods)
            {
                if (lods.IsVisible)
                {
                    foreach (var model in lods.Models)
                    {
                        if(model.IsVisible)
                            model.RenderInstance.Draw(device, parentTransform, commonShaderParameters);
                    }
                }
            }
        }
    }
}
