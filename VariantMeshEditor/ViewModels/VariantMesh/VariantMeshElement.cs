using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Skeleton;

namespace VariantMeshEditor.ViewModels.VariantMesh
{
    public class VariantMeshElement : FileSceneElement
    {
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;

        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
        }
    }
}
