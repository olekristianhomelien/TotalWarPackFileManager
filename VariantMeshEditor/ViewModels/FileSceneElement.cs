
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public enum FileSceneElementEnum
    {
        Root,
        Transform,
        Animation,
        Skeleton,
        VariantMesh,
        Slots,
        Slot,
        RigidModel,
        WsModel
    }

    public abstract class FileSceneElement : TreeViewDataModel, ISceneGraphNode
    {
        public FileSceneElement Parent { get; set; }
        public ObservableCollection<FileSceneElement> Children { get; private set; } = new ObservableCollection<FileSceneElement>();

        public abstract FileSceneElementEnum Type { get; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public virtual UserControl EditorViewModel { get; protected set; }

        public bool Visible { get; set; } = true;

        public FileSceneElement(FileSceneElement parent, string fileName, string fullPath, string displayName)
            : base(displayName)
        {
            FileName = fileName;
            FullPath = fullPath;
            DisplayName = displayName;
            Parent = parent;
        }

        public override string ToString() => DisplayName;

        public void CreateContent(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            CreateEditor(virtualWorld, resourceLibary);
            foreach (var child in Children)
            {
                child.CreateContent(virtualWorld, resourceLibary);
            }
            
        }

        protected virtual void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        { }


        public Matrix WorldTransform { get; set; } = Matrix.Identity;

        public void Render(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            if (Visible == false)
                return;
            DrawNode(device, parentTransform, commonShaderParameters);
            var newWorld = parentTransform * WorldTransform;
            foreach (var child in Children)
            {
                
                child.Render(device, newWorld, commonShaderParameters);
            }
        }

        virtual public void Update(GameTime time)
        {
            UpdateNode(time);
            foreach (var child in Children)
                child.Update(time);
        }

        virtual protected void UpdateNode(GameTime time)
        { }

        virtual protected void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        { }

        public FileSceneElement AddChild(FileSceneElement child)
        {
            child.Parent = child;
            Children.Add(child);
            return child;
        }

        public void RemoveNode(FileSceneElement node)
        {
            // Call destructor in children
            // call destructor on node
            

            this.Children.Remove(node);
        }
    }


    public class RootElement : FileSceneElement
    {
        public RootElement() : base(null, "", "", "Root") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Root;

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
         
            RootEditorView view = new RootEditorView();
            RootController controller = new RootController(view, this, resourceLibary, virtualWorld);
            EditorViewModel = view;
        }
    }



    public class VariantMeshElement : FileSceneElement
    {
        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;
    }

    public class WsModelElement : FileSceneElement
    {
        public WsModelElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            DisplayName = $"WsModel - {FileName}";
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.WsModel;
    }

}
