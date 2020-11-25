using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.Animation
{
    public class AnimationClip
    {
        public class KeyFrame
        {
            public List<Vector3> Translation { get; set; } = new List<Vector3>();
            public List<Quaternion> Rotation { get; set; } = new List<Quaternion>();
        }


        public List<int> StaticTranslationMappingID = new List<int>();
        public List<int> StaticRotationMappingID = new List<int>();
        public KeyFrame StaticFrame { get; set; } = null;


        // Keyframes/default pose
        public List<int> DynamicTranslationMappingID = new List<int>();
        public List<int> DynamicRotationMappingID = new List<int>();
        public List<KeyFrame> DynamicFrames = new List<KeyFrame>();

        public AnimationClip() { }

        public AnimationClip(AnimationFile file)
        {
            StaticTranslationMappingID = file.StaticTranslationMappingID.ToList();
            StaticRotationMappingID = file.StaticRotationMappingID.ToList();

            DynamicTranslationMappingID = file.DynamicTranslationMappingID.ToList();
            DynamicRotationMappingID = file.DynamicRotationMappingID.ToList();

            if (file.StaticFrame != null)
                StaticFrame = CreateKeyFrame(file.StaticFrame);

            foreach(var frame in file.DynamicFrames)
                DynamicFrames.Add(CreateKeyFrame(frame));
        }

        KeyFrame CreateKeyFrame(AnimationFile.Frame frame)
        {
            var output = new KeyFrame();
            foreach (var translation in frame.Transforms)
                output.Translation.Add(new Vector3(translation[0], translation[1], translation[2]));

            foreach (var rotation in frame.Quaternion)
                output.Rotation.Add(new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]));
            return output;
        }
    }


}


/*
         public class Frame
        {
            public List<float[]> Transforms { get; set; } = new List<float[]>();
            public List<short[]> Quaternion { get; set; } = new List<short[]>();
        }

        public class AnimationHeader
        {
            public uint AnimationType { get; set; }
            public uint Unknown0_alwaysOne { get; set; }
            public float FrameRate { get; set; }
            public string SkeletonName { get; set; }
            public uint Unknown1_alwaysZero { get; set; }
        }

        public BoneInfo[] Bones;

        // Version 7 spesific 
        public float AnimationTotalPlayTimeInSec { get; set; }

        public List<int> StaticTranslationMappingID = new List<int>();
        public List<int> StaticRotationMappingID = new List<int>();
        public Frame StaticFrame { get; set; } = null;


        // Keyframes/default pose
        public List<int> DynamicTranslationMappingID = new List<int>();
        public List<int> DynamicRotationMappingID = new List<int>();
        public List<Frame> DynamicFrames = new List<Frame>();
 
 
 */