using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class SkeletonElement : FileSceneElement
    {

        SkeletonController Controller { get; set; }
        public AnimationFile SkeletonFile { get; set; }
        SkeletonModel SkeletonModel { get; set; }
        public Skeleton Skeleton { get; set; }


        public override UserControl EditorViewModel { get => Controller.GetView(); protected set => throw new System.Exception(); }

        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, "", fullPath, "Skeleton")
        {
            ApplyElementCheckboxVisability = System.Windows.Visibility.Hidden;  
        }

        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;


        public void Create(AnimationPlayer animationPlayer, ResourceLibary resourceLibary, string skeletonName)
        {
            string animationFolder = "animations\\skeletons\\";
            var skeletonFilePath = animationFolder + skeletonName;
            var file = PackFileLoadHelper.FindFile(resourceLibary.PackfileContent, skeletonFilePath);
            if (file != null)
            {
                SkeletonFile = AnimationFile.Create(new ByteChunk(file.Data));
                FullPath = skeletonFilePath;
                FileName = Path.GetFileNameWithoutExtension(skeletonFilePath);
                Skeleton = new Skeleton(SkeletonFile);
            }

            SkeletonModel = new SkeletonModel(resourceLibary.GetEffect(ShaderTypes.Line));
            SkeletonModel.Create(animationPlayer, Skeleton);
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            Controller = new SkeletonController(this);
        }

        protected override void UpdateNode(GameTime time)
        {
            SkeletonModel.Update(time);
        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            SkeletonModel.Draw(device, parentTransform, commonShaderParameters);
        }
    }
}
