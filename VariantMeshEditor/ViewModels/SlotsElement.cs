using CommonDialogs;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
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
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slots;

        public ICommand AddSlotCommand { get; set; }

        public SlotsElement(FileSceneElement parent) : base(parent, "", "", "Slots")
        {
            AddSlotCommand = new RelayCommand(AddSlot);
        }

        void AddSlot()
        {
            Children.Add(new SlotElement(this, "new slot", ""));
        }
    }

    public class SlotElement : FileSceneElement
    {
        ResourceLibary _resourceLibary;
        Scene3d _virtualWorld;
        SkeletonElement _skeleton;

        public string _slotName;
        public string SlotName { get { return _slotName; } set { SetAndNotify(ref _slotName, value); SetDisplayName(AttachmentPoint); } }

        string _attachmentPoint;
        public string AttachmentPoint { get { return _attachmentPoint; }set { SetAndNotify(ref _attachmentPoint, value); SetDisplayName(value); } }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;

        public ICommand PopulateAttachmentPointList { get; set; }
        public ICommand DeleteSlot { get; set; }
        public ICommand AddNewMeshCommand { get; set; }

        public ObservableCollection<string> PossibleAttachmentPoints { get; set; } = new ObservableCollection<string>();

        public SlotElement(FileSceneElement parent, string slotName, string attachmentPoint) : base(parent, "", "", "")
        {
            SlotName = slotName;
            AttachmentPoint = attachmentPoint;
            CheckBoxGroupingName = "Slot" + Guid.NewGuid().ToString();
            SetDisplayName(AttachmentPoint);

            PopulateAttachmentPointList = new RelayCommand(OnPopulateAttachmentPointList);
            DeleteSlot = new RelayCommand<SlotElement>(OnDeleteSlot);
            AddNewMeshCommand = new RelayCommand(OnAddMesh);
            OnPopulateAttachmentPointList();
            
            if (DisplayName.ToLower().Contains("stump"))
            {
                IsChecked = false;
                IsExpanded = false;
            }
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

        void OnDeleteSlot(SlotElement slot)
        {
            Parent.Children.Remove(this);
        }

        void OnAddMesh()
        {
            using (LoadedPackFileBrowser loadedPackFileBrowser = new LoadedPackFileBrowser(_resourceLibary.PackfileContent.First()))
            {
                var res = loadedPackFileBrowser.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {

                    var selectedFile = loadedPackFileBrowser.GetSelecteFile();
                    SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
                    //var element = sceneLoader.Load("variantmeshes\\variantmeshdefinitions\\brt_royal_pegasus.variantmeshdefinition", new RootElement());
                    //element.CreateContent(_virtualWorld, _resourceLibary);

                    //var mesh = element.Children.First();
                    //mesh.Parent = null;
                    //_rootElement.AddChild(mesh);
                }
            }

            ////def_armoured_cold_one.variantmeshdefinition
            ////brt_pegasus.variantmeshdefinition
            //SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            //var element = sceneLoader.Load("variantmeshes\\variantmeshdefinitions\\brt_royal_pegasus.variantmeshdefinition", new RootElement());
            //element.CreateContent(_virtualWorld, _resourceLibary);
            //
            //var mesh = element.Children.First();
            //mesh.Parent = null;
            //_rootElement.AddChild(mesh);
            //
            //CreateMeshList();

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
