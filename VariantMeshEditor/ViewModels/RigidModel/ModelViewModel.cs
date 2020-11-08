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
    public class ModelViewModel : NotifyPropertyChangedImpl
    {

        // Actual data
        // ------------------------
        public Rmv2LodModel LodModelInstance { get; set; }
        public TextureMeshRenderItem RenderInstance { get; set; }

        // Commands
        // ------------------------
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand PreviewCommand { get; set; }


        // View Model properties
        // ------------------------
        public string TextureDirectory { get; set; }
        public FileTextureViewModel DiffuseTexture { get; set; }
        public FileTextureViewModel SpecularTexture { get; set; }
        public FileTextureViewModel GlossTexture { get; set; }
        public FileTextureViewModel NormalTexture { get; set; }
        public FileTextureViewModel MaskTexture { get; set; }

        public string ModelName { get { return LodModelInstance.ModelName; } }
        public int VertexCount { get; set; }

        bool _isVisible = true;
        public bool IsVisible { get { return _isVisible; } set { SetAndNotify(ref _isVisible, value); } }

        public ModelViewModel()
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
