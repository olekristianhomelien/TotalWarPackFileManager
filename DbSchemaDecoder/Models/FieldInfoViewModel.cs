using CommonDialogs.Common;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DbSchemaDecoder.Models
{
    public class FieldInfoViewModel : NotifyPropertyChangedImpl
    {

        public int Index { get { return _index; } }
        bool _use = true;
        int _index;
        public bool Use { get { return _use; } set { _use = value; NotifyPropertyChanged(); } }
        public string Name { get { return _fieldInfo.Name; } set { _fieldInfo.Name = value; NotifyPropertyChanged(); } }
        public DbTypesEnum Type
        {
            get { return _fieldInfo.Type; }
            set
            {
                _fieldInfo.Type = value;
                NotifyPropertyChanged();
            }
        }

        public bool PrimaryKey { get { return _fieldInfo.IsKey; } set { _fieldInfo.IsKey = value; NotifyPropertyChanged(); } }
        public bool Optional { get { return _fieldInfo.IsOptional; } set { _fieldInfo.IsOptional = value; NotifyPropertyChanged(); } }
        public string ReferencedField { get { return _fieldInfo.FieldReference; } set { _fieldInfo.FieldReference = value; NotifyPropertyChanged(); } }
        public string ReferencedTable { get { return _fieldInfo.TableReference; } set { _fieldInfo.TableReference = value; NotifyPropertyChanged(); } }

        public FieldInfoViewModel(DbColumnDefinition info, int idx)
        {
            _index = idx;
            _fieldInfo = info;
        }

        DbColumnDefinition _fieldInfo;
        public DbColumnDefinition GetFieldInfo() { return _fieldInfo; }
        public void SetIndex(int idx) { _index = idx; }
    }
}
