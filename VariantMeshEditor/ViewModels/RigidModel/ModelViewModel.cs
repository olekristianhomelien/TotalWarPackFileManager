using CommonDialogs.Common;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VariantMeshEditor.Views.EditorViews.Util;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.RigidModel
{
    public class ModelViewModel : NotifyPropertyChangedImpl, IDisposable
    {

        // Actual data
        // ------------------------
        Rmv2LodModel LodModelInstance { get; set; }
        public TextureMeshRenderItem RenderInstance { get; set; }

        // Commands
        // ------------------------
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand PreviewCommand { get; set; }


        // View Model properties
        // ------------------------


        public GroupTypeEnum MaterialId { get { return LodModelInstance.MaterialId; } }

        bool _renderBB = false;
        public bool RenderBoundngBox
        {
            get { return _renderBB; }
            set { SetAndNotify(ref _renderBB, value); }
        }

        Vector3ViewData _pivot;
        public Vector3ViewData Pivot { get { return _pivot; } set { SetAndNotify(ref _pivot, value); } }

        FileMatrix3x4ViewData _transformMatrixA;
        public FileMatrix3x4ViewData TransformMatrixA { get { return _transformMatrixA; } set { SetAndNotify(ref _transformMatrixA, value); } }

        FileMatrix3x4ViewData _transformMatrixB;
        public FileMatrix3x4ViewData TransformMatrixB { get { return _transformMatrixB; } set { SetAndNotify(ref _transformMatrixB, value); } }

        FileMatrix3x4ViewData _transformMatrixC;
        public FileMatrix3x4ViewData TransformMatrixC { get { return _transformMatrixC; } set { SetAndNotify(ref _transformMatrixC, value); } }


        public VertexFormat VertexFormat { get { return LodModelInstance.VertexFormat; } }

        public int VertexCount { get { return (int)LodModelInstance.VertexCount; } }
        public int IndexCount { get { return (int)LodModelInstance.FaceCount; } }

        ObservableCollection<RigidModelAttachmentPoint> _attachmentPoints;
        public ObservableCollection<RigidModelAttachmentPoint> AttachmentPoints { get { return _attachmentPoints; } set { SetAndNotify(ref _attachmentPoints, value); } }

        public string ShaderName { get { return LodModelInstance.ShaderName; } }

        public string TextureDirectory { get; set; }
        public FileTextureViewModel DiffuseTexture { get; set; }
        public FileTextureViewModel SpecularTexture { get; set; }
        public FileTextureViewModel GlossTexture { get; set; }
        public FileTextureViewModel NormalTexture { get; set; }
        public FileTextureViewModel MaskTexture { get; set; }

        public string ModelName { get { return LodModelInstance.ModelName; } }


        bool _isVisible = true;
        public bool IsVisible { get { return _isVisible; } set { SetAndNotify(ref _isVisible, value); } }

        public ModelViewModel(Rmv2LodModel lodModelInstance, TextureMeshRenderItem renderInstance)
        {
            LodModelInstance = lodModelInstance;
            RenderInstance = renderInstance;
            Pivot = new Vector3ViewData(LodModelInstance.Transformation.Pivot, "Pivot");
            TransformMatrixA = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[0], "Matrix A");
            TransformMatrixB = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[1], "Matrix B");
            TransformMatrixC = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[2], "Matrix C");
            AttachmentPoints = new ObservableCollection<RigidModelAttachmentPoint>(LodModelInstance.AttachmentPoint);

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

        public void Dispose()
        {
            RenderInstance.Dispose();
        }
    }
}
