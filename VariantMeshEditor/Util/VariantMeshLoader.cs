using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.RigidModel;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.ViewModels.VariantMesh;
using Viewer.Scene;
using static Filetypes.RigidModel.VariantMeshDefinition;

namespace VariantMeshEditor.Util
{
    class SceneLoader
    {
        ResourceLibary _resourceLibary;

        public SceneLoader( ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
        }

        public FileSceneElement Load(PackedFile file, FileSceneElement parent)
        {
            switch (file.FileExtention)
            {
                case "variantmeshdefinition":
                     LoadVariantMesh(file, ref parent);
                    break;

                case "rigid_model_v2":
                    LoadRigidMesh(file, ref parent);
                    break;

                case "wsmodel":
                    LoadWsModel(file, ref parent);
                    break;
            }

            return parent;
        }

        FileSceneElement Load(string filePath, FileSceneElement parent)
        {
            var file = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, filePath);
            return Load(file, parent);
        }

        void LoadVariantMesh(PackedFile file, ref FileSceneElement parent)
        {
            var variantMeshElement = new VariantMeshElement(parent,file.Name);
            if (parent == null)
            {
                parent = variantMeshElement;
            }
            else
            {
                if (parent.Children.Count == 0)
                    variantMeshElement.IsChecked = true;
                parent.Children.Add(variantMeshElement);
            }

            AnimationElement animationElement = null;
            SkeletonElement skeletonElement = null;
            if (parent.Parent == null)
            {
                animationElement = new AnimationElement(variantMeshElement);
                variantMeshElement.Children.Add(animationElement);
                skeletonElement = new SkeletonElement(variantMeshElement, "");
                variantMeshElement.Children.Add(skeletonElement);
            }

            var slotsElement = new SlotsElement(variantMeshElement);
            variantMeshElement.Children.Add(slotsElement);
            slotsElement.IsChecked = true;

            var content = file.Data;
            var fileContent = Encoding.Default.GetString(content);
            VariantMeshFile meshFile = VariantMeshDefinition.Create(fileContent);

            foreach (var slot in meshFile.VARIANT_MESH.SLOT)
            {
                var slotElement = new SlotElement(slotsElement, slot.Name, slot.AttachPoint);
                slotsElement.Children.Add(slotElement);
                //slotElement.IsChecked = true;

                foreach (var mesh in slot.VariantMeshes)
                {
                    if(mesh.Name != null)
                        Load(mesh.Name, slotElement);
                }

                foreach (var meshReference in slot.VariantMeshReferences)
                    Load(meshReference.definition, slotElement);
            }

            // Load the animation
            if (parent.Parent == null)
            {
                var rigidModels = new List<RigidModelElement>();

                GetAllOfType<RigidModelElement>(parent, ref rigidModels);
                var skeletons = rigidModels
                    .Where(x => !string.IsNullOrEmpty(x.Model.BaseSkeleton))
                    .Select(x => x.Model.BaseSkeleton)
                    .Distinct()
                    .ToList();

                if (skeletons.Count() > 1)
                    throw new Exception("More the one skeleton for a veriant mesh");
                if (skeletons.Count() == 1)
                {
                    string animationFolder = "animations\\skeletons\\";
                    var skeletonFilePath = animationFolder + skeletons.First() + ".anim";
                    var skeletonFile = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, skeletonFilePath);
                    skeletonElement.Create(animationElement.AnimationPlayer, _resourceLibary, skeletonFile);
                }
                else
                {
                    variantMeshElement.Children.Remove(animationElement);
                    variantMeshElement.Children.Remove(skeletonElement);
                }
            }
        }

        void LoadRigidMesh(PackedFile file, ref FileSceneElement parent)
        {
            var model3d = Rmv2RigidModel.Create(file);
            var model = new RigidModelElement(parent, model3d, file.FullPath);
            if (parent == null)
            {
                parent = model;
            }
            else
            {
                if (parent.Children.Count == 0)
                    model.IsChecked = true;
                parent.Children.Add(model);
            }
        }

        void LoadWsModel(PackedFile file, ref FileSceneElement parent)
        {
            var model = new WsModelElement(parent,file.FullPath);
            if (parent == null)
            {
                parent = model;
            }
            else
            {
                if (parent.Children.Count == 0)
                    model.IsChecked = true;
                parent.Children.Add(model);
            }

            var buffer = file.Data;
            string s = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);

            var nodes = doc.SelectNodes(@"/model/geometry");
            foreach (XmlNode node in nodes)
            {
                var file2 = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, node.InnerText);
                var modelAsBase = model as FileSceneElement;
                LoadRigidMesh(file2,  ref modelAsBase);
                model = modelAsBase as WsModelElement;
            }
        }

        void GetAllOfType<T>(FileSceneElement variantMeshParent, ref List<T> out_items) where T : FileSceneElement
        {
            if (variantMeshParent as T != null)
                out_items.Add(variantMeshParent as T);

            foreach (var child in variantMeshParent.Children)
            {
                if (variantMeshParent as T != null)
                    out_items.Add(variantMeshParent as T);

                GetAllOfType(child, ref out_items);
            }
        }
    }

}
