using CommonDialogs.Common;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Skeleton;

namespace VariantMeshEditor.ViewModels.VariantMesh
{
    public class VariantMeshViewModel : NotifyPropertyChangedImpl
    {
        VariantMeshElement _parent;

        public ICommand PopulateBoneListCommand { get; set; }
        public ICommand PopulateMeshListCommand { get; set; }

        public ObservableCollection<FileSceneElement> MeshList { get; set; } = new ObservableCollection<FileSceneElement>();

        VariantMeshElement _selectedMesh;
        public VariantMeshElement SelectedMesh { get { return _selectedMesh; } set { SetAndNotify(ref _selectedMesh, value); } }


        AnimationFile.BoneInfo _selectedBone;
        public AnimationFile.BoneInfo SelectedBone { get { return _selectedBone; } set { SetAndNotify(ref _selectedBone, value); } }
        

        public ObservableCollection<AnimationFile.BoneInfo> BoneList { get; set; } = new ObservableCollection<AnimationFile.BoneInfo>();

        public VariantMeshViewModel(VariantMeshElement parent)
        {
            _parent = parent;
            PopulateBoneListCommand = new RelayCommand(PopulateBoneList);
            PopulateMeshListCommand = new RelayCommand(PopulateMeshList);
        }

        void PopulateBoneList()
        {
            BoneList.Clear();
            if (_selectedMesh != null)
            {
                var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(SelectedMesh);
                if (skeleton != null)
                {
                    foreach (var bone in skeleton?.SkeletonFile.Bones)
                        BoneList.Add(bone);
                }
            }
        }

        void PopulateMeshList()
        {
            var root = SceneElementHelper.GetRoot(_parent);
            MeshList.Clear();
            foreach (var item in root.Children)
            {
                if(item != _parent)
                    MeshList.Add(item);
            }

            SelectedBone = null;
        }
    }
}
