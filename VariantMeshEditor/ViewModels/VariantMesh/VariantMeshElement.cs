using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation;
using VariantMeshEditor.ViewModels.Skeleton;

namespace VariantMeshEditor.ViewModels.VariantMesh
{
    public class VariantMeshElement : FileSceneElement
    {
        public VariantMeshViewModel ViewModel { get; set; }

        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
            ViewModel = new VariantMeshViewModel(this);
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;




        protected override void UpdateNode(GameTime time)
        {
            if (ViewModel.SelectedMesh != null && ViewModel.SelectedBone != null)
            {
                var skeleton = SceneElementHelper.GetFirstChild<SkeletonElement>(ViewModel.SelectedMesh);
                var animation = SceneElementHelper.GetFirstChild<AnimationElement>(ViewModel.SelectedMesh);
                if (skeleton != null && animation != null)
                {
                    var bonePos = skeleton.Skeleton.WorldTransform[ViewModel.SelectedBone.Id];
                    WorldTransform = Matrix.Identity;
                    
                     WorldTransform= Matrix.CreateTranslation(Matrix.Multiply(bonePos, GetAnimatedBone(ViewModel.SelectedBone.Id, animation)).Translation + new Vector3(0.0f,0.8f, -0.15f)) ;
                    //WorldTransform = bonePos;
                    return;
                }
            }

            WorldTransform = Matrix.Identity;
        }

        public Matrix GetAnimatedBone(int index, AnimationElement animationElement)
        {
            if (index == -1)
                return Matrix.Identity;
            var currentFrame = animationElement.AnimationPlayer.GetCurrentFrame();
            if (currentFrame == null)
                return Matrix.Identity;

            return currentFrame.BoneTransforms[index].Transform;
        }
    }
}
