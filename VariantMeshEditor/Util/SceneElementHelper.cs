﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VariantMeshEditor.ViewModels;

namespace VariantMeshEditor.Util
{
    public class SceneElementHelper
    {
        public static List<T> GetAllOfTypeInSameVariantMesh<T>(FileSceneElement knownNode) where T : FileSceneElement
        {
            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
            {
                knownNode = knownNode.Parent;
                while (knownNode != null)
                {
                    if (knownNode.Type == FileSceneElementEnum.VariantMesh)
                        break;

                    knownNode = knownNode.Parent;
                }
            }

            if (knownNode == null || knownNode.Type != FileSceneElementEnum.VariantMesh)
                return new List<T>();

            var output = new List<T>();
            GetAllOfType(knownNode, ref output);
            return output;
        }

        static void GetAllOfType<T>(FileSceneElement root, ref List<T> outputList) where T : FileSceneElement
        {
            if (root as T != null)
                outputList.Add(root as T);

            foreach (var child in root.Children)
                GetAllOfType<T>(child, ref outputList);
        }

        public static void GetAllChildrenOfType<T>(FileSceneElement element, List<T> output) where T : FileSceneElement
        {
            if (element as T != null)
            {
                output.Add(element as T);
            }

            foreach (var child in element.Children)
            {
                GetAllChildrenOfType(child, output);
            }
        }

        public static T GetFirstChild<T>(FileSceneElement element) where T : FileSceneElement
        {
            if (element as T != null)
                return element as T;

            foreach (var child in element.Children)
            {
                var res =  GetFirstChild<T>(child);
                if (res != null)
                    return res;
            }
                

            return null;
        }

        public static FileSceneElement GetRoot(FileSceneElement item)
        {
            if (item.Parent == null)
                return item;
            return GetRoot(item.Parent);
        }



        public static FileSceneElement GetTopNode(FileSceneElement item)
        {
            var isParentRoot = IsParentRoot(item);
            if (isParentRoot)
                return item;

            return GetTopNode(item.Parent);
        }


        public static bool IsParentRoot(FileSceneElement item)
        {
            if (item.Parent == null)
                return true;
            return false;

        }
    }
}
