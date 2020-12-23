using Common;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.Skeleton
{
    public class SkeletonElement : FileSceneElement
    {
        public AnimationFile SkeletonFile { get; set; }
        public SkeletonRender SkeletonRenderer { get; set; }
        public GameSkeleton GameSkeleton { get; set; }

        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;

        public SkeletonViewModel ViewModel { get; set; }

        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, "", fullPath, "Skeleton")
        {
        }

        public void Create(AnimationPlayer animationPlayer, ResourceLibary resourceLibary, string skeletonName)
        {
            string animationFolder = "animations\\skeletons\\";
            var skeletonFilePath = animationFolder + skeletonName;
            var file = PackFileLoadHelper.FindFile(resourceLibary.PackfileContent, skeletonFilePath);
            if (file != null)
            {
                SkeletonFile = AnimationFile.Create(file);
                FullPath = skeletonFilePath;
                FileName = Path.GetFileNameWithoutExtension(skeletonFilePath);
                GameSkeleton = new GameSkeleton(SkeletonFile, animationPlayer);
                SkeletonRenderer = new SkeletonRender(resourceLibary.GetEffect(ShaderTypes.Line), GameSkeleton);
                DisplayName = "Skeleton - " + FileName;
            }

            ViewModel = new SkeletonViewModel(this);
        }


        protected override void UpdateNode(GameTime time)
        {
            GameSkeleton.Update();
        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            SkeletonRenderer.SelectedBoneIndex = ViewModel.SelectedBone?.BoneIndex;
            SkeletonRenderer.Draw(device, parentTransform, commonShaderParameters);
        }
    }
}
