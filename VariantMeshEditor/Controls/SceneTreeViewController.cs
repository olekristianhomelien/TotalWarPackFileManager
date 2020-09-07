﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;

namespace VariantMeshEditor.Controls
{
    class SceneTreeViewController
    {
        public delegate void SceneElementSelectedEventHandler(FileSceneElement element);
        public event SceneElementSelectedEventHandler SceneElementSelectedEvent;
        public delegate void VisabilityChangedEvntHandler(FileSceneElement element, bool isVisible);
        public event VisabilityChangedEvntHandler VisabilityChangedEvent;

        TreeView _viewModel;
        public SceneTreeViewController(TreeView viewModel)
        {
            _viewModel = viewModel;
            _viewModel.SelectedItemChanged += _viewModel_SelectedItemChanged;
        }

        public List<T> GetAllOfTypeInSameVariantMesh<T>(FileSceneElement knownNode) where T : FileSceneElement
        {
            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
            {
                knownNode = knownNode.Parent;
                while (knownNode != null )
                {
                    if (knownNode.Type == FileSceneElementEnum.VariantMesh)
                        break;

                    knownNode = knownNode.Parent;
                }
            }

            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
                return new List<T>();

            var output = new List<T>();
            GetAllOfType(knownNode, ref output);
            return output;
        }


        void GetAllOfType<T>(FileSceneElement root, ref List<T> outputList) where T : FileSceneElement
        {
            if (root as T != null)
                outputList.Add(root as T);

            foreach (var child in root.Children)
                GetAllOfType<T>(child, ref outputList);
        }


        public void Populate(FileSceneElement rootItem)
        {
            SetInitialVisability(rootItem, true);
            _viewModel.DataContext = new ObservableCollection<TreeViewDataModel>() { rootItem }; ;
        }

        public void SetInitialVisability(FileSceneElement scene, bool shouldBeSelected, TreeViewDataModel parent = null)
        {

            scene.IsChecked = shouldBeSelected;

            scene.PropertyChanged += Node_PropertyChanged;

            if (scene as TransformElement != null)
                scene.Vis = Visibility.Hidden;
            if (scene as AnimationElement != null)
                scene.Vis = Visibility.Hidden;
            if (scene as SkeletonElement != null)
                scene.IsChecked = false;

            bool areAllChildrenModels = scene.Children.Where(x => (x as RigidModelElement) != null).Count() == scene.Children.Count();
            bool firstItem = true;
            foreach (var item in scene.Children)
            {
                if (areAllChildrenModels && !firstItem)
                    shouldBeSelected = false;

                firstItem = false;
                SetInitialVisability(item, shouldBeSelected, null);
            }
        }

        private void _viewModel_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewDataModel selectedItem = e.NewValue as TreeViewDataModel;
            SceneElementSelectedEvent?.Invoke(selectedItem as FileSceneElement);
        }

        bool _updatingCheckedStatus = false;
        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_updatingCheckedStatus == true)
                return;
            _updatingCheckedStatus = true;
            if (e.PropertyName == "IsChecked")
            {
                var fileSceneElement = sender as FileSceneElement;

                if (fileSceneElement.Type == FileSceneElementEnum.Skeleton)
                {
                    VisabilityChangedEvent?.Invoke(fileSceneElement as FileSceneElement, fileSceneElement.IsChecked);
                }

                if (fileSceneElement.IsChecked)
                {
                    if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                    {
                        var parentChildren = fileSceneElement.Parent.Children;
                        foreach (var child in parentChildren)
                        {
                            child.IsChecked = false;
                            VisabilityChangedEvent?.Invoke(child as FileSceneElement, child.IsChecked);
                        }
                        fileSceneElement.IsChecked = true;

                        VisabilityChangedEvent?.Invoke(fileSceneElement as FileSceneElement, fileSceneElement.IsChecked);

                        var parent = fileSceneElement.Parent;
                        while (parent != null)
                        {
                            parent.IsChecked = true;
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        SetChildrenVisability(fileSceneElement, true);
                    }
                }
                else
                {
                    if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                    {
                        VisabilityChangedEvent?.Invoke(fileSceneElement as FileSceneElement, fileSceneElement.IsChecked);
                    }
                    else
                    {
                        SetChildrenVisability(fileSceneElement, false);
                    }
                }
            }

            _updatingCheckedStatus = false;
        }

        void SetChildrenVisability(FileSceneElement root, bool isVisible)
        {
            foreach (var treeItem in root.Children)
            {
                var fileSceneElement = treeItem as FileSceneElement;
                if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                {
                    if (isVisible)
                        VisabilityChangedEvent?.Invoke(treeItem as FileSceneElement, treeItem.IsChecked);
                    else
                        VisabilityChangedEvent?.Invoke(treeItem as FileSceneElement, false);
                }

                if (fileSceneElement.Type == FileSceneElementEnum.Slot && treeItem.IsChecked == false)
                {
                }
                else if (fileSceneElement.Type == FileSceneElementEnum.VariantMesh && treeItem.IsChecked == false)
                {
                }
                else
                {
                    SetChildrenVisability(treeItem, isVisible);
                }
                
            }
        }
    }
}