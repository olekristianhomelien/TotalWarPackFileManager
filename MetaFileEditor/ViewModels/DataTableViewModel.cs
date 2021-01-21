using CommonDialogs.Common;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using MetaFileEditor.DataType;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace MetaFileEditor.ViewModels
{


    class DataTableRow
    {
        public List<ItemWrapper> Values { get; set; } = new List<ItemWrapper>();


        public string FileName { get { return DataItem.ParentFileName; } }
        public int ByteSize { get { return DataItem.Size; } }

        List<DbColumnDefinition> _fieldInfos;
        public MetaDataTagItem.Data DataItem { get; private set; }

        public int Index { get; private set; }
        public DataTableRow(int index, List<DbColumnDefinition> fieldInfos, MetaDataTagItem.Data dataItem)
        {
            _fieldInfos = fieldInfos;
            DataItem = dataItem;
            Index = index;
            Rebuild();
        }

        public string GetError()
        {
            for (int i = 0; i < Values.Count(); i++)
            {
                if (Values[i].Error != "")
                    return Values[i].Error;
            }

            return "";
        }

        public void Rebuild()
        {
            Values.Clear();
            var offset = 0;
            for (int i = 0; i < _fieldInfos.Count(); i++)
            {
                var parser = Filetypes.ByteParsing.ByteParserFactory.Create(_fieldInfos.ElementAt(i).Type);
                var result = parser.TryDecode(DataItem.Bytes, DataItem.Start + offset, out string value, out var bytesRead, out var Error);
                Values.Add(new ItemWrapper(this, value, Error));
                offset += bytesRead;
            }
        }


    }

    class ItemWrapper : NotifyPropertyChangedImpl
    {
        DataTableRow _parent;

        string _value;

        IByteParser _byteParser;

        public ItemWrapper(DataTableRow parent, string value, string error)
        {
            _parent = parent;
            _value = value;
            Error = error;
        }


        public ItemWrapper(string origianlValue, IByteParser byteParser)
        {
            _value = origianlValue;
            _byteParser = byteParser;
        }

        public string Value 
        {
            get
            {
                
                return _value;
            }
                
            set 
            {
                //_byteParser.
                //_parent.Rebuild();
            }
        }

        public string Error { get; set; }
    }

    class DataTableViewModel : NotifyPropertyChangedImpl
    {
        DataGrid _dataGirdRef;
        public void SetDataGridRef(DataGrid dataGirdRef)
        {
            _dataGirdRef = dataGirdRef;
    
        }



        public event ValueChangedDelegate<DataTableRow> SelectedRowChanged;
        DataTableRow _selectedItem;
        public DataTableRow SelectedItem { get { return _selectedItem; } set { SetAndNotify(ref _selectedItem, value, SelectedRowChanged); } }


        ICollectionView _rows;

        DbTableDefinitionViewModel _dbTableDefinition;
        public MetaDataTagItem CurrentFile { get; private set; }


        public DataTableViewModel(DbTableDefinitionViewModel dbTableDefinition)
        {
            _dbTableDefinition = dbTableDefinition;
            _dbTableDefinition.DefinitionChanged += _dbTableDefinition_DefinitionChanged;
        }

        private void _dbTableDefinition_DefinitionChanged(DbTableDefinition newValue)
        {
            Update();
        }

        public void SetCurrentFile(MetaDataTagItem file)
        {
            CurrentFile = file;
        }

        void Update()
        {
            if (_dataGirdRef == null)
                return;

           

            _dataGirdRef.Columns.Clear();


            var sizeColoumn = new DataGridTextColumn() { Header = "ByteSize"};
            sizeColoumn.Binding = new Binding("ByteSize");
            _dataGirdRef.Columns.Add(sizeColoumn);


            var fileNameColoumn = new DataGridTextColumn() { Header = "FileName" };
            fileNameColoumn.Binding = new Binding("FileName");
            _dataGirdRef.Columns.Add(fileNameColoumn);


            var index = 0;
            foreach (var columnDefinition in _dbTableDefinition.Definition.ColumnDefinitions)
            {
                var header = new DataGridTextColumn() { Header = columnDefinition.Name };

                var style = new Style(typeof(DataGridColumnHeader));
                style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, columnDefinition.Description));
                header.HeaderStyle = style;
                header.Binding = new Binding("Values[" + index + "].Value");
                _dataGirdRef.Columns.Add(header);
                index++;
            }


            var tableRows = new ObservableCollection<DataTableRow>();

            int counter = 0;
            foreach (var item in CurrentFile.DataItems)
            {
                tableRows.Add(new DataTableRow(counter++, _dbTableDefinition.Definition.ColumnDefinitions, item));
            }

            _rows = CollectionViewSource.GetDefaultView(tableRows);
            _dataGirdRef.ItemsSource = _rows;
        }




    }
}
