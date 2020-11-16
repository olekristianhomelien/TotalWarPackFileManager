using Microsoft.Xna.Framework;
using System.Linq;
using System.Windows;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class AnimationElement : FileSceneElement
    {
        public AnimationExplorerViewModel AnimationExplorer { get; set; }
        public AnimationPlayerViewModel AnimationPlayerViewModel { get; set; }
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();
        public FragmentExplorerViewModel AnimationFragmentExplorerViewModel { get; set; }

        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;
        

        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation")
        {
            ApplyElementCheckboxVisability = Visibility.Hidden;
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this);
            if (skeleton.Count == 1)
            {
                AnimationPlayerViewModel = new AnimationPlayerViewModel(this);
                AnimationExplorer = new AnimationExplorerViewModel(resourceLibary, skeleton.First(), AnimationPlayerViewModel);
                AnimationFragmentExplorerViewModel = new FragmentExplorerViewModel(resourceLibary, AnimationPlayerViewModel);
            }
        }

        protected override void UpdateNode(GameTime time)
        {
            AnimationPlayer.Update(time);
            if (AnimationPlayerViewModel != null)
                AnimationPlayerViewModel.Update();
        }
    }
}
