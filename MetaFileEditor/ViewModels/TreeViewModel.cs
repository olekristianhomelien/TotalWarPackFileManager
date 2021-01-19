using CommonDialogs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetaFileEditor.ViewModels
{
    class TreeViewModel : NotifyPropertyChangedImpl
    {
        string _filterText;
        public string FilterText { get { return _filterText; } set { SetAndNotify(ref _filterText, value); } }

        ICommand _clearFilterCommand;
        public ICommand ClearFilterCommand { get { return _clearFilterCommand; } set { SetAndNotify(ref _clearFilterCommand, value); } }
    }
}
