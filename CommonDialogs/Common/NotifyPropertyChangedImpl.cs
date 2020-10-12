using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommonDialogs.Common
{
    public class NotifyPropertyChangedImpl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
