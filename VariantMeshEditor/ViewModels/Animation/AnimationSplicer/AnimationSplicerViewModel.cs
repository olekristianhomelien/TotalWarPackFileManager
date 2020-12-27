using Common;
using CommonDialogs.Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using VariantMeshEditor.Services;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.Settings;
using VariantMeshEditor.ViewModels.Skeleton;
using Viewer.Animation;
using Viewer.Gizmo;
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
        AnimationClip _lastComputedAnimation = null;
        GizmoEditor _selectionGizmo;
        SkeletonAnimationLookUpHelper _animationToSkeletonTypeHelper = new SkeletonAnimationLookUpHelper();

        
        bool _isSelected;
        public bool IsSelected { get { return _isSelected; } set { SetAndNotify(ref _isSelected, value); IsInFocus(IsSelected); } }

        public FilterableAnimationsViewModel TargetAnimation { get; set; }
        public FilterableAnimationsViewModel ExternalAnimation { get; set; }

        public ExternalSkeletonVisualizationHelper ExternalSkeletonVisualizationHelper { get; set; } = new ExternalSkeletonVisualizationHelper();

        ObservableCollection<MappedSkeletonBoneConfig> _boneMapping;
        public ObservableCollection<MappedSkeletonBoneConfig> BoneMapping { get { return _boneMapping; } set { SetAndNotify(ref _boneMapping, value); } }

        MainAnimation _selectedMainAnimation = MainAnimation.Other;
        public MainAnimation SelectedMainAnimation { get { return _selectedMainAnimation; } set { SetAndNotify(ref _selectedMainAnimation, value); } }

        public ICommand ForceComputeCommand { get; set; }
        public ICommand ClearBindingSelfCommand { get; set; }
        public ICommand ClearBindingSelfAndChildrenCommand { get; set; }
        public ICommand SaveAnimationCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand ResetMappingBackToDefaultCommand { get; set; }
        public ICommand ClearAllMappingsCommand { get; set; }

        public ICommand LoadTestData_FloatingHumanCommand { get; set; }
        public ICommand LoadTestData_BlowPipeGoblinCommand { get; set; }
        public ICommand LoadTestData_DancingDwarfCommand { get; set; }


        public MappedSkeletonBoneConfig _selectedNode;
        public MappedSkeletonBoneConfig SelectedNode { get { return _selectedNode; } set { SetAndNotify(ref _selectedNode, value); OnItemSelected(_selectedNode); } }


        public BoneCopyMethod _boneCopyMethod = BoneCopyMethod.Ratio;
        public BoneCopyMethod DefaultBoneCopyMethod{get { return _boneCopyMethod; }set { SetAndNotify(ref _boneCopyMethod, value); UpdateBoneCopyMethod(value); }}

        public bool _useAttachmentPointFix = false;
        public bool UseAttachmentPointFix { get { return _useAttachmentPointFix; } set { SetAndNotify(ref _useAttachmentPointFix, value); } }

        void OnItemSelected(MappedSkeletonBoneConfig bone)
        {
            if (bone != null && bone.ExternalBone != null)
            {
                _targetSkeletonNode.ViewModel.SelectedBone = bone.OriginalBone;
                ExternalSkeletonVisualizationHelper.SetSelectedBone(bone.ExternalBone.BoneIndex);

                var currentGizmoItem = new SkeletonBoneGizmoItemWrapper(_targetSkeletonNode.GameSkeleton, bone.OriginalBone.BoneIndex, bone, _selectionGizmo);
                _selectionGizmo.SelectItem(currentGizmoItem);
            }
            else
            {
                ExternalSkeletonVisualizationHelper.SetSelectedBone(-1);
                _selectionGizmo.SelectItem(null);
    
            }
        }


        public AnimationSplicerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer, GizmoEditor selectionGizmo)
        {
            _selectionGizmo = selectionGizmo;
            _selectionGizmo.GizmoUpdatedEvent += () => BuildAnimation();
            _resourceLibary = resourceLibary;
            _targetSkeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;

            _animationToSkeletonTypeHelper.FindAllAnimations(_resourceLibary);

            TargetAnimation = new FilterableAnimationsViewModel("Source Animation: ", _resourceLibary, _animationToSkeletonTypeHelper, false);
            TargetAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, _targetSkeletonNode.FullPath);

            ExternalAnimation = new FilterableAnimationsViewModel("Target Animation: ", _resourceLibary, _animationToSkeletonTypeHelper, true);
            ExternalAnimation.SelectedSkeletonChanged += SetExteralSkeleton;
            ExternalAnimation.SelectedAnimationChanged += SetExternalAnimation;
            CreateCommands();

            ResetAnimationMapping();

            SelectedNode = BoneMapping.FirstOrDefault();
        }

        void CreateCommands()
        {
            ForceComputeCommand = new RelayCommand(() => BuildAnimation());

            LoadTestData_FloatingHumanCommand = new RelayCommand(LoadTestData_FloatingHuman);
            LoadTestData_BlowPipeGoblinCommand = new RelayCommand(LoadTestData_BlowPipeGoblin);
            LoadTestData_DancingDwarfCommand = new RelayCommand(LoadTestData_DancingDwarf);

            SaveAnimationCommand = new RelayCommand(SaveAnimationToFile);
            ExportCommand = new RelayCommand(ExportCurrentConfiguration);
            ImportCommand = new RelayCommand(LoadConfiguration);
            ResetMappingBackToDefaultCommand = new RelayCommand(ResetAnimationMappingBackToDefault);
            ClearAllMappingsCommand = new RelayCommand(ResetAnimationMapping);

            ClearBindingSelfCommand = new RelayCommand<MappedSkeletonBoneConfig>(ClearBindingSelf);
            ClearBindingSelfAndChildrenCommand = new RelayCommand<MappedSkeletonBoneConfig>(ClearBindingSelfAndChildren);
        }

        void UpdateBoneCopyMethod(BoneCopyMethod value)
        {
            if (BoneMapping != null)
                MappableSkeletonBoneHelper.SetDefaultBoneCopyMethod(BoneMapping.FirstOrDefault(), value);
        }

        void ResetAnimationMapping()
        {
            BoneMapping = MappableSkeletonBoneHelper.Create(_targetSkeletonNode);
            UpdateBoneCopyMethod(DefaultBoneCopyMethod);
        }

        void ResetAnimationMappingBackToDefault()
        {
            BoneMapping = MappableSkeletonBoneHelper.Create(_targetSkeletonNode);
            UpdateBoneCopyMethod(DefaultBoneCopyMethod);
            if (ExternalAnimation != null)
                PrefilBoneMappingBasedOnName(BoneMapping, ExternalAnimation.SelectedSkeletonBonesFlattened);
        }

        void LoadTestData_FloatingHuman()
        {
            TargetAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\staff_and_sword\combat_idles\hu1_sfsw_combat_idle_07.anim");
            ExternalAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid01b.anim");
            ExternalAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01b\subset\spellsinger\sword\stand\hu1b_elf_spellsinger_sw_stand_idle_01.anim");
        }

        void LoadTestData_BlowPipeGoblin()
        {
            TargetAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
            ExternalAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid07.anim");
            ExternalAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid07\club_and_blowpipe\missile_actions\hu7_clbp_aim_idle_01.anim");
        }

        void LoadTestData_DancingDwarf()
        {
            TargetAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\staff_and_sword\combat_idles\hu1_sfsw_combat_idle_07.anim");
            ExternalAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid03.anim");
            ExternalAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid03\unarmed\cannon_crew\hu3_clap_and_order_2.anim");
        }

        void SaveAnimationToFile()
        {
            _logger.Here().Information("Starting to save animation");
            SaveFileDialog dialog = new SaveFileDialog
            {
                DefaultExt = ".anim"
            };
            if (dialog.ShowDialog() == true)
            {
                var anim = BuildAnimation();
                if (anim == null)
                {
                    _logger.Here().Error("Animation failed to build");
                    System.Windows.MessageBox.Show("Error - Animation failed to build");
                }
                else
                {
                    try
                    {
                        var fileFormat = anim.ConvertToFileFormat(_targetSkeletonNode.GameSkeleton);
                        AnimationFile.Write(fileFormat, dialog.FileName);
                    }
                    catch (Exception e)
                    {
                        _logger.Here().Error("Animation saving failed: " + e.ToString());
                        System.Windows.MessageBox.Show("Error - Unable to save animation");
                    }
                }
            }
            _logger.Here().Information("Saving animation completed");
        }

        void ExportCurrentConfiguration()
        {
            _logger.Here().Information("Starting to save config");
            SaveFileDialog dialog = new SaveFileDialog
            {
                DefaultExt = ".splicCfg"
            };

            if (dialog.ShowDialog() == true)
            {
                AnimationSplicerSettings settings = new AnimationSplicerSettings
                {
                    ExternalAnimation = ExternalAnimation.Data,
                    TargetAnimation = TargetAnimation.Data,
                    DefaultBoneCopyMethod = DefaultBoneCopyMethod,
                    SelectedMainAnimation = SelectedMainAnimation,
                    MappableBoneSettings = BoneMapping
                };
                //settings.PreperForSave(SceneElementHelper.GetRoot(_targetSkeletonNode);

                var str = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(dialog.FileName, str);
            }

            _logger.Here().Information("Saving config completed");
        }

        void LoadConfiguration()
        { 
        
        }


        AnimationClip BuildAnimation()
        {
            try
            {
                AnimationBuilderService.AnimationBuilderSettings settings = new AnimationBuilderService.AnimationBuilderSettings()
                {
                    OtherAnimationClip = ExternalAnimation.SelectedAnimationClip,
                    OtherSkeletonFile = ExternalAnimation.SelectedGameSkeleton,

                    BoneSettings = BoneMapping,
                    SourceSkeleton = _targetSkeletonNode.GameSkeleton,
                    SourceAnimationClip = TargetAnimation.SelectedAnimationClip,

                    SelectedMainAnimation = SelectedMainAnimation
                };

                AnimationBuilderService builder = new AnimationBuilderService();
                var animation = builder.CreateMergedAnimation(settings);
                _lastComputedAnimation = animation;

                var currentFrame = _animationPlayer._animationNode.AnimationPlayer.CurrentFrame;

                if (animation != null)
                {
                    _animationPlayer.SetAnimationClip(new List<AnimationClip>() { animation }, _targetSkeletonNode.GameSkeleton);
                    _animationPlayer._animationNode.AnimationPlayer.CurrentFrame = currentFrame;
                    _selectionGizmo.UpdatePositionOfItems(true);
                }
                else
                    _animationPlayer.SetAnimationClip(null, _targetSkeletonNode.GameSkeleton);

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
           ExternalSkeletonVisualizationHelper.SetAnimation(ExternalAnimation.SelectedAnimationClip);
       }

        private void SetExteralSkeleton(FilterableAnimationsViewModel newSelectedSkeleton)
        {
            if (newSelectedSkeleton.SelectedSkeleton != null)
            {
                PrefilBoneMappingBasedOnName(BoneMapping, newSelectedSkeleton.SelectedSkeletonBonesFlattened);
                ExternalSkeletonVisualizationHelper.Create(_resourceLibary, newSelectedSkeleton.CurrentSkeletonName + ".anim");
            }
        }

        void ClearBindingSelf(MappedSkeletonBoneConfig node)
        {
            node.ExternalBone = null;
        }

        void ClearBindingSelfAndChildren(MappedSkeletonBoneConfig node)
        {
            node.ExternalBone = null;

            foreach (var child in node.Children)
                ClearBindingSelfAndChildren(child);
        }

        void PrefilBoneMappingBasedOnName(IEnumerable<MappedSkeletonBoneConfig> nodes, ObservableCollection<SkeletonBoneNode> externalBonesList)
        {
            foreach(var node in nodes)
            {
                if (node != null)
                {
                    node.ExternalBone = externalBonesList.FirstOrDefault(x => x?.BoneName == node.OriginalBone.BoneName);
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
            ExternalSkeletonVisualizationHelper.UpdateNode(time);
            ExternalSkeletonVisualizationHelper.SetFrame(_animationPlayer.CurrentFrame);

            if(_animationPlayer.IsPlaying() == false)
                _selectionGizmo.UpdatePositionOfItems(false);
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            ExternalSkeletonVisualizationHelper?.DrawNode(device, parentTransform, commonShaderParameters);
        }
        #endregion
    }
}
