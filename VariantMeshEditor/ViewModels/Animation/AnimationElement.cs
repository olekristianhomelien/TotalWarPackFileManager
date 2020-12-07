using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Windows;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class AnimationElement : FileSceneElement
    {
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();
        public AnimationPlayerViewModel AnimationPlayerViewModel { get; set; }

        public AnimationExplorerViewModel AnimationExplorerViewModel { get; set; }    
        public FragmentExplorerViewModel AnimationFragmentExplorerViewModel { get; set; }
        public AnimationSplicerViewModel AnimationSplicerViewModel { get; set; }

        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;
        

        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation")
        {
            ApplyElementCheckboxVisability = Visibility.Collapsed;
            IsChecked = true;   // Triggers update and render
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this);
            if (skeleton.Count == 1)
            {
                AnimationPlayerViewModel = new AnimationPlayerViewModel(this);
                AnimationExplorerViewModel = new AnimationExplorerViewModel(resourceLibary, skeleton.First(), AnimationPlayerViewModel);
                AnimationFragmentExplorerViewModel = new FragmentExplorerViewModel(resourceLibary, AnimationPlayerViewModel);
                AnimationSplicerViewModel = new AnimationSplicerViewModel(resourceLibary, skeleton.First(), AnimationPlayerViewModel);
            }
        }

        protected override void UpdateNode(GameTime time)
        {
            AnimationPlayer.Update(time);
            if (AnimationPlayerViewModel != null)
                AnimationPlayerViewModel.Update();

            AnimationSplicerViewModel.UpdateNode(time);
        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            AnimationSplicerViewModel.DrawNode(device, parentTransform, commonShaderParameters);
        }
    }
}
