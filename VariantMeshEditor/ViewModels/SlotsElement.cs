using Common;
using CommonDialogs;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using Serilog;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.RigidModel;
using VariantMeshEditor.ViewModels.Skeleton;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class SlotsElement : FileSceneElement
    {
        ILogger _logger = Logging.Create<SlotsElement>();

        ResourceLibary _resourceLibary;
        Scene3d _virtualWorld;

        public override FileSceneElementEnum Type => FileSceneElementEnum.Slots;

        public ICommand AddSlotCommand { get; set; }

        public SlotsElement(FileSceneElement parent) : base(parent, "", "", "Slots")
        {
            AddSlotCommand = new RelayCommand(AddSlot);
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
            _virtualWorld = virtualWorld;
        }

        void AddSlot()
        {
            _logger.Here().Information("Creating new slot");
            var newSlot = new SlotElement(this, "new slot", "");
            newSlot.CreateContent(_virtualWorld, _resourceLibary);
            Children.Add(newSlot);
        }
    }

    public class SlotElement : FileSceneElement
    {
        ILogger _logger = Logging.Create<SlotElement>();

        ResourceLibary _resourceLibary;
        Scene3d _virtualWorld;
        SkeletonElement _skeleton;

        public string _slotName;
        public string SlotName { get { return _slotName; } set { SetAndNotify(ref _slotName, value); SetDisplayName(AttachmentPoint); } }

        string _attachmentPoint;
        public string AttachmentPoint { get { return _attachmentPoint; }set { SetAndNotify(ref _attachmentPoint, value); SetDisplayName(value); } }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;

        public ICommand PopulateAttachmentPointList { get; set; }
        public ICommand DeleteSlotCommand { get; set; }
        public ICommand GotoSlotCommand { get; set; }
        public ICommand AddNewMeshCommand { get; set; }

        public ObservableCollection<string> PossibleAttachmentPoints { get; set; } = new ObservableCollection<string>();

        public SlotElement(FileSceneElement parent, string slotName, string attachmentPoint) : base(parent, "", "", "")
        {
            SlotName = slotName;
            AttachmentPoint = attachmentPoint;
            CheckBoxGroupingName = "Slot" + Guid.NewGuid().ToString();
            SetDisplayName(AttachmentPoint);

            PopulateAttachmentPointList = new RelayCommand(OnPopulateAttachmentPointList);
            DeleteSlotCommand = new RelayCommand(OnDeleteSlot);
            GotoSlotCommand = new RelayCommand(OnGotoSlot);
            AddNewMeshCommand = new RelayCommand(OnAddMesh);
            OnPopulateAttachmentPointList();

            var enableNodeAsDefault = !DisplayName.ToLower().Contains("stump");
            IsChecked = enableNodeAsDefault;
            IsExpanded = enableNodeAsDefault;
        }

        void OnPopulateAttachmentPointList()
        {
            PossibleAttachmentPoints.Clear();
            PossibleAttachmentPoints.Add("   ");

            var attachmentPoints = new List<string>();
            var allRigidModelElements = SceneElementHelper.GetAllOfTypeInSameVariantMesh<RigidModelElement>(this);
            
            foreach (var rigidModelElement in allRigidModelElements)
            {
                foreach (var header in rigidModelElement.Model.LodHeaders)
                {
                    foreach (var model in header.LodModels)
                    {
                        foreach (var attacmentPoint in model.AttachmentPoint)
                            attachmentPoints.Add(attacmentPoint.Name);
                    }
                }
            }

            var possibleAttackmentPoints = attachmentPoints.Distinct().ToList();
            foreach (var item in possibleAttackmentPoints)
                PossibleAttachmentPoints.Add(item);
        }

        void OnDeleteSlot()
        {
            Parent.Children.Remove(this);
        }

        void OnGotoSlot()
        {
            var root = SceneElementHelper.GetRoot(this) as RootElement;
            if (root != null)
                root.SelectNode(this);
        }

        void OnAddMesh()
        {
            try
            {
                using (LoadedPackFileBrowser loadedPackFileBrowser = new LoadedPackFileBrowser(_resourceLibary.PackfileContent.First()))
                {
                    loadedPackFileBrowser.OnlyShowModelExtentions();
                    var res = loadedPackFileBrowser.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        var selectedFile = loadedPackFileBrowser.GetSelecteFile();
                        SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
                        var element = sceneLoader.Load(selectedFile, new RootElement(null));
                        element.CreateContent(_virtualWorld, _resourceLibary);

                        var mesh = element.Children.First();
                        AddChild(mesh);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Here().Error($"Error loading new file - {e}");
            }
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
            _virtualWorld = virtualWorld;
            _skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this).FirstOrDefault();
        }

        protected override void UpdateNode(GameTime time)
        {
            var boneIndex = -1;
            if( _skeleton?.GameSkeleton != null)
                boneIndex  = _skeleton.GameSkeleton.GetBoneIndex(AttachmentPoint);

            WorldTransform = Matrix.Identity;
            if (boneIndex != -1)
                WorldTransform = _skeleton.GameSkeleton.GetAnimatedWorldTranform(boneIndex);    
        }

        void SetDisplayName(string attachmentPointName)
        {
            var name = $"[Slot] - {SlotName}";
            if (!string.IsNullOrWhiteSpace(attachmentPointName))
                name += $" - {attachmentPointName}";
            DisplayName = name;
        }
    }
}
