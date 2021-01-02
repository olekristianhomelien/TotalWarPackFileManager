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
using System.Windows.Forms.Integration;
using System.Windows.Input;
using VariantMeshEditor.Services;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.Settings;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.ViewModels.VariantMesh;
using VariantMeshEditor.Views.EditorViews.Animation.AnimationSplicerViews.BoneMappingViews;
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
        GizmoEditor _selectionGizmo;
        Scene3d _virtualWorld;
        SkeletonAnimationLookUpHelper _animationToSkeletonTypeHelper = new SkeletonAnimationLookUpHelper();
        AdvBoneMappingWindow _advanceBoneMappingWindow;

        public VariantMeshElement VariantMeshParent { get { return _targetSkeletonNode.Parent as VariantMeshElement; } }

        public FilterableAnimationsViewModel TargetAnimation { get; set; }
        public FilterableAnimationsViewModel ExternalAnimation { get; set; }

        public ExternalSkeletonVisualizationHelper ExternalSkeletonVisualizationHelper { get; set; } = new ExternalSkeletonVisualizationHelper();



        MainAnimation _selectedMainAnimation = MainAnimation.Other;
        public MainAnimation SelectedMainAnimation { get { return _selectedMainAnimation; } set { SetAndNotify(ref _selectedMainAnimation, value); } }

        public ICommand ForceComputeCommand { get; set; }
        public ICommand SaveAnimationCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand OpenAdvanceMappingWindow { get; set; }

        public ICommand LoadTestData_FloatingHumanCommand { get; set; }
        public ICommand LoadTestData_BlowPipeGoblinCommand { get; set; }
        public ICommand LoadTestData_DancingDwarfCommand { get; set; }


        ObservableCollection<AdvBoneMappingBone> _boneMapping = new ObservableCollection<AdvBoneMappingBone>();
        public ObservableCollection<AdvBoneMappingBone> BoneMapping{ get { return _boneMapping; } set { SetAndNotify(ref _boneMapping, value); } }

        public AdvBoneMappingBone _selectedNode;
        public AdvBoneMappingBone SelectedNode { get { return _selectedNode; } set { SetAndNotify(ref _selectedNode, value); OnItemSelected(_selectedNode); } }

        void OnItemSelected(AdvBoneMappingBone bone)
        {
            // If mapping select the other node.
            if (bone?.Settings.HasMapping == true)
            {
                _targetSkeletonNode.ViewModel.SetSelectedBoneByIndex(bone.BoneIndex);
                ExternalSkeletonVisualizationHelper.GetSkeletonElement().ViewModel.SetSelectedBoneByIndex(bone.Settings.MappingBoneId);
            }
            else
            {
                _targetSkeletonNode.ViewModel.SetSelectedBoneByIndex(-1);
                ExternalSkeletonVisualizationHelper.GetSkeletonElement()?.ViewModel?.SetSelectedBoneByIndex(-1);
            }

            // If bone, update gizmo
            if (bone != null)
            {
                var currentGizmoItem = new SkeletonBoneGizmoItemWrapper(_targetSkeletonNode.GameSkeleton, bone.BoneIndex, bone, _selectionGizmo);
                _selectionGizmo.SelectItem(currentGizmoItem);
            }
            else
            {
                _selectionGizmo.SelectItem(null);
            }
        }

        public AnimationSplicerViewModel(ResourceLibary resourceLibary, SkeletonElement skeletonNode, AnimationPlayerViewModel animationPlayer, Scene3d virtualWorld)
        {
            _virtualWorld = virtualWorld;
            _selectionGizmo = _virtualWorld.Gizmo;
            _selectionGizmo.GizmoUpdatedEvent += () => BuildAnimation();
            _resourceLibary = resourceLibary;
            _targetSkeletonNode = skeletonNode;
            _animationPlayer = animationPlayer;

            _animationToSkeletonTypeHelper.FindAllAnimations(_resourceLibary);

            TargetAnimation = new FilterableAnimationsViewModel("Source Animation: ", _resourceLibary, _animationToSkeletonTypeHelper, false);
            TargetAnimation.SelectedSkeletonChanged += (arg) => OnTargetSkeletonChanged();
            TargetAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, _targetSkeletonNode.FullPath);

            ExternalAnimation = new FilterableAnimationsViewModel("Target Animation: ", _resourceLibary, _animationToSkeletonTypeHelper, true);
            ExternalAnimation.SelectedSkeletonChanged += OnOtherSkeletonChanged;
            ExternalAnimation.SelectedAnimationChanged += OnOtherAnimationChanaged;

            // Create commands
            ForceComputeCommand = new RelayCommand(() => BuildAnimation());

            LoadTestData_FloatingHumanCommand = new RelayCommand(() => LoadTestData(true, false, false));
            LoadTestData_BlowPipeGoblinCommand = new RelayCommand(() => LoadTestData(false, true, false));
            LoadTestData_DancingDwarfCommand = new RelayCommand(() => LoadTestData(false, false, true));

            SaveAnimationCommand = new RelayCommand(SaveAnimationToFile);
            ExportCommand = new RelayCommand(() => { AnimationSplicerSettings.ExportCurrentConfiguration(this); });
            ImportCommand = new RelayCommand(() => { AnimationSplicerSettings.ImportConfiguration(this); });

            OpenAdvanceMappingWindow = new RelayCommand(OnOpenAdvanceMappingWindow);
        }

        void OnOpenAdvanceMappingWindow()
        {
            try
            {
                if (_advanceBoneMappingWindow == null)
                {
                    if (ExternalAnimation.SelectedGameSkeleton == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Error - No Target skeleton selected");
                        return;
                    }
                    _advanceBoneMappingWindow = new AdvBoneMappingWindow();
                    _advanceBoneMappingWindow.Deactivated += (sender, e) => { _advanceBoneMappingWindow.Topmost = true; _advanceBoneMappingWindow.Activate(); };
                    _advanceBoneMappingWindow.Closed += (sender, e) =>
                    {
                        var context = _advanceBoneMappingWindow.DataContext as AdvBoneMappingViewModel;
                        BoneMapping = context.GetBoneMapping();
                        _advanceBoneMappingWindow = null;
                    };

                    _advanceBoneMappingWindow.DataContext = new AdvBoneMappingViewModel(BoneMapping,
                        ExternalAnimation.SelectedGameSkeleton,
                        _targetSkeletonNode.ViewModel,
                        ExternalSkeletonVisualizationHelper.GetSkeletonElement().ViewModel);

                    ElementHost.EnableModelessKeyboardInterop(_advanceBoneMappingWindow);
                    _advanceBoneMappingWindow.Show();
                }
                else
                {
                    _advanceBoneMappingWindow.BringIntoView();
                }
            }
            catch (Exception e)
            {
                _advanceBoneMappingWindow = null;
                _logger.Here().Error("BoneMappingWindow failed: " + e.ToString());
            }
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

                var animation = AnimationBuilderService.CreateMergedAnimation(settings);
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

        void OnTargetSkeletonChanged()
        {
            BoneMapping = BoneMappingHelper.CreateSkeletonBoneList(TargetAnimation.SelectedGameSkeleton);
            BoneMappingHelper.ComputeBoneCount(BoneMapping);
            SelectedNode = BoneMapping.FirstOrDefault();
        }

        private void OnOtherAnimationChanaged(PackedFile externalAnimationClip)
        {
            ExternalSkeletonVisualizationHelper.SetAnimation(ExternalAnimation.SelectedAnimationClip);
        }

        private void OnOtherSkeletonChanged(FilterableAnimationsViewModel newSelectedSkeleton)
        {
            if (newSelectedSkeleton.SelectedSkeleton != null)
            {
                var otherBones = BoneMappingHelper.CreateSkeletonBoneList(newSelectedSkeleton.SelectedGameSkeleton);
                BoneMapping.FirstOrDefault().ClearMapping(true);
                BoneMappingHelper.AutomapDirectBoneLinksBasedOnNames(BoneMapping.FirstOrDefault(), otherBones);
                ExternalSkeletonVisualizationHelper.Create(_resourceLibary, newSelectedSkeleton.SelectedSkeleton);
            }
        }

        void LoadTestData(bool human, bool goblin, bool dancingDwarf)
        {
            if (human)
            {
                TargetAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\staff_and_sword\combat_idles\hu1_sfsw_combat_idle_07.anim");
                ExternalAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid01b.anim");
                ExternalAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01b\subset\spellsinger\sword\stand\hu1b_elf_spellsinger_sw_stand_idle_01.anim");
            }
            else if (goblin)
            {
                TargetAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid05\dual_sword\stand\hu5_ds_stand_idle_01.anim");
                ExternalAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid07.anim");
                ExternalAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid07\club_and_blowpipe\missile_actions\hu7_clbp_aim_idle_01.anim");
            }
            else if (dancingDwarf)
            {
                TargetAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid01\staff_and_sword\combat_idles\hu1_sfsw_combat_idle_07.anim");
                ExternalAnimation.SelectedSkeleton = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\skeletons\humanoid03.anim");
                ExternalAnimation.SelectedAnimation = PackFileLoadHelper.FindFile(_resourceLibary.PackfileContent, @"animations\battle\humanoid03\unarmed\cannon_crew\hu3_clap_and_order_2.anim");
            }
        }

        #region Real time stuff, needed to update the external skeleton if it is visible
        public void UpdateNode(GameTime time)
        {
            ExternalSkeletonVisualizationHelper.UpdateNode(time);
            ExternalSkeletonVisualizationHelper.SetFrame(_animationPlayer.CurrentFrame);

            _selectionGizmo.UpdateGizmo = !_animationPlayer.IsPlaying();
            if (_animationPlayer.IsPlaying() == false)
                _selectionGizmo.UpdatePositionOfItems(false);
        }

        public void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            ExternalSkeletonVisualizationHelper?.DrawNode(device, parentTransform, commonShaderParameters);
        }
        #endregion
    }
}
