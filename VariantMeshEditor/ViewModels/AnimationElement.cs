using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Controls.EditorControllers.Animation;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class AnimationElement : FileSceneElement
    {
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();

        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation") 
        {
            ApplyElementCheckboxVisability = System.Windows.Visibility.Hidden; 
        }


        

        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;



        AnimationController _controller;



        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this);
            if (skeleton.Count == 1)
            {
                _controller = new AnimationController(resourceLibary, this, skeleton.First());
            }
        }

        protected override void UpdateNode(GameTime time)
        {
            AnimationPlayer.Update(time);
            DisplayName = "Animation - " + _controller.GetCurrentAnimationName();
            _controller.Update();
        }
    }
}
