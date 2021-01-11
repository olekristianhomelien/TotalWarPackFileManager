using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Skeleton;
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

      
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var topNode = SceneElementHelper.GetTopNode(this);
            if (topNode == null)
                throw new Exception("This should not be null");
            var parentAnimationNode = SceneElementHelper.GetFirstChild<AnimationElement>(topNode);
            var parentSkeletonNode = SceneElementHelper.GetFirstChild<SkeletonElement>(topNode);

            for (int lodIndex = 0; lodIndex < Model.Header.LodCount; lodIndex++)
            {
                var currentLod = new LodHeaderViewModel(Model.LodHeaders[lodIndex], $"Lod {lodIndex + 1}", lodIndex == 0);

                for (var modelIndex = 0; modelIndex < Model.LodHeaders[lodIndex].MeshCount; modelIndex++)
                {
                    var model = new ModelViewModel(parentSkeletonNode, parentAnimationNode, Model, lodIndex, modelIndex, virtualWorld, resourceLibary);
                    currentLod.Models.Add(model);
                }
                Lods.Add(currentLod);
            }

            if (Parent is WsModelElement)
            {
                IsChecked = true;
                ApplyElementCheckboxVisability = Visibility.Collapsed;
            }
            else
            {
                CheckBoxGroupingName = Parent?.CheckBoxGroupingName + "_RigidModel";
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
