using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels.VariantMesh
{
    public class VariantMeshElement : FileSceneElement
    {
        public VariantMeshViewModel ViewModel { get; set; }

        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
            ViewModel = new VariantMeshViewModel(this);
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;
    }
}
