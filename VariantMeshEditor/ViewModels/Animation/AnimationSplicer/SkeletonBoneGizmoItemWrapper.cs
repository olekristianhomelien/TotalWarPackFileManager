using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using Viewer.Animation;
using Viewer.Gizmo;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class SkeletonBoneGizmoItemWrapper : GizmoItemWrapper
    {
        GameSkeleton _skeleton;
        int _boneIndex;
        MappableSkeletonBone _mappableSkeletonBone;

        public SkeletonBoneGizmoItemWrapper(GameSkeleton skeleton, int boneIndex, MappableSkeletonBone mappableSkeletonBone)
        {
            _skeleton = skeleton;
            _boneIndex = boneIndex;
            _mappableSkeletonBone = mappableSkeletonBone;
        }

        public void OnRotate(TransformationEventArgs gizmoRelativeMovementMatrix, Matrix axisMatrix)
        {
            //var boneTransform = Matrix.CreateScale(-1, 1, 1) * _skeleton.GetAnimatedWorldTranform(_boneIndex);
            //boneTransform.Decompose(out Vector3 _, out Quaternion boneRot, out Vector3 _);
            var invBoneRotation = Matrix.Invert(axisMatrix);

            Matrix relativeGizmoMovement = ((Matrix)gizmoRelativeMovementMatrix.Value);
            relativeGizmoMovement.Decompose(out _, out Quaternion currentGizmoRotation, out _);

            currentGizmoRotation.ToAxisAngle(out Vector3 rotationAxis, out float rotationAngle);
            var boneSpaceRotationAxis = Vector3.Transform(rotationAxis, invBoneRotation);

            var boneSpaceRotationVector = boneSpaceRotationAxis * MathHelper.ToDegrees(rotationAngle);
            var oldRotationOffset = MathConverter.ToVector3(_mappableSkeletonBone.ContantRotationOffset);
            MathConverter.AssignFromVector3(_mappableSkeletonBone.ContantRotationOffset, oldRotationOffset + boneSpaceRotationVector);
        }

        public void OnTranslate(TransformationEventArgs gizmoRelativeMovementMatrix, Matrix axisMatrix)
        {
            //var boneTransform = /* Matrix.CreateScale(-1, 1, 1) **/_skeleton.GetAnimatedWorldTranform(_boneIndex);
            //boneTransform.Decompose(out _, out Quaternion boneRot, out _);
            var invBoneRotation = Matrix.Invert(axisMatrix);// Quaternion.Inverse(boneRot);

            //Matrix relativeGizmoMovement = ((Matrix)gizmoRelativeMovementMatrix.Value);
            //relativeGizmoMovement.Decompose(out Vector3 _, out Quaternion currentBoneRotation, out Vector3 _);
            var gismoValue = (Vector3)gizmoRelativeMovementMatrix.Value;

            //gismoValue.X *= -1; 
            var gismoValueCopy = gismoValue;
            //gismoValue.X = gismoValueCopy.Y;
            //gismoValue.Y = gismoValueCopy.Z;
            //gismoValue.Z = gismoValueCopy.X;
            //gismoValue.X *= -1;

            var boneLocalRotation = Vector3.Transform(gismoValue, invBoneRotation);
            var current = MathConverter.ToVector3(_mappableSkeletonBone.ContantTranslationOffset);
            MathConverter.AssignFromVector3(_mappableSkeletonBone.ContantTranslationOffset, boneLocalRotation + current);

        }

        public bool isFIrstTime = true;
        public void Update(bool force = false)
        {
            if (isFIrstTime || force)
            {
                var transform = Matrix.CreateScale(-1, 1, 1) * _skeleton.GetAnimatedWorldTranform(_boneIndex);
                transform.Decompose(out Vector3 _, out Quaternion rot, out Vector3 translation);
                Position = translation;
                Orientation = rot;
                isFIrstTime = false;
            }
        }
    }
}
