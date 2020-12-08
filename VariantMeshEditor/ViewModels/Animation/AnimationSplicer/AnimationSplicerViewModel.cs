using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using VariantMeshEditor.Services;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Scene;
using WpfTest.Scenes;
using static VariantMeshEditor.ViewModels.Skeleton.SkeletonViewModel;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class AnimationSplicerViewModel : NotifyPropertyChangedImpl
    {
        ILogger _logger = Logging.Create<AnimationSplicerViewModel>();

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
        public ICommand LoadTestDataCommand { get; set; }
        public ICommand ClearBindingSelfCommand { get; set; }
        public ICommand ClearBindingSelfAndChildrenCommand { get; set; }
        public ICommand SaveAnimationCommand { get; set; }


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

            CreateCommands();

            TargetAnimation.SelectionChanged += x => BuildAnimation();
            ExternalSkeleton.SelectionChanged += SetExteralSkeleton;
            ExternalAnimation.SelectionChanged += SetExternalAnimation;

            // Initial init
            TargetAnimation.FindAllAnimations(_resourceLibary, _targetSkeletonNode.SkeletonFile.Header.SkeletonName);
            ExternalSkeleton.FindAllSkeletons(_resourceLibary);
            TargetSkeletonBones = MappableSkeletonBone.Create(_targetSkeletonNode);

            SelectedNode = TargetSkeletonBones.FirstOrDefault();
        }

        void CreateCommands()
        {
            ForceComputeCommand = new RelayCommand(() => BuildAnimation());
            LoadTestDataCommand = new RelayCommand(LoadTestData);
            SaveAnimationCommand = new RelayCommand(SaveAnimation);

            ClearBindingSelfCommand = new RelayCommand<MappableSkeletonBone>(ClearBindingSelf);
            ClearBindingSelfAndChildrenCommand = new RelayCommand<MappableSkeletonBone>(ClearBindingSelfAndChildren);
        }

        void LoadTestData()
        {
            // Temp - Populate with debug data. 
            //TargetAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
            //ExternalSkeleton.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid07.anim");
            //ExternalAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid07\club_and_blowpipe\missile_actions\hu7_clbp_aim_idle_01.anim");

            TargetAnimation.SelectedItem =  PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\staff_and_sword\combat_idles\hu1_sfsw_combat_idle_07.anim");
            ExternalSkeleton.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid01b.anim");
            ExternalAnimation.SelectedItem = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01b\subset\spellsinger\sword\stand\hu1b_elf_spellsinger_sw_stand_idle_01.anim");
        }


        void SaveAnimation()
        {
            try
            {
                var anim = BuildAnimation();
                if (anim != null)
                {
                    var fileFormat = anim.ConvertToFileFormat(_targetSkeletonNode.Skeleton);
                    AnimationFile.Write(fileFormat, @"C:\temp\Animation\floatyBoi.anim");
                }
            }
            catch (Exception e)
            { 
            
            }
            //
            //    var testAnim = AnimationFile.Create(new Filetypes.ByteParsing.ByteChunk(mainAnim.Data));
            //try
            //{
            //       var file = new Viewer.Animation.AnimationClip(testAnim);
            //       var fileToSave = file.ConvertToFileFormat(skeleton.Skeleton);
            //       AnimationFile.Write(testAnim);
            //}
            //catch (Exception e)
            //{ 
            //
            //}
            //
        }


        AnimationClip BuildAnimation()
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

                return animation;
            }
            catch (Exception e)
            {
                var error = $"Error creating new animation: {e.Message}";
                _logger.Error(error);
                return null;
            }
        }

        private void SetExternalAnimation(PackedFile externalAnimationClip)
        {
            ExternalSkeletonSettings.SetAnimation(externalAnimationClip);
            BuildAnimation();
        }

        private void SetExteralSkeleton(PackedFile newSelectedSkeleton)
        {
            ExternalAnimation.SelectedItem = null;
            if (ExternalSkeleton.SelectedItem != null)
            {
                ExternalAnimation.FindAllAnimations(_resourceLibary, ExternalSkeleton.SkeletonFile.Header.SkeletonName);
                PrefilBoneMappingBasedOnName(TargetSkeletonBones, ExternalSkeleton.SelectedSkeletonBonesFlattened);

                ExternalSkeletonSettings.Create(_resourceLibary, newSelectedSkeleton.Name);
            }
            BuildAnimation();
        }



        void ClearBindingSelf(MappableSkeletonBone node)
        {
            node.MappedBone = null;
        }

        void ClearBindingSelfAndChildren(MappableSkeletonBone node)
        {
            node.MappedBone = null;

            foreach (var child in node.Children)
                ClearBindingSelfAndChildren(child);
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



        void IsInFocus(bool isInFocus)
        {
        }

        #region Real time stuff, needed to update the external skeleton if it is visible
        public void UpdateNode(GameTime time)
        {
            ExternalSkeletonSettings.UpdateNode(time);
            ExternalSkeletonSettings.SetFrame(_animationPlayer.CurrentFrame);
            
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            ExternalSkeletonSettings?.DrawNode(device, parentTransform, commonShaderParameters);
        }
        #endregion
    }
}
