using CommonDialogs.Common;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VariantMeshEditor.ViewModels.Skeleton
{
    public class SkeletonViewModel : NotifyPropertyChangedImpl
    {
        SkeletonElement _skeletonElement;


        string _skeltonName;
        public string SkeletonName
        {
            get { return _skeltonName; }
            set { SetAndNotify(ref _skeltonName, value); }
        }


        int _boneCount = 0;
        public int BoneCount
        {
            get { return _boneCount; }
            set { SetAndNotify(ref _boneCount, value); }
        }


        public ObservableCollection<SkeletonBoneNode> Bones { get; set; } = new ObservableCollection<SkeletonBoneNode>();

        public SkeletonBoneNode _selectedBone;
        public SkeletonBoneNode SelectedBone
        {
            get { return _selectedBone; }
            set { SetAndNotify(ref _selectedBone, value); }
        }

        public SkeletonViewModel(SkeletonElement skeletonElement)
        {
            _skeletonElement = skeletonElement;
            CreateBoneOverview();
        }


        void CreateBoneOverview()
        {
            SelectedBone = null;
            Bones.Clear();

            SkeletonName = _skeletonElement.FileName;

            foreach (var bone in _skeletonElement.SkeletonFile.Bones)
            {
                if (bone.ParentId == -1)
                {
                    Bones.Add(CreateNode(bone));
                }
                else
                {
                    var parentBone = _skeletonElement.SkeletonFile.Bones[bone.ParentId];
                    var treeParent = GetParent(Bones, parentBone);

                    if (treeParent != null)
                        treeParent.Children.Add(CreateNode(bone));
                }
            }

            BoneCount = _skeletonElement.SkeletonFile.Bones.Count();
        }

        SkeletonBoneNode CreateNode(AnimationFile.BoneInfo bone)
        {
            SkeletonBoneNode item = new SkeletonBoneNode
            {
                BoneIndex = bone.Id,
                BoneName = bone.Name// + " [" + bone.Id + "]" + " P[" + bone.ParentId + "]",
            };
            return item;
        }

        SkeletonBoneNode GetParent(ObservableCollection<SkeletonBoneNode> root, AnimationFile.BoneInfo parentBone)
        {
            foreach (SkeletonBoneNode item in root)
            {
                if (item.BoneIndex == parentBone.Id)
                    return item;

                var result = GetParent(item.Children, parentBone);
                if (result != null)
                    return result;
            }
            return null;
        }


        public class SkeletonBoneNode : NotifyPropertyChangedImpl
        {
            string _boneName;
            public string BoneName
            {
                get { return _boneName; }
                set { SetAndNotify(ref _boneName, value); }
            }

            int _boneIndex;
            public int BoneIndex
            {
                get { return _boneIndex; }
                set { SetAndNotify(ref _boneIndex, value); }
            }

            public ObservableCollection<SkeletonBoneNode> Children { get; set; } = new ObservableCollection<SkeletonBoneNode>();
        }
    }
}
