using CommonDialogs.Common;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.RigidModel
{
    public class FileTextureViewModel : NotifyPropertyChangedImpl
    {
        // Actual data
        // ------------------------
        public TextureMeshRenderItem RenderInstance { get; set; }

        // View Model properties
        // ------------------------

        public string _path;
        public string Path { get { return _path; } set { SetAndNotify(ref _path, value); } }


        public bool _useTexture = true;
        public bool UseTexture { get { return _useTexture; } set { SetAndNotify(ref _useTexture, value); } }

        public FileTextureViewModel()
        {

        }
    }
}
