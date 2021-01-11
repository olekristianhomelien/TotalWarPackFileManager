using Common;
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
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.Views.EditorViews.Util;
using VariantMeshEditor.Views.TexturePreview;
using Viewer.Animation;
using Viewer.GraphicModels;
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


        public ICommand FixBoneCommand { get; set; }
        
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
        SkeletonElement _parentSkeleton;
        AnimationElement _parentAnimation;
        RmvRigidModel _model;
        int _lodIndex;
        int _modelIndex;

        public ModelViewModel(SkeletonElement parentSkeleton, AnimationElement parentAnimation, RmvRigidModel model, int lodIndex, int modelIndex, Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _parentSkeleton = parentSkeleton;
            _parentAnimation = parentAnimation;
            _model = model;
            _lodIndex = lodIndex;
            _modelIndex = modelIndex;
            _virtualWorld = virtualWorld;
            _resourceLibary = resourceLibary;
         

      
            Create3dModel();

            //Pivot = new Vector3ViewData(LodModelInstance.Transformation.Pivot, "Pivot");
            //TransformMatrix[0] = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[0], "Matrix A");
            //TransformMatrix[1] = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[1], "Matrix B");
            //TransformMatrix[2] = new FileMatrix3x4ViewData(LodModelInstance.Transformation.Matrices[2], "Matrix C");
            AttachmentPoints = new ObservableCollection<RmvAttachmentPoint>(LodModelInstance.AttachmentPoints);
            //
            foreach (var texture in LodModelInstance.Textures)
                Textures.Add(texture.TexureType, new FileTextureViewModel() { Path = texture.Path, RenderInstance = RenderInstance });
            //
            SelectedAlphaMode = LodModelInstance.Mesh.AlphaSettings.Mode;

            BrowseCommand = new RelayCommand<FileTextureViewModel>(OnBrowseCommand);
            BrowseTextureDirCommand = new RelayCommand<FileTextureViewModel>(OnBrowseTextureDirCommand);
            RemoveCommand = new RelayCommand<FileTextureViewModel>(OnRemoveCommand);
            PreviewCommand = new RelayCommand<FileTextureViewModel>(OnPreviewCommand);
            FixBoneCommand = new RelayCommand(OnFixBoneCommand);

        }

        void Create3dModel()
        {
            LodModelInstance = _model.MeshList[_lodIndex][_modelIndex];
            Rmv2RenderModel meshModel = new Rmv2RenderModel();
            meshModel.Create(_parentAnimation?.AnimationPlayer, _virtualWorld.GraphicsDevice, LodModelInstance);

            TextureMeshRenderItem meshRenderItem = new TextureMeshRenderItem(meshModel, _resourceLibary.GetEffect(ShaderTypes.Phazer), _resourceLibary)
            {
                Visible = true,
                Textures = meshModel.ResolveTextures(_resourceLibary, _virtualWorld.GraphicsDevice)
            };

            LodModelInstance = _model.MeshList[_lodIndex][_modelIndex];
            RenderInstance = meshRenderItem;
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

        public void OnFixBoneCommand()
        {
            var boneIndecisThatNeedMapping = LodModelInstance.Mesh.VertexList.SelectMany(x => x.BoneIndex)
                .Distinct()
                .ToList();

            var expectedSkeletonName = _parentSkeleton.GameSkeleton.SkeletonName;
            var actualSkeletonName = _model.Header.SkeletonName;
            if (expectedSkeletonName != actualSkeletonName)
            {

                string animationFolder = "animations\\skeletons\\";
                var skeletonFilePath = animationFolder + actualSkeletonName + ".anim";
                var skeletonPackFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, skeletonFilePath);
                AnimationFile skeletonFile = AnimationFile.Create(skeletonPackFile);
                GameSkeleton gameSkeleton = new GameSkeleton(skeletonFile, null);

                var boneNameDebug = boneIndecisThatNeedMapping.Select(x => $"{x} - {gameSkeleton.BoneNames[x]}").ToList();
                var boneNames = boneIndecisThatNeedMapping.Select(x =>gameSkeleton.BoneNames[x]).ToList();

                var mapping = boneNames.Select(x => _parentSkeleton.GameSkeleton.GetBoneIndexByName(x)).ToList();


                List<MappingData> mappingData = new List<MappingData>();
                for (int i = 0; i < boneIndecisThatNeedMapping.Count; i++)
                {
                    var d = new MappingData()
                    {
                        OriginalBoneIndex = boneIndecisThatNeedMapping[i],
                        OriginalBoneName = boneNames[i],
                        NewBoneIndex = mapping[i],
                        NewBoneName = GetNameSafe(_parentSkeleton.GameSkeleton, mapping[i])

                    };
                    mappingData.Add(d);
                }


                for (int vertexIndex = 0; vertexIndex < LodModelInstance.Mesh.VertexList.Length; vertexIndex++)
                {
                    // Is one we should update
                    for (int boneIndex = 0; boneIndex < LodModelInstance.Mesh.VertexList[vertexIndex].BoneIndex.Length; boneIndex++)
                    {
                        var currentBoneIndex = LodModelInstance.Mesh.VertexList[vertexIndex].BoneIndex[boneIndex];
                        if (boneIndecisThatNeedMapping.Contains(currentBoneIndex))
                        {
                            var currentBoneName = mappingData.First(x => x.OriginalBoneIndex == currentBoneIndex).OriginalBoneName;
                            var originalBoneIndex = mappingData.First(x => x.OriginalBoneIndex == currentBoneIndex).OriginalBoneIndex;
                            var newBoneIndex = mappingData.First(x => x.OriginalBoneIndex == currentBoneIndex).NewBoneIndex;
                            var newBoneName = mappingData.First(x => x.OriginalBoneIndex == currentBoneIndex).NewBoneName;
                         

                            if(originalBoneIndex == 22)
                                newBoneIndex = 22;

                            if (originalBoneIndex == 59)
                                newBoneIndex = 53;

                            if (newBoneIndex == -1)
                                newBoneIndex = 22;

                            newBoneIndex = 22;
                            LodModelInstance.Mesh.VertexList[vertexIndex].BoneIndex[boneIndex] = (byte)newBoneIndex;

                        }
                    
                    }
                    

                
                }


            }

            Create3dModel();
        }

        string GetNameSafe(GameSkeleton skeleton, int index)
        {
            if (index == -1)
                return "";
            else
                return skeleton.BoneNames[index];
        }

        class MappingData
        { 
            public string OriginalBoneName { get; set; }
            public int OriginalBoneIndex { get; set; }
            public string NewBoneName { get; set; }
            public int NewBoneIndex { get; set; }

            public override string ToString()
            {
                return $"{OriginalBoneName}[{OriginalBoneIndex}] => {NewBoneName}[{NewBoneIndex}]";
            }
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
