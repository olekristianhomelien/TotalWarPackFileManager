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


        // Commands
        // ------------------------
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand PreviewCommand { get; set; }


        // View Model properties
        // ------------------------
        public TexureType Type { get; set; }
        public string Path { get; set; }


        public FileTextureViewModel()
        {
            BrowseCommand = new RelayCommand<RigidModelElement>(OnBrowseCommand);
            RemoveCommand = new RelayCommand<RigidModelElement>(OnRemoveCommand);
            PreviewCommand = new RelayCommand<RigidModelElement>(OnPreviewCommand);
        }



        void OnRemoveCommand(RigidModelElement element)
        {
        }

        void OnBrowseCommand(RigidModelElement element)
        {
        }


        void OnPreviewCommand(RigidModelElement element)
        {
        }
    }
}
