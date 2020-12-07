using Filetypes.RigidModel;
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
                output.Rotation.Add(new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]));
            return output;
        }


        public AnimationFile ConvertToFileFormat(Skeleton skeleton)
        {
            AnimationFile output = new AnimationFile();
            output.Header.AnimationType = 20;
            output.Header.AnimationTotalPlayTimeInSec = DynamicFrames.Count() / output.Header.FrameRate;
            output.Header.SkeletonName = skeleton.SkeletonName;

            // Mappings

            // Static

            // Dynamic


            return output;
        }


    }

}