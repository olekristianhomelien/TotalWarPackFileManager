using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels
{
    public class VariantMeshElement : FileSceneElement
    {
        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;
    }
}
