﻿using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels.Skeleton
{
    public class SkeletonElement : FileSceneElement
    {

        public AnimationFile SkeletonFile { get; set; }
        SkeletonModel SkeletonModel { get; set; }
        public Viewer.Animation.Skeleton Skeleton { get; set; }

        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;

        public SkeletonViewModel ViewModel { get; set; }

        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, "", fullPath, "Skeleton")
        {

        }

        public void Create(AnimationPlayer animationPlayer, ResourceLibary resourceLibary, string skeletonName)
        {
            string animationFolder = "animations\\skeletons\\";
            var skeletonFilePath = animationFolder + skeletonName;
            var file = PackFileLoadHelper.FindFile(resourceLibary.PackfileContent, skeletonFilePath);
            if (file != null)
            {
                SkeletonFile = AnimationFile.Create(new ByteChunk(file.Data));
                FullPath = skeletonFilePath;
                FileName = Path.GetFileNameWithoutExtension(skeletonFilePath);
                Skeleton = new Viewer.Animation.Skeleton(SkeletonFile);
            }

            SkeletonModel = new SkeletonModel(resourceLibary.GetEffect(ShaderTypes.Line));
            SkeletonModel.Create(animationPlayer, Skeleton);

            ViewModel = new SkeletonViewModel(this);
        }


        protected override void UpdateNode(GameTime time)
        {
            SkeletonModel.Update(time);
        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            SkeletonModel.SelectedBoneIndex = ViewModel.SelectedBone?.BoneIndex;
            SkeletonModel.Draw(device, parentTransform, commonShaderParameters);
        }
    }
}