using CommonDialogs.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

    public abstract class FileSceneElement : NotifyPropertyChangedImpl, ISceneGraphNode, IDisposable
    {
        public Visibility ApplyElementCheckboxVisability { get; set; } = Visibility.Visible;


        string _displayName = "";
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (ApplyElementCheckboxVisability == Visibility.Visible)
                {
                    _isChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        bool _isExpanded = true;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyPropertyChanged();
            }
        }

        string _checkBoxGroupingName = "";
        public string CheckBoxGroupingName
        {
            get { return _checkBoxGroupingName; }
            set
            {
                _checkBoxGroupingName = value;
                NotifyPropertyChanged();
            }
        }

        public FileSceneElement Parent { get; set; }
        public ObservableCollection<FileSceneElement> Children { get; set; } = new ObservableCollection<FileSceneElement>();

        public abstract FileSceneElementEnum Type { get; }
        public string FileName { get; set; }
        public string FullPath { get; set; }



        public FileSceneElement(FileSceneElement parent, string fileName, string fullPath, string displayName)
            : base()
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
            if (IsChecked == false)
                return;

            DrawNode(device, parentTransform, commonShaderParameters);
            foreach (var child in Children)
                child.Render(device, parentTransform * WorldTransform, commonShaderParameters);
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
            child.Parent = this;
            this.Children.Add(child);
            return child;
        }

        public virtual void Dispose()
        {
            foreach (var child in Children)
                child.Dispose();
        }
    }
}
