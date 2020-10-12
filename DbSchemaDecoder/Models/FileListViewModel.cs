using Common;
using CommonDialogs.Common;
using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    public class FileListViewModel : NotifyPropertyChangedImpl
    {
        string _searchFilter;
        public string SearchFilter
        {
            get
            {
                return _searchFilter;
            }
            set
            {
                _searchFilter = value;
                NotifyPropertyChanged();
            }
        }

        int _totalDbFiles;
        public int TotalDbFiles
        {
            get
            {
                return _totalDbFiles;
            }
            set
            {
                _totalDbFiles = value;
                NotifyPropertyChanged();
            }
        }


        int _dbFilesWithError;
        public int DbFilesWithError
        {
            get
            {
                return _dbFilesWithError;
            }
            set
            {
                _dbFilesWithError = value;
                NotifyPropertyChanged();
            }
        }

        bool _onlyShowTablesWithErrors;
        public bool OnlyShowTablesWithErrors
        {
            get
            {
                return _onlyShowTablesWithErrors;
            }
            set
            {
                _onlyShowTablesWithErrors = value;
                NotifyPropertyChanged();
            }
        }

        bool _onlyShowUniqueTables;
        public bool OnlyShowUniqueTables
        {
            get
            {
                return _onlyShowUniqueTables;
            }
            set
            {
                _onlyShowUniqueTables = value;
                NotifyPropertyChanged();
            }
        }

        bool _onlyShowTablesWithContent;
        public bool OnlyShowTablesWithContent
        {
            get
            {
                return _onlyShowTablesWithContent;
            }
            set
            {
                _onlyShowTablesWithContent = value;
                NotifyPropertyChanged();
            }
        }

        public int _itemsInFilter = 0;
        public int ItemsInFilter
        {
            get
            {
                return _itemsInFilter;
            }
            set
            {
                _itemsInFilter = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<DatabaseFileViewModel> FileList { get; set; } = new ObservableCollection<DatabaseFileViewModel>();

    }
}
