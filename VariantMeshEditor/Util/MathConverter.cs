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
        public static Vector3 ToVector3(Vector3ViewModel vector3ViewModel)
        {
            float x = (float)Math.Round(vector3ViewModel.X.Value, 10);
            float y = (float)Math.Round(vector3ViewModel.Y.Value, 10);
            float z = (float)Math.Round(vector3ViewModel.Z.Value, 10);
            return new Vector3(x, y, z);
        }

        public static Quaternion ToQuaternion(Vector4ViewModel vector4ViewModel)
        {


            var q = new Quaternion((float)vector4ViewModel.X.Value, (float)vector4ViewModel.Y.Value, (float)vector4ViewModel.Z.Value, (float)vector4ViewModel.W.Value);
            q.Normalize();
            return q;
        }

        public static void AssignFromQuaternion(Vector4ViewModel vector4ViewModel, Quaternion quaternion)
        {
            vector4ViewModel.X.Value = quaternion.X;
            vector4ViewModel.Y.Value = quaternion.Y;
            vector4ViewModel.Z.Value = quaternion.Z;
            vector4ViewModel.W.Value = quaternion.W;
        }

        public static void AssignFromVector3(Vector3ViewModel vector3ViewModel, Vector3 vector)
        {
            float x = (float)Math.Round(vector.X, 6);
            float y = (float)Math.Round(vector.Y, 6);
            float z = (float)Math.Round(vector.Z, 6);

            vector3ViewModel.X.Value = x;
            vector3ViewModel.Y.Value = y;
            vector3ViewModel.Z.Value = z;
        }

        public static Quaternion ToQuaternion(FileVector4 vector4)
        {
            return new Quaternion(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }
    }
}
