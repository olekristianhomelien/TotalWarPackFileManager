using Common;
using CommonDialogs;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pfim;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;
using VariantMeshEditor.Views.TexturePreview;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.Controls.EditorControllers
{
    public class RigidModelController
    {
        RigidModelEditorView _view;
        RigidModelElement _element;
        ResourceLibary _resourceLibary;
        Scene3d _world;
        Dictionary<RigidModelMeshEditorView, MeshRenderItem> _modelEditors = new Dictionary<RigidModelMeshEditorView, MeshRenderItem>();
        List<List<MeshRenderItem>> MeshInstances { get; set; } = new List<List<MeshRenderItem>>();

        public RigidModelController(RigidModelElement element, ResourceLibary resourceLibary, Scene3d world)
        {
            _element = element;
            _resourceLibary = resourceLibary;
            _world = world;
            //PopulateUi(_view, _element);

            Create3dModels(world, resourceLibary);
        }

        public RigidModelEditorView GetView()
        {
            if (_view == null)
            {
                _view = new RigidModelEditorView();
                PopulateUi(_view, _element);
            }
            return _view;
        }


        public void AssignModel(MeshRenderItem meshInstance, int lodIndex, int modelIndex)
        {
            //var item = _modelEditors.Where(x => x.Key.ModelIndex == modelIndex && x.Key.LodIndex == lodIndex).First();
            //_modelEditors[item.Key] = meshInstance;
        }

        void Create3dModels( Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            var animation = SceneElementHelper.GetAllOfTypeInSameVariantMesh<AnimationElement>(_element).FirstOrDefault();

            for (int lodIndex = 0; lodIndex < _element.Model.LodHeaders.Count; lodIndex++)
            {
                MeshInstances.Add(new List<MeshRenderItem>());

                for (int modelIndex = 0; modelIndex < _element.Model.LodHeaders[lodIndex].LodModels.Count(); modelIndex++)
                {
                    

                    Rmv2Model meshModel = new Rmv2Model();
                    meshModel.Create(animation?.AnimationPlayer, virtualWorld.GraphicsDevice, _element.Model, lodIndex, modelIndex);

                    TextureMeshRenderItem meshRenderItem = new TextureMeshRenderItem(meshModel, resourceLibary.GetEffect(ShaderTypes.Mesh))
                    {
                        Visible = lodIndex == 0,
                        Textures = meshModel.ResolveTextures(resourceLibary, virtualWorld.GraphicsDevice)
                    };

                    MeshInstances[lodIndex].Add(meshRenderItem);
                    AssignModel(meshRenderItem, lodIndex, modelIndex);
                }
            }
        }

        class Lod 
        {
            public LodHeader LodHeader { get; set; }
            public LodEditorView Editor { get; set; }
            public List<Model> Models { get; set; } = new List<Model>();
        }

        class Model 
        {
            public RigidModelMeshEditorView Editor { get; set; }
            public MeshRenderItem RenderItem { get; set; }
        }

        List<Lod> _dataList  = new List<Lod>();



        void CreateLod(LodHeader loadHead, RigidModelEditorView view)
        {
            var lodCollapsableButton = new CollapsableButton()
            {
                LabelText = ($"Lod - {loadHead.LodLevel}")
            };

            

            LodEditorView lodEditorView = new LodEditorView();
            lodEditorView.LodCameraDistance.Text = $"{loadHead.LodCameraDistance}";
            lodEditorView.QualityLvl.Text = $"{loadHead.QualityLvl}";

            var lodStackPanel = new StackPanel();
            lodStackPanel.Children.Add(lodEditorView);
            lodCollapsableButton.InnerContent = lodStackPanel;

            var lod = new Lod { Editor = lodEditorView, LodHeader = loadHead };
            lodCollapsableButton.CheckBox.Click += (s, e) => CheckBox_Click(lod);

            foreach (var mesh in loadHead.LodModels)
            {
                var lodModelContent = CreateLodModel(mesh, (int)loadHead.LodLevel, 0, lod);
                lodStackPanel.Children.Add(lodModelContent);
            }



            // Add the lod to the veiw
            view.LodStackPanel.Children.Add(lodCollapsableButton);
        }

        private void CheckBox_Click(Lod lod)
        {
            //throw new NotImplementedException();
        }

        CollapsableButton CreateLodModel(LodModel mesh, int currentLodIndex, int currentModelIndex, Lod parentLod)
        {
            var meshContnet = new CollapsableButton()
            {
                LabelText = $"{mesh.ModelName}"
            };
            var meshStackPanel = new StackPanel();
            meshContnet.InnerContent = meshStackPanel;

            // Create the model

            var meshView = new RigidModelMeshEditorView
            {
                ModelIndex = currentModelIndex,
                LodIndex = currentLodIndex
            };
            _modelEditors.Add(meshView, null);

            meshView.ModelType.Text = mesh.MaterialId.ToString();
            meshView.VisibleCheckBox.Click += (sender, arg) => VisibleCheckBox_Click(meshView);

            DisplayTransforms(mesh, meshView);

            meshView.VertexType.Text = mesh.VertexFormat.ToString();
            meshView.VertexCount.Text = mesh.VertexCount.ToString();
            meshView.FaceCount.Text = mesh.FaceCount.ToString();

            meshView.AlphaMode.Items.Add(mesh.AlphaMode);
            meshView.AlphaMode.SelectedIndex = 0;

            foreach (var bone in mesh.AttachmentPoint)
                meshView.BoneList.Items.Add(bone.Name);

            meshView.TextureDir.LabelName.Width = 100;
            meshView.TextureDir.LabelName.Content = "Texture Directory";
            meshView.TextureDir.PathTextBox.Text = mesh.TextureDirectory;
            meshView.TextureDir.RemoveButton.Visibility = System.Windows.Visibility.Collapsed;
            meshView.TextureDir.PreviewButton.Visibility = System.Windows.Visibility.Collapsed;

            CreateTextureDisplayItem(mesh, meshView.Diffuse, TexureType.Diffuse);
            CreateTextureDisplayItem(mesh, meshView.Specular, TexureType.Specular);
            CreateTextureDisplayItem(mesh, meshView.Normal, TexureType.Normal);
            CreateTextureDisplayItem(mesh, meshView.Mask, TexureType.Mask);
            CreateTextureDisplayItem(mesh, meshView.Gloss, TexureType.Gloss);

            AddUnknownTexture(meshView, mesh);
            AddUnknowData(meshView, mesh);

            meshStackPanel.Children.Add(meshView);

            parentLod.Models.Add( new Model() {Editor = meshView,});

            return meshContnet;

        }

        private void PopulateUi(RigidModelEditorView view, RigidModelElement element)
        {
            foreach (var loadHead in element.Model.LodHeaders)
                CreateLod(loadHead, view);

        }

        void DisplayTransforms(LodModel mesh, RigidModelMeshEditorView view)
        {
            view.PivotView.GroupBox.Header = "Pivot";
            view.PivotView.Row0_0.Text = mesh.Transformation.Pivot.X.ToString();
            view.PivotView.Row0_1.Text = mesh.Transformation.Pivot.Y.ToString();
            view.PivotView.Row0_2.Text = mesh.Transformation.Pivot.Z.ToString();
            DisplayMatrix("Unknown0", view.MatrixView0, mesh.Transformation.Matrices[0]);
            DisplayMatrix("Unknown1", view.MatrixView1, mesh.Transformation.Matrices[1]);
            DisplayMatrix("Unknown2", view.MatrixView2, mesh.Transformation.Matrices[2]);
        }

        void DisplayMatrix(string name, Matrix3x4View view, FileMatrix3x4 matrix)
        {
            view.GroupBox.Header = name;
            view.Row0_0.Text = matrix.Matrix[0].X.ToString();
            view.Row0_1.Text = matrix.Matrix[0].Y.ToString();
            view.Row0_2.Text = matrix.Matrix[0].Z.ToString();
            view.Row0_3.Text = matrix.Matrix[0].W.ToString();

            view.Row1_0.Text = matrix.Matrix[1].X.ToString();
            view.Row1_1.Text = matrix.Matrix[1].Y.ToString();
            view.Row1_2.Text = matrix.Matrix[1].Z.ToString();
            view.Row1_3.Text = matrix.Matrix[1].W.ToString();

            view.Row2_0.Text = matrix.Matrix[2].X.ToString();
            view.Row2_1.Text = matrix.Matrix[2].Y.ToString();
            view.Row2_2.Text = matrix.Matrix[2].Z.ToString();
            view.Row2_3.Text = matrix.Matrix[2].W.ToString();
        }

        void CreateTextureDisplayItem(LodModel mesh, BrowsableItemView view, TexureType textureType)
        {
            view.LabelName.Width = 100;
            view.LabelName.Content = textureType.ToString();
            view.PathTextBox.Text = GetTextureName(mesh, textureType);
            view.CheckBox.Visibility = System.Windows.Visibility.Visible;
            if (view.PathTextBox.Text.Length != 0)
                view.CheckBox.IsChecked = true;

            view.PreviewButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            view.RemoveButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            view.BrowseButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            view.CheckBox.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
        }

        private void VisibleCheckBox_Click(RigidModelMeshEditorView editorView)
        {
            var model = _modelEditors[editorView];
            model.Visible = editorView.VisibleCheckBox.IsChecked == true;
        }

        void DisplayTexture(TexureType type, string path)
        {
            TexturePreviewController.Create(path, _world.TextureToTextureRenderer, _resourceLibary);
        }

        void AddUnknownTexture(RigidModelMeshEditorView view, LodModel model)
        {
            foreach (var item in model.Textures)
            {
                var isDefined = Enum.IsDefined(typeof(TexureType), item.TypeRaw);
                if (!isDefined)
                {
                    if (view.DebugInfo.Text.Length != 0)
                        view.DebugInfo.Text += "\n";

                    view.DebugInfo.Text += $"Unknown Texture Type: {item.TypeRaw} - {item.Name}"; 
                }
            }
        }

        void AddUnknowData(RigidModelMeshEditorView view, LodModel model)
        {
            
        }

        string GetTextureName(LodModel model, TexureType type)
        {
            foreach (var material in model.Textures)
            {
                if (material.Type == type)
                    return material.Name;
            }

            return "";
        }


        public void DrawNode(GraphicsDevice device, Microsoft.Xna.Framework.Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            foreach (var item in MeshInstances)
            {
                foreach (var item2 in item)
                    item2.Draw(device, parentTransform, commonShaderParameters);
            }
        }
    }
}
