using CommonDialogs.Common;
using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.Codecs;
using Filetypes.DB;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace DbSchemaDecoder.Controllers
{

    public class NextItemControllerItem : NotifyPropertyChangedImpl
    {
        SolidColorBrush _backgroundColour = new SolidColorBrush(Colors.White);
        public SolidColorBrush BackgroundColour
        {
            get { return _backgroundColour; }
            set
            {
                _backgroundColour = value;
                NotifyPropertyChanged();
            }
        }

        string _CustomDisplayText;
        public string CustomDisplayText
        {
            get { return _CustomDisplayText; }
            set
            {
                _CustomDisplayText = value;
                NotifyPropertyChanged();
            }
        }

        string _valueText;
        public string ValueText
        {
            get { return _valueText; }
            set
            {
                _valueText = value;
                NotifyPropertyChanged();
            }
        }

        string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                _buttonText = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand CustomButtonPressedCommand { get;  set; }
        public DbTypesEnum EnumValue { get; set; }

    }

    public class NextItemController : NotifyPropertyChangedImpl
    {
        string _helperText;
        public string HelperText
        {
            get { return _helperText; }
            set
            {
                _helperText = value;
                NotifyPropertyChanged();
            }
        }


        WindowState _windowState;
        public List<NextItemControllerItem> Items { get; set; } = new List<NextItemControllerItem>();
        public NextItemController(WindowState windowState)
        {
            Create(DbTypesEnum.String_ascii);
            Create(DbTypesEnum.Optstring_ascii);
            Create(DbTypesEnum.String);
            Create(DbTypesEnum.Optstring);
            Create(DbTypesEnum.Int64);
            Create(DbTypesEnum.Integer);
            Create(DbTypesEnum.Single);
            Create(DbTypesEnum.Short);
            Create(DbTypesEnum.Byte);
            Create(DbTypesEnum.Boolean);

            _windowState = windowState;
            _windowState.OnFileSelected += (sender, file) => { Update(); };
            _windowState.OnSetDbSchema += (sender, schama) => {  Update(); };
            _windowState.OnSelectedDbSchemaRowChanged += (sender, item) => { Update(); };
        }


        void Create(DbTypesEnum enumValue)
        {
            var type = ByteParserFactory.Create(enumValue);
            NextItemControllerItem viewModel = new NextItemControllerItem();
            viewModel.EnumValue = enumValue;
            viewModel.CustomDisplayText = type.TypeName;
            viewModel.ButtonText = "Add";
            viewModel.CustomButtonPressedCommand = new RelayCommand<NextItemControllerItem>(OnButtonPressed);
            Items.Add(viewModel);
        }

        void UpdateViewModel(NextItemControllerItem viewModelRef, byte[] data, int index)
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

        public void OnButtonPressed(NextItemControllerItem val)
        {
            if (_windowState.SelectedDbSchemaRow != null)
                _windowState.SelectedDbSchemaRow.Type = val.EnumValue;
            else
                _windowState.TriggerNewDbSchemaRowCreated(this, val.EnumValue);
        }

        void Update()
        {
            if (_windowState.DbSchemaFields == null || _windowState.SelectedFile == null)
                return;

            if (_windowState.SelectedDbSchemaRow != null)
            {
                HelperText = $"Update type for field '{_windowState.SelectedDbSchemaRow.Name}' at Index '{_windowState.SelectedDbSchemaRow.Index}'";
                Items.ForEach(x => x.ButtonText = "Update");
            }
            else
            {
                HelperText = "Create a new field";
                Items.ForEach(x => x.ButtonText = "Add");
            }

            DBFileHeader header = PackedFileDbCodec.readHeader(_windowState.SelectedFile.DbFile);
            int index = header.Length;
            var endIndex = _windowState.DbSchemaFields.Count();
            if (_windowState.SelectedDbSchemaRow != null)
                endIndex = _windowState.SelectedDbSchemaRow.Index - 1;
            for (int i = 0; i < endIndex; i++)
            {
                if (i < _windowState.DbSchemaFields.Count)
                {
                    var byteParserType = _windowState.DbSchemaFields[i].Type;
                    var parser = ByteParserFactory.Create(byteParserType);
                    parser.TryDecode(_windowState.SelectedFile.DbFile.Data, index, out _, out var bytesRead, out _);
                    index += bytesRead;
                }
            }

            for (int i = 0; i < Items.Count; i++)
                UpdateViewModel(Items[i], _windowState.SelectedFile.DbFile.Data, index);
        }

    }
}
