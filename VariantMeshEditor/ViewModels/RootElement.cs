using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class RootElement : FileSceneElement
    {
        public RootElement() : base(null, "", "", "Root") { IsChecked = true; }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Root;

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            RootController controller = new RootController(null, this, resourceLibary, virtualWorld);

        }


        // Add remove for each child

    }

}
