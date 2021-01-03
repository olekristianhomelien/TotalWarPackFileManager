using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Viewer.Animation;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping
{
    public static class BoneMappingHelper
    {
        public static void AutomapDirectBoneLinksBasedOnNames(AdvBoneMappingBone boneToGetMapping, IEnumerable<AdvBoneMappingBone> externalBonesList)
        {
            var otherBone = FindBoneBasedOnName(boneToGetMapping.BoneName, externalBonesList);
            if (otherBone != null)
                boneToGetMapping.CreateDirectMapping(otherBone);

            foreach (var bone in boneToGetMapping.Children)
                AutomapDirectBoneLinksBasedOnNames(bone, externalBonesList);
        }

        public static void AutomapDirectBoneLinksBasedOnHierarchy(AdvBoneMappingBone boneToGetMapping, AdvBoneMappingBone otherBoneToStartFrom)
        {
            if (otherBoneToStartFrom == null)
            {
                MessageBox.Show("Error - No target selected");
                return;
            }
            boneToGetMapping.CreateDirectMapping(otherBoneToStartFrom);
            for (int i = 0; i < boneToGetMapping.Children.Count(); i++)
            {
                if (i < otherBoneToStartFrom.Children.Count())
                    AutomapDirectBoneLinksBasedOnHierarchy(boneToGetMapping.Children[i], otherBoneToStartFrom.Children[i]);
            }
        }

        public static AdvBoneMappingBone FindBoneBasedOnName(string name, IEnumerable<AdvBoneMappingBone> boneList)
        {
            foreach (var bone in boneList)
            {
                if (bone.BoneName == name)
                    return bone;

                var result = FindBoneBasedOnName(name, bone.Children);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static ObservableCollection<AdvBoneMappingBone> CreateSkeletonBoneList(GameSkeleton skeleton)
        {
            var outputList = new ObservableCollection<AdvBoneMappingBone>();

            if (skeleton != null)
            {
                for (int i = 0; i < skeleton.BoneCount; i++)
                {
                    int boneId = i;
                    int parentBoneId = skeleton.GetParentBone(i);
                    string boneName = skeleton.BoneNames[i];

                    AdvBoneMappingBone node = new AdvBoneMappingBone
                    {
                        BoneIndex = boneId,
                        BoneName = boneName,
                        ParentBoneIndex = parentBoneId,
                        DisplayName = $"{boneName} [{boneId}]",
                    };

                    var treeParent = GetParent(outputList, parentBoneId);
                    if (treeParent == null)
                        outputList.Add(node);
                    else
                        treeParent.Children.Add(node);
                }
            }

            return outputList;
        }

        public static AdvBoneMappingBone GetParent(ObservableCollection<AdvBoneMappingBone> root, int parentBoneId)
        {
            foreach (var item in root)
            {
                if (item.BoneIndex == parentBoneId)
                    return item;

                var result = GetParent(item.Children, parentBoneId);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static AdvBoneMappingBone GetBoneFromIndex(IEnumerable<AdvBoneMappingBone> root, int index)
        {
            foreach (var item in root)
            {
                if (item.BoneIndex == index)
                    return item;

                var result = GetBoneFromIndex(item.Children, index);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void ComputeBoneCount(IEnumerable<AdvBoneMappingBone> bones)
        {
            foreach (var bone in bones)
            {
                bone.ChildNodeCounts = bone.ComputeChildBoneCount();
                ComputeBoneCount(bone.Children);
            }
        }
    }
}
