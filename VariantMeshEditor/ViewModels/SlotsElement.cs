using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Xna.Framework;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class SlotsElement : FileSceneElement
    {
        public SlotsElement(FileSceneElement parent) : base(parent, "", "", "Slots") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slots;
    }

    public class SlotElement : FileSceneElement
    {
        SlotController _controller;
        AnimationElement _animation;
        SkeletonElement _skeleton;

        public string SlotName { get; set; }
        public string AttachmentPoint { get; set; }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;

        public SlotElement(FileSceneElement parent, string slotName, string attachmentPoint) : base(parent, "", "", "")
        {
            SlotName = slotName;
            AttachmentPoint = attachmentPoint;
            CheckBoxGroupingName = "Slot" + Guid.NewGuid().ToString();
            SetDisplayName(AttachmentPoint);
        }



        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _animation = SceneElementHelper.GetAllOfTypeInSameVariantMesh<AnimationElement>(this).FirstOrDefault();
            _skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this).FirstOrDefault();
            SlotEditorView view = new SlotEditorView();
            _controller = new SlotController(view, this, _skeleton);
        }

        protected override void UpdateNode(GameTime time)
        {
            int boneIndex = _controller.AttachmentBoneIndex;
            if (boneIndex != -1)
            {
                var bonePos = _skeleton.Skeleton.WorldTransform[boneIndex];
                WorldTransform = Matrix.Multiply(bonePos, GetAnimatedBone(boneIndex));
                SetDisplayName(_skeleton.Skeleton.BoneNames[boneIndex]);
            }
            else
            {
                WorldTransform = Matrix.Identity;
                SetDisplayName("");
            }
        }

        public Matrix GetAnimatedBone(int index)
        {
            if (index == -1)
                return Matrix.Identity;
            var currentFrame = _animation.AnimationPlayer.GetCurrentFrame();
            if (currentFrame == null)
                return Matrix.Identity;

            return _animation.AnimationPlayer.GetCurrentFrame().BoneTransforms[index].Transform;
        }

        void SetDisplayName(string attachmentPointName)
        {
            DisplayName = $"Slot -{SlotName} - {attachmentPointName}";
        }
    }
}
