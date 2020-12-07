using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonDialogs.Common.NotifyPropertyChangedImpl;
using static CommonDialogs.FilterDialog.FilterUserControl;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;
using Viewer.Scene;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class FilterableSkeletonsViewModel : NotifyPropertyChangedImpl
    {
        public AnimationFile SkeletonFile { get; set; }

        public List<PackedFile> SkeletonList { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }


        public event ValueChangedDelegate<PackedFile> SelectionChanged;
        PackedFile _selectedSkeleton;
        public PackedFile SelectedItem { get { return _selectedSkeleton; } set { SetAndNotify(ref _selectedSkeleton, value); OnSkeletonSelected(); } }


        public ObservableCollection<SkeletonBoneNode> SelectedSkeletonBonesFlattened { get; set; } = new ObservableCollection<SkeletonBoneNode>();

        public void FindAllSkeletons(ResourceLibary resourceLibary)
        {
            var allFilesInFolder = PackFileLoadHelper.GetAllFilesInDirectory(resourceLibary.PackfileContent, "animations\\skeletons");
            SkeletonList = allFilesInFolder.Where(x => x.FileExtention == "anim").ToList();
        }

        void OnSkeletonSelected()
        {
            SelectedSkeletonBonesFlattened.Clear();
            if (SelectedItem != null)
            {
                SkeletonFile = AnimationFile.Create(new ByteChunk(SelectedItem.Data));
                SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = -1, BoneName = "" });
                foreach (var bone in SkeletonFile.Bones)
                    SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = bone.Id, BoneName = bone.Name });
            }

            SelectionChanged?.Invoke(SelectedItem);
        }


    }
}
