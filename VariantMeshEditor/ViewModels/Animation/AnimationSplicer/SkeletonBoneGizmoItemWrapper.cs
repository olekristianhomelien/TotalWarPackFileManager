using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping;
using Viewer.Animation;
using Viewer.Gizmo;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer
{
    public class SkeletonBoneGizmoItemWrapper : GizmoItemWrapper, IDisposable
    {
        GameSkeleton _skeleton;
        int _boneIndex;
        AdvBoneMappingBone _mappableSkeletonBone;
        GizmoEditor _gizmoEditor;
        bool _hasBeenPositionBeenSet = false;

        public SkeletonBoneGizmoItemWrapper(GameSkeleton skeleton, int boneIndex, AdvBoneMappingBone mappableSkeletonBone, GizmoEditor gizmoEditor)
        {
            _skeleton = skeleton;
            _boneIndex = boneIndex;
            _mappableSkeletonBone = mappableSkeletonBone;
            _gizmoEditor = gizmoEditor;

            _gizmoEditor.RotateEvent += GizmoRotateEvent;
            _gizmoEditor.TranslateEvent += GizmoTranslateEvent;
        }

        public override void Dispose()
        {
            _gizmoEditor.TranslateEvent -= GizmoTranslateEvent;
            _gizmoEditor.RotateEvent -= GizmoRotateEvent;
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
            var oldRotationOffset = MathConverter.ToVector3(_mappableSkeletonBone.Settings.ContantRotationOffset);
            MathConverter.AssignFromVector3(_mappableSkeletonBone.Settings.ContantRotationOffset, oldRotationOffset + boneSpaceRotationVector);

            /*var m2 = (Matrix)gizmoRelativeMovementMatrix.Value2;
            m2.Decompose(out _, out Quaternion currentGizmoRotation, out _);
            currentGizmoRotation.ToAxisAngle(out Vector3 rotationAxis, out float rotationAngle);
            var boneSpaceRotationVector = rotationAxis * MathHelper.ToDegrees(rotationAngle);
            var oldRotationOffset = MathConverter.ToVector3(_mappableSkeletonBone.Settings.ContantRotationOffset);
            MathConverter.AssignFromVector3(_mappableSkeletonBone.Settings.ContantRotationOffset, oldRotationOffset + boneSpaceRotationVector);*/
        }

        public void OnTranslate(TransformationEventArgs gizmoRelativeMovementMatrix, Matrix axisMatrix)
        {
            ////var boneTransform = /* Matrix.CreateScale(-1, 1, 1) **/_skeleton.GetAnimatedWorldTranform(_boneIndex);
            ////boneTransform.Decompose(out _, out Quaternion boneRot, out _);
            //var invBoneRotation = Matrix.Invert(axisMatrix);// Quaternion.Inverse(boneRot);
            //
            ////Matrix relativeGizmoMovement = ((Matrix)gizmoRelativeMovementMatrix.Value);
            ////relativeGizmoMovement.Decompose(out Vector3 _, out Quaternion currentBoneRotation, out Vector3 _);
            //var gismoValue = (Vector3)gizmoRelativeMovementMatrix.Value;
            //
            //var boneLocalRotation = Vector3.Transform(gismoValue, invBoneRotation);
            //var current = MathConverter.ToVector3(_mappableSkeletonBone.Settings.ContantTranslationOffset);
            //MathConverter.AssignFromVector3(_mappableSkeletonBone.Settings.ContantTranslationOffset, boneLocalRotation + current);


            var gismoValue = (Vector3)gizmoRelativeMovementMatrix.Value2;
            var current = MathConverter.ToVector3(_mappableSkeletonBone.Settings.ContantTranslationOffset);
            MathConverter.AssignFromVector3(_mappableSkeletonBone.Settings.ContantTranslationOffset, gismoValue + current);
        }

        public override void Update(bool force = false)
        {
            if (!_hasBeenPositionBeenSet || force)
            {
                var transform = Matrix.CreateScale(-1, 1, 1) * _skeleton.GetAnimatedWorldTranform(_boneIndex);
                transform.Decompose(out Vector3 _, out Quaternion rot, out Vector3 translation);
                Position = translation;
                Orientation = rot;
                _hasBeenPositionBeenSet = true;
            }
        }

        private void GizmoRotateEvent(ITransformable transformable, TransformationEventArgs e)
        {
            var t = transformable as SkeletonBoneGizmoItemWrapper;
            t.OnRotate(e, _gizmoEditor.AxisMatrix);    
        }

        private void GizmoTranslateEvent(ITransformable transformable, TransformationEventArgs e)
        {
            var t = transformable as SkeletonBoneGizmoItemWrapper;
            t.OnTranslate(e, _gizmoEditor.AxisMatrix);
        }
    }
}
