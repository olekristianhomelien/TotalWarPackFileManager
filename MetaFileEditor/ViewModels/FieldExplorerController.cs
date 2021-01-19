using CommonDialogs.Common;
using Filetypes.ByteParsing;
using GalaSoft.MvvmLight.CommandWpf;
using MetaFileEditor.DataType;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WpfHexaEditor.Core;

namespace MetaFileEditor.ViewModels
{


    class SingleFieldExplporer : NotifyPropertyChangedImpl
    {
        SolidColorBrush _backgroundColour = new SolidColorBrush(Colors.White);
        public SolidColorBrush BackgroundColour
        {
            get { return _backgroundColour; }
            set
            {
                SetAndNotify(ref _backgroundColour, value);
            }
        }

        string _CustomDisplayText;
        public string CustomDisplayText
        {
            get { return _CustomDisplayText; }
            set
            {
                SetAndNotify(ref _CustomDisplayText, value);
            }
        }

        string _valueText;
        public string ValueText
        {
            get { return _valueText; }
            set
            {
                SetAndNotify(ref _valueText, value);
            }
        }

        string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                SetAndNotify(ref _buttonText, value);
            }
        }

        public ICommand CustomButtonPressedCommand { get; set; }
        public DbTypesEnum EnumValue { get; set; }

    }

    class FieldExplorerController : NotifyPropertyChangedImpl
    {

        DbTableDefinitionViewModel _tableDefinition;
        DataTableViewModel _dataTableView;

        string _helperText;
        public string HelperText
        {
            get { return _helperText; }
            set{ SetAndNotify(ref _helperText, value);}
        }

        public ObservableCollection<SingleFieldExplporer> Fields { get; set; } = new ObservableCollection<SingleFieldExplporer>();

        //HexEdit.Stream = new MemoryStream(selectedFile.DbFile.Data);
        MemoryStream _byteStream;
        public MemoryStream ByteStream
        {
            get { return _byteStream; }
            set { SetAndNotify(ref _byteStream, value); }
        }

        //int _byteSelectionEnd;
        //public int ByteSelectionEnd
        //{
        //    get { return _byteSelectionEnd; }
        //    set { SetAndNotify(ref _byteSelectionEnd, value); }
        //
        //}


        List<CustomBackgroundBlock> _backgroundBlocks = new List<CustomBackgroundBlock>();
        public List<CustomBackgroundBlock> BackgroundBlocks
        {
            get { return _backgroundBlocks; }
            set { SetAndNotify(ref _backgroundBlocks, value); }
        }

        //
        public FieldExplorerController(DataTableViewModel dataTableView, DbTableDefinitionViewModel tableDefinition)
        {
            dataTableView.SelectedRowChanged += DataTableView_SelectedRowChanged;




            Create(DbTypesEnum.String_ascii);
            Create(DbTypesEnum.Optstring_ascii);
            Create(DbTypesEnum.String);
            Create(DbTypesEnum.Optstring);
            Create(DbTypesEnum.Int64);  
            Create(DbTypesEnum.Integer);
            Create(DbTypesEnum.Single);
            Create(DbTypesEnum.Float16);
            Create(DbTypesEnum.Short);
            Create(DbTypesEnum.Byte);
            Create(DbTypesEnum.Boolean);

            _dataTableView = dataTableView;
            _tableDefinition = tableDefinition;

        }

        private void DataTableView_SelectedRowChanged(DataTableRow newValue)
        {
            if (newValue == null)
                return;
            Update(newValue.DataItem, _tableDefinition);
        }

        public void TriggerUpdate()
        {
            var item = _dataTableView.Rows.FirstOrDefault();
            if (item != null)
                Update(item.DataItem, _tableDefinition);
        }

        void Create(DbTypesEnum enumValue)
        {
            var type = ByteParserFactory.Create(enumValue);
            SingleFieldExplporer newItem = new SingleFieldExplporer();
            newItem.EnumValue = enumValue;
            newItem.CustomDisplayText = type.TypeName;
            newItem.ButtonText = "Add";
            //newItem.CustomButtonPressedCommand = new RelayCommand<SingleFieldExplporer>(OnButtonPressed);
            Fields.Add(newItem);
        }

        public void Update(MetaDataTagItem.Data data, DbTableDefinitionViewModel tableDef)
        {

            var tBackgroundBlocks = new List<CustomBackgroundBlock>();
            ByteStream = new MemoryStream(data.Bytes, data.Start, data.Size);

            int counter = 0;
            var endIndex = tableDef.Definition.ColumnDefinitions.Count();
            int index = data.Start;
            for (int i = 0; i < endIndex; i++)
            {
                if (i < endIndex)
                {
                    var byteParserType = tableDef.Definition.ColumnDefinitions[i].Type;
                    var parser = ByteParserFactory.Create(byteParserType);
                    parser.TryDecode(data.Bytes, index, out _, out var bytesRead, out _);
                    index += bytesRead;


                    var block = new CustomBackgroundBlock()
                    {
                        Description = tableDef.Definition.ColumnDefinitions[i].Name,
                        Color = counter % 2 == 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Yellow),
                        Length = bytesRead,
                        StartOffset = index - bytesRead - data.Start,
                        
                    };
                    tBackgroundBlocks.Add(block);

                    counter++;

                }
            }

            BackgroundBlocks = tBackgroundBlocks;


            for (int i = 0; i < Fields.Count; i++)
                UpdateViewModel(Fields[i], data.Bytes, index);
        }

        void UpdateViewModel(SingleFieldExplporer viewModelRef, byte[] data, int index)
        {
            var parser = ByteParserFactory.Create(viewModelRef.EnumValue);
            var result = parser.TryDecode(data, index, out string value, out var _, out string error);
            if (result == false)
            {
                viewModelRef.ValueText = "Error:" + error;
                viewModelRef.BackgroundColour = new SolidColorBrush(Colors.Pink);
            }
            else
            {
                viewModelRef.ValueText = value;
                viewModelRef.BackgroundColour = new SolidColorBrush(Colors.White);
            }

            if (value == null)
                return;
        }

        void Update()
        {
            //if (_windowState.DbSchemaFields == null || _windowState.SelectedFile == null)
            //    return;
            //
            //if (_windowState.SelectedDbSchemaRow != null)
            //{
            //    HelperText = $"Update type for field '{_windowState.SelectedDbSchemaRow.Name}' at Index '{_windowState.SelectedDbSchemaRow.Index}'";
            //    Items.ForEach(x => x.ButtonText = "Update");
            //}
            //else
            //{
            //    HelperText = "Create a new field";
            //    Items.ForEach(x => x.ButtonText = "Add");
            //}
            //
            //DBFileHeader header = PackedFileDbCodec.readHeader(_windowState.SelectedFile.DbFile);
            //int index = header.Length;
            //var endIndex = _windowState.DbSchemaFields.Count();
            //if (_windowState.SelectedDbSchemaRow != null)
            //    endIndex = _windowState.SelectedDbSchemaRow.Index - 1;
            //for (int i = 0; i < endIndex; i++)
            //{
            //    if (i < _windowState.DbSchemaFields.Count)
            //    {
            //        var byteParserType = _windowState.DbSchemaFields[i].Type;
            //        var parser = ByteParserFactory.Create(byteParserType);
            //        parser.TryDecode(_windowState.SelectedFile.DbFile.Data, index, out _, out var bytesRead, out _);
            //        index += bytesRead;
            //    }
            //}
            //
            //for (int i = 0; i < Items.Count; i++)
            //    UpdateViewModel(Items[i], _windowState.SelectedFile.DbFile.Data, index);
        }   //

    }
}
