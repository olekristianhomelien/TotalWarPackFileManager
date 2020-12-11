﻿using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Filetypes.RigidModel.AnimationFile;

namespace Viewer.Animation
{
    public class AnimationClip
    {
        public class KeyFrame
        {
            public List<Vector3> Translation { get; set; } = new List<Vector3>();
            public List<Quaternion> Rotation { get; set; } = new List<Quaternion>();
        }


        public KeyFrame StaticFrame { get; set; } = null;
        public List<KeyFrame> DynamicFrames = new List<KeyFrame>();

        public List<AnimationBoneMapping> RotationMappings { get; set; } = new List<AnimationBoneMapping>();
        public List<AnimationBoneMapping> TranslationMappings { get; set; } = new List<AnimationBoneMapping>();

        public bool UseStaticFrame { get; set; } = true;
        public bool UseDynamicFames { get; set; } = true;

        public AnimationClip() { }

        public AnimationClip(AnimationFile file)
        {
            RotationMappings = file.RotationMappings.ToList();
            TranslationMappings = file.TranslationMappings.ToList();

            if (file.StaticFrame != null)
                StaticFrame = CreateKeyFrame(file.StaticFrame);

            foreach(var frame in file.DynamicFrames)
                DynamicFrames.Add(CreateKeyFrame(frame));
        }

        KeyFrame CreateKeyFrame(AnimationFile.Frame frame)
        {
            var output = new KeyFrame();
            foreach (var translation in frame.Transforms)
                output.Translation.Add(new Vector3(translation.X, translation.Y, translation.Z));

            foreach (var rotation in frame.Quaternion)
                output.Rotation.Add(new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));
            return output;
        }


        public AnimationFile ConvertToFileFormat(Skeleton skeleton)
        {
            AnimationFile output = new AnimationFile();
            output.Header.AnimationType = 7;
            output.Header.AnimationTotalPlayTimeInSec = DynamicFrames.Count() / output.Header.FrameRate;
            output.Header.SkeletonName = skeleton.SkeletonName;

            output.Bones = new BoneInfo[skeleton.BoneCount];
            for (int i = 0; i < skeleton.BoneCount; i++)
            {
                output.Bones[i] = new BoneInfo()
                {
                    Id = i,
                    Name = skeleton.BoneNames[i],
                    ParentId = skeleton.ParentBoneId[i]
                };
            }

            // Mappings
            output.RotationMappings = RotationMappings.ToList();
            output.TranslationMappings = TranslationMappings.ToList();

            // Static
            if (StaticFrame != null)
                output.StaticFrame = CreateFrameFromKeyFrame(StaticFrame);

            // Dynamic
            foreach (var frame in DynamicFrames)
                output.DynamicFrames.Add(CreateFrameFromKeyFrame(frame));

            return output;
        }

        Frame CreateFrameFromKeyFrame(KeyFrame keyFrame)
        {
            var frame = new Frame();
            foreach (var trans in keyFrame.Translation)
                frame.Transforms.Add(new FileVector3(trans.X, trans.Y, trans.Z));

            foreach (var rot in keyFrame.Rotation)
                frame.Quaternion.Add(new FileVector4(rot.X, rot.Y, rot.Z, rot.W));

            return frame;
        }


    }

}