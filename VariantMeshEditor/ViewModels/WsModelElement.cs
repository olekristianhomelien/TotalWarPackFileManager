using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.ViewModels
{
    public class WsModelElement : FileSceneElement
    {
        public WsModelElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            DisplayName = $"WsModel - {FileName}";
            CheckBoxGroupingName = parent.CheckBoxGroupingName + "_wsModel_";
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.WsModel;
    }

}
