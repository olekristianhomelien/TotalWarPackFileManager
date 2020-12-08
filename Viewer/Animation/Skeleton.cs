using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Viewer.Animation
{
    public class Skeleton
    {
        public Matrix[] Transform { get; private set; }
        public Vector3[] Translation { get; private set; }
        public Quaternion[] Rotation { get; private set; }
        public Matrix[] WorldTransform { get; private set; }
        public int[] ParentBoneId { get; private set; }
        public string[] BoneNames { get; private set; }
        public int BoneCount { get; set; }
        public string SkeletonName { get; set; }

        public Skeleton(AnimationFile skeletonFile)
        {
            BoneCount = skeletonFile.Bones.Count();
            Transform = new Matrix[BoneCount];
            Translation = new Vector3[BoneCount];
            Rotation = new Quaternion[BoneCount];
            WorldTransform = new Matrix[BoneCount];
            ParentBoneId = new int[BoneCount];
            BoneNames = new string[BoneCount];
            SkeletonName = skeletonFile.Header.SkeletonName;

            for (int i = 0; i < BoneCount; i++)
            {
                ParentBoneId[i] = skeletonFile.Bones[i].ParentId;
                BoneNames[i] = skeletonFile.Bones[i].Name;
            }

            int skeletonWeirdIndex = 0;
            for (int i = 0; i < BoneCount; i++)
            {
                var quat = new Quaternion(
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i].X,
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i].Y,
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i].Z,
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i].W);
                //quat.Normalize();
              
                Rotation[i] = quat;

                var rotationMatrix = Matrix.CreateFromQuaternion(quat);
                var translationMatrix =  Matrix.CreateTranslation(
                            skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i].X,
                            skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i].Y,
                            skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i].Z);

                Translation[i] = new Vector3(
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i].X,
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i].Y, 
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i].Z);

                var transform = rotationMatrix * translationMatrix;

                Transform[i] = transform;
                WorldTransform[i] = transform;
            }


            for (int i = 0; i < BoneCount; i++)
            {
                var parentIndex = skeletonFile.Bones[i].ParentId;
                if (parentIndex == -1)
                    continue;
                WorldTransform[i] = WorldTransform[i] * WorldTransform[parentIndex];
            }
        }

        public int GetBoneIndex(string name)
        {
            for (int i = 0; i < BoneNames.Count(); i++)
            {
                if (BoneNames[i] == name)
                    return i;
            }

            return -1;
        }
    }
}
