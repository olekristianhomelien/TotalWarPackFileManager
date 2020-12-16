﻿using CommonDialogs.Common;
using CommonDialogs.MathViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.Services;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class MappableSkeletonBone : NotifyPropertyChangedImpl
    {
        public SkeletonBoneNode OriginalBone { get; set; }
        public ObservableCollection<MappableSkeletonBone> Children { get; set; } = new ObservableCollection<MappableSkeletonBone>();

        private bool _useContantOffset = true;
        public bool UseConstantOffset
        {
            get { return _useContantOffset; }
            set { SetAndNotify(ref _useContantOffset, value); }
        }

        Vector3ViewModel _contantTranslationOffset = new Vector3ViewModel();
        public Vector3ViewModel ContantTranslationOffset
        {
            get { return _contantTranslationOffset; }
            set { SetAndNotify(ref _contantTranslationOffset, value); }
        }

        Vector3ViewModel _contantRotationOffset = new Vector3ViewModel();
        public Vector3ViewModel ContantRotationOffset
        {
            get { return _contantRotationOffset; }
            set { SetAndNotify(ref _contantRotationOffset, value); }
        }

        private bool _useMapping = true;
        public bool UseMapping
        {
            get { return _useMapping; }
            set { SetAndNotify(ref _useMapping, value); }
        }

        SkeletonBoneNode _mappedBone;
        public SkeletonBoneNode MappedBone { get { return _mappedBone; } set { SetAndNotify(ref _mappedBone, value); } }


        private TransformTypes _transformTypesToCopy = TransformTypes.Both;
        public TransformTypes TransformTypesToCopy
        {
            get { return _transformTypesToCopy; }
            set { SetAndNotify(ref _transformTypesToCopy, value); }
        }

        private BoneCopyMethod _boneCopyMethod = BoneCopyMethod.Relative;
        public BoneCopyMethod BoneCopyMethod
        {
            get { return _boneCopyMethod; }
            set { SetAndNotify(ref _boneCopyMethod, value); }
        }
    }

    public class MappableSkeletonBoneHelper
    {
        public static ObservableCollection<MappableSkeletonBone> Create(SkeletonElement skeletonNode)
        {
            var output = new ObservableCollection<MappableSkeletonBone>();
            foreach (var bone in skeletonNode.ViewModel.Bones)
                RecuseiveCreate(bone, output);
            return output;
        }

        public static void SetDefaultBoneCopyMethod(MappableSkeletonBone node, BoneCopyMethod value)
        {
            node.BoneCopyMethod = value;
            foreach (var child in node.Children)
                SetDefaultBoneCopyMethod(child, value);
        }


        static void RecuseiveCreate(SkeletonBoneNode bone, ObservableCollection<MappableSkeletonBone> outputList)
        {
            if (bone.ParentBoneIndex == -1)
            {
                outputList.Add(CreateNode(bone));
            }
            else
            {
                var treeParent = GetParent(outputList, bone.ParentBoneIndex);

                if (treeParent != null)
                    treeParent.Children.Add(CreateNode(bone));
            }


            foreach (var item in bone.Children)
            {
                RecuseiveCreate(item, outputList);
            }
        }

        static MappableSkeletonBone CreateNode(SkeletonBoneNode bone)
        {
            MappableSkeletonBone item = new MappableSkeletonBone()
            {
                OriginalBone = bone
            };
            return item;
        }

        static MappableSkeletonBone GetParent(ObservableCollection<MappableSkeletonBone> root, int parentBoneIndex)
        {
            foreach (MappableSkeletonBone item in root)
            {
                if (item.OriginalBone.BoneIndex == parentBoneIndex)
                    return item;

                var result = GetParent(item.Children, parentBoneIndex);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
