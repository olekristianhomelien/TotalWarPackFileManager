using Microsoft.Xna.Framework;
using System.Linq;
using System.Windows;
using VariantMeshEditor.Util;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.Animation
{
    public class AnimationElement : FileSceneElement
    {
        protected AnimationExplorerViewModel AnimationExplorer { get; set; }
        protected AnimationPlayerViewModel AnimationPlayerViewModel { get; set; }
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();

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
                AnimationExplorer = new AnimationExplorerViewModel(resourceLibary, skeleton.First(), this);
                AnimationPlayerViewModel = new AnimationPlayerViewModel(this);
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
