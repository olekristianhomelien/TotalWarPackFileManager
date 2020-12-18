using CommonDialogs.Common;
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
using Viewer.Animation;
using VariantMeshEditor.Util;
using Microsoft.Xna.Framework;

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

        DoubleViewModel _rotationOffsetAlongPrimaryAxis = new DoubleViewModel();
        public DoubleViewModel RotationOffsetAlongPrimaryAxis
        {
            get { return _rotationOffsetAlongPrimaryAxis; }
            set { SetAndNotify(ref _rotationOffsetAlongPrimaryAxis, value); }
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

        private BoneCopyMethod _boneCopyMethod = BoneCopyMethod.Ratio;
        public BoneCopyMethod BoneCopyMethod
        {
            get { return _boneCopyMethod; }
            set { SetAndNotify(ref _boneCopyMethod, value); }
        }

        public bool _debugValue0 = false;
        public bool DebugValue0
        {
            get { return _debugValue0; }
            set { SetAndNotify(ref _debugValue0, value); }
        }

        public bool _debugValue1 = false;
        public bool DebugValue1
        {
            get { return _debugValue1; }
            set { SetAndNotify(ref _debugValue1, value); }
        }

        Vector4ViewModel _debugVector4 = new Vector4ViewModel();
        public Vector4ViewModel DebugVector4
        {
            get { return _debugVector4; }
            set { SetAndNotify(ref _debugVector4, value); }
        }


        public void SetCurrentInformation(int frame, AnimationClip animationClip)
        {
            if (animationClip != null && animationClip.DynamicFrames.Count > frame)
            {
                
                int boneIndex = OriginalBone.BoneIndex;
                /*animationClip.DynamicFrames[frame].Rotation[boneIndex].ToAxisAngle(out Vector3 axis, out float angle);
                DebugVector4.X.Value = (double)axis.X;
                DebugVector4.Y.Value = (double)axis.Y;
                DebugVector4.Z.Value = (double)axis.Z;
                DebugVector4.W.Value = (double)angle;*/

                var axis = animationClip.DynamicFrames[frame].Rotation[boneIndex].ToAxisAngleDegrees();
                DebugVector4.X.Value = (double)axis.X;
                DebugVector4.Y.Value = (double)axis.Y;
                DebugVector4.Z.Value = (double)axis.Z;
                DebugVector4.W.Value = (double)0;
            }
            else
            {
                DebugVector4.SetValue(-1);
            }

            foreach (var child in Children)
                child.SetCurrentInformation(frame, animationClip);
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
