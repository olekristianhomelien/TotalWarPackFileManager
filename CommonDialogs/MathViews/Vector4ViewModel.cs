﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonDialogs.MathViews
{
    public class Vector4ViewModel : Vector3ViewModel
    {
        public override event ValueChangedDelegate<Vector3ViewModel> OnValueChanged;

        DoubleViewModel _w = new DoubleViewModel();
        public DoubleViewModel W
        {
            get { return _w; }
            set { SetAndNotify(ref _w, value); OnValueChanged?.Invoke(this); }
        }

        public void SetValue(double value)
        {
            X.Value = value;
            Y.Value = value;
            Z.Value = value;
            W.Value = value;
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}, {W}";
        }
    }
}
