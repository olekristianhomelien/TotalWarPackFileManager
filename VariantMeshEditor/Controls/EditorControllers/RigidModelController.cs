using Filetypes.RigidModel;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VariantMeshEditor.ViewModels.RigidModel;
using VariantMeshEditor.Views.EditorViews.RigidBodyEditor;
using VariantMeshEditor.Views.EditorViews.Util;
using VariantMeshEditor.Views.TexturePreview;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.Controls.EditorControllers
{
    public class RigidModelController
    {
        ResourceLibary _resourceLibary;
        Scene3d _world;
        Dictionary<RigidModelMeshEditorView, MeshRenderItem> _modelEditors = new Dictionary<RigidModelMeshEditorView, MeshRenderItem>();

        public RigidModelController(RigidModelElement element, ResourceLibary resourceLibary, Scene3d world)
        {
            _resourceLibary = resourceLibary;
            _world = world;
 
        }


        void DisplayTransforms(Rmv2LodModel mesh, RigidModelMeshEditorView view)
        {
          //  view.PivotView.GroupBox.Header = "Pivot";
          //  view.PivotView.Row0_0.Text = mesh.Transformation.Pivot.X.ToString();
          //  view.PivotView.Row0_1.Text = mesh.Transformation.Pivot.Y.ToString();
          //  view.PivotView.Row0_2.Text = mesh.Transformation.Pivot.Z.ToString();
          //  DisplayMatrix("Unknown0", view.MatrixView0, mesh.Transformation.Matrices[0]);
          //  DisplayMatrix("Unknown1", view.MatrixView1, mesh.Transformation.Matrices[1]);
          //  DisplayMatrix("Unknown2", view.MatrixView2, mesh.Transformation.Matrices[2]);
        }

        void DisplayMatrix(string name, Matrix3x4View view, FileMatrix3x4 matrix)
        {
            //view.GroupBox.Header = name;
            //view.Row0_0.Text = matrix.Matrix[0].X.ToString();
            //view.Row0_1.Text = matrix.Matrix[0].Y.ToString();
            //view.Row0_2.Text = matrix.Matrix[0].Z.ToString();
            //view.Row0_3.Text = matrix.Matrix[0].W.ToString();
            //
            //view.Row1_0.Text = matrix.Matrix[1].X.ToString();
            //view.Row1_1.Text = matrix.Matrix[1].Y.ToString();
            //view.Row1_2.Text = matrix.Matrix[1].Z.ToString();
            //view.Row1_3.Text = matrix.Matrix[1].W.ToString();
            //
            //view.Row2_0.Text = matrix.Matrix[2].X.ToString();
            //view.Row2_1.Text = matrix.Matrix[2].Y.ToString();
            //view.Row2_2.Text = matrix.Matrix[2].Z.ToString();
            //view.Row2_3.Text = matrix.Matrix[2].W.ToString();
        }

        void CreateTextureDisplayItem(Rmv2LodModel mesh, BrowsableItemView view, TexureType textureType)
        {
            //view.LabelName.Width = 100;
            //view.LabelName.Content = textureType.ToString();
            //view.PathTextBox.Text = GetTextureName(mesh, textureType);
            //view.CheckBox.Visibility = System.Windows.Visibility.Visible;
            //if (view.PathTextBox.Text.Length != 0)
            //    view.CheckBox.IsChecked = true;
            //
            //view.PreviewButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            //view.RemoveButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            //view.BrowseButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            //view.CheckBox.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
        }

        private void VisibleCheckBox_Click(RigidModelMeshEditorView editorView)
        {
            var model = _modelEditors[editorView];
           // model.Visible = editorView.VisibleCheckBox.IsChecked == true;
        }

        void DisplayTexture(TexureType type, string path)
        {
            TexturePreviewController.Create(path, _world.TextureToTextureRenderer, _resourceLibary);
        }

        void AddUnknownTexture(RigidModelMeshEditorView view, Rmv2LodModel model)
        {
           // foreach (var item in model.Textures)
           // {
           //     var isDefined = Enum.IsDefined(typeof(TexureType), item.TypeRaw);
           //     if (!isDefined)
           //     {
           //         if (view.DebugInfo.Text.Length != 0)
           //             view.DebugInfo.Text += "\n";
           //
           //         view.DebugInfo.Text += $"Unknown Texture Type: {item.TypeRaw} - {item.Name}"; 
           //     }
           // }
        }

        void AddUnknowData(RigidModelMeshEditorView view, Rmv2LodModel model)
        {
            
        }

        string GetTextureName(Rmv2LodModel model, TexureType type)
        {
            foreach (var material in model.Textures)
            {
                if (material.Type == type)
                    return material.Name;
            }

            return "";
        }
    }
}
