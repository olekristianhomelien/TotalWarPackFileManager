using Common;
using CommonDialogs.Common;
using CommonDialogs.MathViews;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VariantMeshEditor.Services;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;
using static CommonDialogs.FilterDialog.FilterUserControl;
using static Filetypes.RigidModel.AnimationFile;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.ViewModels.Animation
{


    public  class FilterableAnimationsViewModel : NotifyPropertyChangedImpl
    {
        List<PackedFile> _filterList;

        public List<PackedFile> CurrentItems { get { return _filterList; } set { SetAndNotify(ref _filterList, value); } }
        List<PackedFile> AllAnimations { get; set; } = new List<PackedFile>();
        List<PackedFile> AnimationsForCurrentSkeleton { get; set; } = new List<PackedFile>();
        public OnSeachDelegate FilterItemOnSearch { get { return (item, expression) => { return expression.Match((item as PackedFile).FullPath).Success; }; } }

        public event ValueChangedDelegate<PackedFile> SelectionChanged;
        PackedFile _selectedAnimation;
        public PackedFile SelectedItem { get { return _selectedAnimation; } set { SetAndNotify(ref _selectedAnimation, value, SelectionChanged); } }

        bool _onlyDisplayAnimationsForCurrentSkeleton = true;
        public bool OnlyDisplayAnimationsForCurrentSkeleton
        {
            get { return _onlyDisplayAnimationsForCurrentSkeleton; }
            set
            {
                SetAndNotify(ref _onlyDisplayAnimationsForCurrentSkeleton, value);
                if (value)
                    CurrentItems = AnimationsForCurrentSkeleton;
                else
                    CurrentItems = AllAnimations;
            }
        }

        public void FindAllAnimations(ResourceLibary resourceLibary, string skeletonName)
        {
            AllAnimations = PackFileLoadHelper.GetAllWithExtention(resourceLibary.PackfileContent, "anim");
            AnimationsForCurrentSkeleton.Clear();

            foreach (var animation in AllAnimations)
            {
                var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(animation.Data)).SkeletonName;
                if (animationSkeletonName == skeletonName)
                    AnimationsForCurrentSkeleton.Add(animation);
            }

            OnlyDisplayAnimationsForCurrentSkeleton = true;
        }
    }


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
                SkeletonFile = Create(new ByteChunk(SelectedItem.Data));
                SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = -1, BoneName = ""});
                foreach (var bone in SkeletonFile.Bones)
                    SelectedSkeletonBonesFlattened.Add(new SkeletonBoneNode { BoneIndex = bone.Id, BoneName = bone.Name });
            }

            SelectionChanged?.Invoke(SelectedItem);
        }

  
    }

    public class ExternalSkeletonViewModel : NotifyPropertyChangedImpl
    {
        Vector3ViewModel _skeletonOffset = new Vector3ViewModel(3,0,0);
        public Vector3ViewModel SkeletonOffset
        {
            get { return _skeletonOffset; }
            set { SetAndNotify(ref _skeletonOffset, value); }
        }

        bool _drawSkeleton = true;
        public bool DrawSkeleton
        {
            get { return _drawSkeleton; }
            set { SetAndNotify(ref _drawSkeleton, value); }
        }

        bool _drawLine = false;
        public bool DrawLine
        {
            get { return _drawLine; }
            set { SetAndNotify(ref _drawLine, value); }
        }

        SkeletonElement _externalElement;
        AnimationPlayer _animationPlayer;
        public void Create(ResourceLibary resourceLibary, string skeletonName)
        {
            if (_externalElement != null)
                _externalElement.Dispose();

            _externalElement = new SkeletonElement(null, "");
            _externalElement.IsChecked = true;
            _animationPlayer = new AnimationPlayer();
            _externalElement.Create(_animationPlayer, resourceLibary, skeletonName);
        }

        public void SetSelectedBone(int index)
        {
            if (_externalElement != null)
            {
                _externalElement.ViewModel.SelectedBone = _externalElement.ViewModel.GetBoneFromIndex(index, _externalElement.ViewModel.Bones);
            }
        }


        public void SetFrame(int currentFrame)
        {
            _animationPlayer.CurrentFrame = currentFrame;
        }

        public void SetAnimation(AnimationClip clip)
        {
            _animationPlayer.SetAnimation(clip, _externalElement.Skeleton);
        }

        public void UpdateNode(GameTime time)
        {
            _externalElement?.Update(time);
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            var matrix = Matrix.CreateTranslation((float)_skeletonOffset.X.Value, (float)_skeletonOffset.Y.Value, (float)_skeletonOffset.Z.Value);
            _externalElement?.Render(device, matrix, commonShaderParameters);
        }
    }

    public class AnimationSplicerViewModel : NotifyPropertyChangedImpl
    {
        ResourceLibary _resourceLibary;
        SkeletonElement _targetSkeletonNode;
        AnimationPlayerViewModel _animationPlayer;

        bool _isSelected;
        public bool IsSelected { get { return _isSelected; } set { SetAndNotify(ref _isSelected, value); IsInFocus(IsSelected); } }

        public FilterableAnimationsViewModel TargetAnimation { get; set; } = new FilterableAnimationsViewModel();
        public FilterableSkeletonsViewModel ExternalSkeleton { get; set; } = new FilterableSkeletonsViewModel();
        public FilterableAnimationsViewModel ExternalAnimation { get; set; } = new FilterableAnimationsViewModel();

        public ExternalSkeletonViewModel ExternalSkeletonSettings { get; set; } = new ExternalSkeletonViewModel();

        ObservableCollection<MappableSkeletonBone> _targetSkeletonBones;
        public ObservableCollection<MappableSkeletonBone> TargetSkeletonBones { get { return _targetSkeletonBones; } set{ SetAndNotify(ref _targetSkeletonBones, value);}}


        public ICommand ForceComputeCommand { get; set; }
        public MappableSkeletonBone _selectedNode;
        public MappableSkeletonBone SelectedNode {get { return _selectedNode; }set { SetAndNotify(ref _selectedNode, value); OnItemSelected(_selectedNode); } }


        void OnItemSelected(MappableSkeletonBone bone)
        {
            _targetSkeletonNode.ViewModel.SelectedBone = bone.OriginalBone;

            if(bone != null && bone.MappedBone != null)
                ExternalSkeletonSettings.SetSelectedBone(bone.MappedBone.BoneIndex);
            else
                ExternalSkeletonSettings.SetSelectedBone(-1);
        }


        public AnimationSplicerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer)
        {
            _resourceLibary = resourceLibary;
            _targetSkeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;

            TargetAnimation.SelectionChanged += x => Rebuild();
            ExternalSkeleton.SelectionChanged += PossibleExternalSkeletons_SelectionChanged;
            ExternalAnimation.SelectionChanged += ExternalAnimation_SelectionChanged;

            // Initial init
            TargetAnimation.FindAllAnimations(_resourceLibary, _targetSkeletonNode.SkeletonFile.Header.SkeletonName);
            ExternalSkeleton.FindAllSkeletons(_resourceLibary);
            TargetSkeletonBones = MappableSkeletonBone.Create(_targetSkeletonNode);

            SelectedNode = TargetSkeletonBones.FirstOrDefault();

            ForceComputeCommand = new RelayCommand(Rebuild);

            // Temp - Populate with debug data. 
            //TargetAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
            //ExternalSkeleton.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid07.anim");
            //ExternalAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid07\club_and_blowpipe\missile_actions\hu7_clbp_aim_idle_01.anim");

            TargetAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
            ExternalSkeleton.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid01b.anim");
            ExternalAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01b\subset\spellsinger\sword\stand\hu1b_elf_spellsinger_sw_stand_idle_01.anim");
        }

        private void ExternalAnimation_SelectionChanged(PackedFile newSelectedSkeleton)
        {
            Rebuild();
            if (newSelectedSkeleton != null)
            {
                var externalAnim = AnimationFile.Create(new ByteChunk(newSelectedSkeleton.Data));
                var externalAnimationClip = new AnimationClip(externalAnim);

                //var externalSkeletonFile = AnimationFile.Create(new ByteChunk(ExternalSkeleton.SelectedItem.Data));
                //Viewer.Animation.Skeleton externalSkeleton = new Viewer.Animation.Skeleton(externalSkeletonFile);
                ExternalSkeletonSettings.SetAnimation(externalAnimationClip);
            }
        }

        void Rebuild()
        {
            try
            {
                AnimationBuilderService.AnimationBuilderSettings settings = new AnimationBuilderService.AnimationBuilderSettings()
                {
                    ExternalAnimationFile = ExternalAnimation.SelectedItem,
                    ExternalSkeletonFile = ExternalSkeleton.SelectedItem,

                    TargetSkeletonBones = TargetSkeletonBones,
                    SourceSkeleton = _targetSkeletonNode.Skeleton,
                    SourceAnimationFile = TargetAnimation.SelectedItem
                };

                AnimationBuilderService builder = new AnimationBuilderService();
                var animation = builder.CreateMergedAnimation(settings);

                if (animation != null)
                    _animationPlayer.SetAnimationClip(new List<AnimationClip>() { animation }, _targetSkeletonNode.Skeleton);
                else
                    _animationPlayer.SetAnimationClip(null, _targetSkeletonNode.Skeleton);
            }
            catch (Exception e)
            { 
            
            }
        }

        private void PossibleExternalSkeletons_SelectionChanged(PackedFile newSelectedSkeleton)
        {
            ExternalAnimation.SelectedItem = null;
            if (ExternalSkeleton.SelectedItem != null)
            {
                ExternalAnimation.FindAllAnimations(_resourceLibary, ExternalSkeleton.SkeletonFile.Header.SkeletonName);
                PrefilBoneMappingBasedOnName(TargetSkeletonBones, ExternalSkeleton.SelectedSkeletonBonesFlattened);

                ExternalSkeletonSettings.Create(_resourceLibary, newSelectedSkeleton.Name);
            }
        }



        void ExternalSkeletonSelected()
        {
            ExternalAnimation.SelectedItem = null;
        }

        void PrefilBoneMappingBasedOnName(IEnumerable<MappableSkeletonBone> nodes, ObservableCollection<SkeletonBoneNode> externalBonesList)
        {
            foreach(var node in nodes)
            {
                if (node != null)
                {
                    node.MappedBone = externalBonesList.FirstOrDefault(x => x?.BoneName == node.OriginalBone.BoneName);
                    PrefilBoneMappingBasedOnName(node.Children, externalBonesList);
                }
            }
        }

        void ApplyCurrentAnimation()
        {
        
        }

        void IsInFocus(bool isInFocus)
        {



        }

        public void UpdateNode(GameTime time)
        {
            ExternalSkeletonSettings?.UpdateNode(time);
            ExternalSkeletonSettings.SetFrame(_animationPlayer.CurrentFrame);
            
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            ExternalSkeletonSettings?.DrawNode(device, parentTransform, commonShaderParameters);
        }

    }


   



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


        public ICommand RestoreDefaultMapping { get; set; }


        public static ObservableCollection<MappableSkeletonBone> Create(SkeletonElement skeletonNode)
        {
            var output = new ObservableCollection<MappableSkeletonBone>();
            foreach (var bone in skeletonNode.ViewModel.Bones)
                RecuseiveCreate(bone, output);
            return output;
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
