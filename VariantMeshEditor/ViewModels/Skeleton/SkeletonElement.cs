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

        public void Create(AnimationPlayer animationPlayer, ResourceLibary resourceLibary, PackedFile skeletonPackFile)
        {
            if (skeletonPackFile != null)
            {
                SkeletonFile = AnimationFile.Create(skeletonPackFile);
                FullPath = skeletonPackFile.FullPath;
                FileName = Path.GetFileNameWithoutExtension(FullPath);
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
