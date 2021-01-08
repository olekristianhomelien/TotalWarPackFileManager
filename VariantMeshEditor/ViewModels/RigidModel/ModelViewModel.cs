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
using VariantMeshEditor.Views.TexturePreview;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.RigidModel
{
    public class ModelViewModel : NotifyPropertyChangedImpl, IDisposable
    {

        // Actual data
        // ------------------------
        RmvSubModel LodModelInstance { get; set; }
        public TextureMeshRenderItem RenderInstance { get; set; }

        // Commands
        // ------------------------
        public ICommand RemoveCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand PreviewCommand { get; set; }
        public ICommand BrowseTextureDirCommand { get; set; }

        // View Model properties
        // ------------------------


        public GroupTypeEnum MaterialId { get { return LodModelInstance.Header.MaterialId; } }

        bool _renderBB = false;
        public bool RenderBoundngBox
        {
            get { return _renderBB; }
            set { SetAndNotify(ref _renderBB, value); }
        }

        Vector3ViewData _pivot;
        public Vector3ViewData Pivot { get { return _pivot; } set { SetAndNotify(ref _pivot, value); } }

        FileMatrix3x4ViewData[] _transformMatrix = new FileMatrix3x4ViewData[3];
        public FileMatrix3x4ViewData[] TransformMatrix { get { return _transformMatrix; } }


        public bool TransformHasPivot { get { return !LodModelInstance.Header.Transform.IsIdentityPivot(); } }
        public bool TransformHasIdentityMatrices { get { return LodModelInstance.Header.Transform.IsIdentityMatrices(); } }

        public VertexFormat VertexFormat { get { return LodModelInstance.Header.VertextType; } }

        public int VertexCount { get { return (int)LodModelInstance.Header.VertexCount; } }
        public int IndexCount { get { return (int)LodModelInstance.Header.FaceCount; } }

        ObservableCollection<RmvAttachmentPoint> _attachmentPoints;
        public ObservableCollection<RmvAttachmentPoint> AttachmentPoints { get { return _attachmentPoints; } set { SetAndNotify(ref _attachmentPoints, value); } }


        public List<AlphaMode> PossibleAlphaModes { get { return new List<AlphaMode> { AlphaMode.Alpha_Blend, AlphaMode.Alpha_Test, AlphaMode.Opaque }; } }
        public AlphaMode SelectedAlphaMode 
        { 
            get { return LodModelInstance.Mesh.AlphaSettings.Mode; } 
            set 
            {
                var settings = LodModelInstance.Mesh.AlphaSettings;
                settings.Mode = value; RenderInstance.AlphaMode = (int)value; 
                NotifyPropertyChanged();  
            } 
        }

        public string ShaderName { get { return LodModelInstance.Header.ShaderParams.ShaderName; } }

        public string TextureDirectory 
        { 
            get { return LodModelInstance.Header.TextureDirectory; } 
            set 
            {
                var item = LodModelInstance.Header;
                item.TextureDirectory = value; 
                NotifyPropertyChanged(); 
            } 
        }


        public Dictionary<TexureType, FileTextureViewModel> Textures { get; set; } = new Dictionary<TexureType, FileTextureViewModel>();

        public string ModelName { get { return LodModelInstance.Header.ModelName; } }


        bool _isVisible = true;
        public bool IsVisible { get { return _isVisible; } set { SetAndNotify(ref _isVisible, value); } }


        public int LinkDirectlyToBoneIndex { get { return LodModelInstance.Header.LinkDirectlyToBoneIndex; } }


        Scene3d _virtualWorld;
        ResourceLibary _resourceLibary;

        public ModelViewModel(RmvSubModel lodModelInstance, TextureMeshRenderItem renderInstance, Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _virtualWorld = virtualWorld;
            _resourceLibary = resourceLibary;

            LodModelInstance = lodModelInstance;
            RenderInstance = renderInstance;
            //Pivot = new Vector3ViewData(LodModelInstance.Transformation.Pivot, "Pivot");
            //TransformMatrix[0] = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[0], "Matrix A");
            //TransformMatrix[1] = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[1], "Matrix B");
            //TransformMatrix[2] = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[2], "Matrix C");
            AttachmentPoints = new ObservableCollection<RmvAttachmentPoint>(LodModelInstance.AttachmentPoints);
            //
            foreach (var texture in LodModelInstance.Textures)
                Textures.Add(texture.TexureType, new FileTextureViewModel() { Path = texture.Path, RenderInstance = renderInstance });
            //
            SelectedAlphaMode = LodModelInstance.Mesh.AlphaSettings.Mode;

            BrowseCommand = new RelayCommand<FileTextureViewModel>(OnBrowseCommand);
            BrowseTextureDirCommand = new RelayCommand<FileTextureViewModel>(OnBrowseTextureDirCommand);
            RemoveCommand = new RelayCommand<FileTextureViewModel>(OnRemoveCommand);
            PreviewCommand = new RelayCommand<FileTextureViewModel>(OnPreviewCommand);

        }

        void OnBrowseTextureDirCommand(FileTextureViewModel element)
        {

        }

        void OnBrowseCommand(FileTextureViewModel element)
        {

        }

        void OnRemoveCommand(FileTextureViewModel element)
        {
            element.Path = "";
        }





        void OnPreviewCommand(FileTextureViewModel element)
        {
            TexturePreviewController.Create(element.Path, _virtualWorld.TextureToTextureRenderer, _resourceLibary);
        }

        public void Dispose()
        {
            RenderInstance.Dispose();
        }
    }
}
