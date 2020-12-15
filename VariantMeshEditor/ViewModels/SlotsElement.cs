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
        AnimationElement _animation;
        SkeletonElement _skeleton;

        public string _slotName;
        public string SlotName { get { return _slotName; } set { SetAndNotify(ref _slotName, value); SetDisplayName(AttachmentPoint); } }

        string _attachmentPoint;
        public string AttachmentPoint { get { return _attachmentPoint; }set { SetAndNotify(ref _attachmentPoint, value); SetDisplayName(value); } }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;

        public ICommand PopulateAttachmentPointList { get; set; }
        public ICommand DeleteSlot { get; set; }

        public ObservableCollection<string> PossibleAttachmentPoints { get; set; } = new ObservableCollection<string>();

        public SlotElement(FileSceneElement parent, string slotName, string attachmentPoint) : base(parent, "", "", "")
        {
            SlotName = slotName;
            AttachmentPoint = attachmentPoint;
            CheckBoxGroupingName = "Slot" + Guid.NewGuid().ToString();
            SetDisplayName(AttachmentPoint);

            PopulateAttachmentPointList = new RelayCommand(OnPopulateAttachmentPointList);
            DeleteSlot = new RelayCommand<SlotElement>(OnDeleteSlot);
            OnPopulateAttachmentPointList();
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

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _animation = SceneElementHelper.GetAllOfTypeInSameVariantMesh<AnimationElement>(this).FirstOrDefault();
            _skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this).FirstOrDefault();




         


        }

        protected override void UpdateNode(GameTime time)
        {
            var boneIndex = -1;

            for (int i = 0; i < _skeleton?.Skeleton?.BoneNames.Length; i++)
            {
                if (_skeleton.Skeleton.BoneNames[i] == AttachmentPoint)
                {
                    boneIndex = i;
                    break;
                }
            }

            if (boneIndex != -1)
            {
                var bonePos = _skeleton.Skeleton.WorldTransform[boneIndex];
                WorldTransform = Matrix.Multiply(bonePos, GetAnimatedBone(boneIndex));
            }
            else
            {
                WorldTransform = Matrix.Identity;
            }
        }

        public Matrix GetAnimatedBone(int index)
        {
            if (index == -1)
                return Matrix.Identity;
            var currentFrame = _animation.AnimationPlayer.GetCurrentFrame();
            if (currentFrame == null)
                return Matrix.Identity;

            return currentFrame.BoneTransforms[index].Transform;
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
