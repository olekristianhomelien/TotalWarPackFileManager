using CommonDialogs.MathViews;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariantMeshEditor.Util
{
    public static class MathConverter
    {
        public static Vector3 ToVector(Vector3ViewModel vector3ViewModel)
        {
            return new Vector3((float)vector3ViewModel.X.Value, (float)vector3ViewModel.Y.Value, (float)vector3ViewModel.Z.Value);
        }

        public static Quaternion ToQuaternion(Vector3ViewModel vector3ViewModel)
        {
            var x = Matrix.CreateRotationX(MathHelper.ToRadians((float)vector3ViewModel.X.Value));
            var y = Matrix.CreateRotationY(MathHelper.ToRadians((float)vector3ViewModel.Y.Value));
            var z = Matrix.CreateRotationZ(MathHelper.ToRadians((float)vector3ViewModel.Z.Value));
            var rotationMatrix = x * y * z;
            return Quaternion.CreateFromRotationMatrix(rotationMatrix);
        }

        public static Quaternion ToQuaternion(FileVector4 vector4)
        {
            return new Quaternion(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }
    }
}
