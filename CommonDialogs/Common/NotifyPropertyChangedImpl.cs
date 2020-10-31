using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommonDialogs.Common
{
    public class NotifyPropertyChangedImpl
    {
        public bool DisableCallbacks { get; set; } = false;
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void ValueChangedDelegate<T>(T newSelectedSkeleton);

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void NotifyPropertyChanged<T>(T value, ValueChangedDelegate<T> valueChangedDelegate, [CallerMemberName] String propertyName = "")
        {
            NotifyPropertyChanged(propertyName);
            if (DisableCallbacks == false)
                valueChangedDelegate?.Invoke(value);
        }


        protected virtual void SetAndNotify<T>(ref T variable, T newValue, ValueChangedDelegate<T> valueChangedDelegate = null, [CallerMemberName] String propertyName = "")
        {
            variable = newValue;
            NotifyPropertyChanged(propertyName);
            if (DisableCallbacks == false)
                valueChangedDelegate?.Invoke(newValue);
        }
    }

    public class DisableCallbacks : IDisposable
    {
        NotifyPropertyChangedImpl _view;
        public DisableCallbacks(NotifyPropertyChangedImpl view)
        {
            _view = view;
            _view.DisableCallbacks = true;
        }

        public void Dispose()
        {
            _view.DisableCallbacks = false;
        }
    }
}
