using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.RigidModel
{
    public class RigidModelElement : FileSceneElement
    {
        public RmvRigidModel Model { get; set; }
        public ObservableCollection<LodHeaderViewModel> Lods { get; set; } = new ObservableCollection<LodHeaderViewModel>();

        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;


        public RigidModelElement(FileSceneElement parent, RmvRigidModel model, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            Model = model;
            DisplayName = $"RigidModel - {FileName}";

            if (parent is WsModelElement)
            {
                IsChecked = true;
                ApplyElementCheckboxVisability = Visibility.Collapsed;
            }
            else
            {
                CheckBoxGroupingName = parent?.CheckBoxGroupingName + "_RigidModel";
            }
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var topNode = SceneElementHelper.GetTopNode(this);
            var parentAnimationNode = SceneElementHelper.GetFirstChild<AnimationElement>(topNode);

            for (int lodIndex = 0; lodIndex < Model.Header.LodCount; lodIndex++)
            {
                var currentLod = new LodHeaderViewModel(Model.LodHeaders[lodIndex], $"Lod {lodIndex + 1}", lodIndex == 0);

                for (var modelIndex = 0; modelIndex < Model.LodHeaders[lodIndex].MeshCount; modelIndex++)
                {
                    var lodMesh = Model.MeshList[lodIndex][modelIndex];
                    Rmv2RenderModel meshModel = new Rmv2RenderModel();
                    meshModel.Create(parentAnimationNode?.AnimationPlayer, virtualWorld.GraphicsDevice, lodMesh);

                    TextureMeshRenderItem meshRenderItem = new TextureMeshRenderItem(meshModel, resourceLibary.GetEffect(ShaderTypes.Phazer), resourceLibary)
                    {
                        Visible = true,
                        Textures = meshModel.ResolveTextures(resourceLibary, virtualWorld.GraphicsDevice)
                    };

                    var model = new ModelViewModel(lodMesh, meshRenderItem, virtualWorld, resourceLibary);
                    currentLod.Models.Add(model);
                }
                Lods.Add(currentLod);
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

        public override void Dispose()
        {
            foreach (var lod in Lods)
                lod.Dispose();

            base.Dispose();
        }
    }
}
