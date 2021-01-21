using CommonDialogs.Common;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using GalaSoft.MvvmLight.CommandWpf;
using MetaFileEditor.DataType;
using MetaFileEditor.ViewModels.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetaFileEditor.ViewModels
{
    class TableDefinitionEditor : NotifyPropertyChangedImpl
    {
        public ICommand RemoveDefinitionCommand { get; set; }
        public ICommand AddDefinitionCommand { get; set; }
        public ICommand MoveUpDefinitionCommand { get; set; }
        public ICommand MoveDownDefinitionCommand { get; set; }

        public ObservableCollection<FieldInfoViewModel> Rows { get; set; } = new ObservableCollection<FieldInfoViewModel>();

        public event ValueChangedDelegate<FieldInfoViewModel> SelectionChanged;
        FieldInfoViewModel _selectedItem;
        public FieldInfoViewModel SelectedItem { get { return _selectedItem; } set { SetAndNotify(ref _selectedItem, value, SelectionChanged); } }

        TableDefinitionModel _tableDefinitionModel;
        ActiveMetaDataContentModel _activeMetaDataContentModel;

        public TableDefinitionEditor(ActiveMetaDataContentModel activeMetaDataContentModel, TableDefinitionModel tableDefinitionViewModel)
        {
            AddDefinitionCommand = new RelayCommand(() => AddNewDefinitionItem());
            RemoveDefinitionCommand = new RelayCommand(OnRemoveSelected);
            _activeMetaDataContentModel = activeMetaDataContentModel;
            _tableDefinitionModel = tableDefinitionViewModel;

            _activeMetaDataContentModel.SelectedTagTypeChanged += OnSelectedTagTypeChanged;
        }


        void LoadDefintion(MetaDataTagItem newValue)
        {
            //SchemaManager.Instance.GetTableDefinitionsForTable("", 123);

            _tableDefinitionModel.Definition.TableName = newValue.Name;
            _tableDefinitionModel.Definition.Version = newValue.Version;

            _tableDefinitionModel.DisableCallbacks = true;
            _tableDefinitionModel.Definition.ColumnDefinitions.Clear();
            _tableDefinitionModel.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "Version",
                    Description = "This is the version",
                    Type = DbTypesEnum.Integer
                });

            _tableDefinitionModel.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "PropBone",
                    Description = "This is a test PropBone",
                    Type = DbTypesEnum.Single
                });

            _tableDefinitionModel.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "OffsetX",
                    Description = "This is a test OffsetX",
                    Type = DbTypesEnum.Integer
                });

            _tableDefinitionModel.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "BoneName",
                    Description = "This is a test BoneName",
                    Type = DbTypesEnum.Integer
                });

            _tableDefinitionModel.DisableCallbacks = false;
        }

        private void OnSelectedTagTypeChanged(MetaDataTagItem newValue)
        {
            LoadDefintion(newValue);
            Update();

            _tableDefinitionModel.TriggerUpdates();
        }

        public void AddNewDefinitionItem(DbTypesEnum type = DbTypesEnum.Integer)
        {
            if (_tableDefinitionModel.Definition == null)
                return;

            _tableDefinitionModel.Definition.ColumnDefinitions.Add(new DbColumnDefinition() { Name = "New Field", Type = type});
            _tableDefinitionModel.TriggerUpdates();
            Update();
        }

        void OnRemoveSelected()
        {
            if (_tableDefinitionModel.Definition == null || SelectedItem == null)
                return;

            var selectedViewModel = Rows.Where(X => X.InternalId == SelectedItem.InternalId).FirstOrDefault();
            if (selectedViewModel == null)
                return;

            _tableDefinitionModel.Definition.ColumnDefinitions.Remove(selectedViewModel.GetFieldInfo());
            _tableDefinitionModel.TriggerUpdates();
            Update();
        }

        void Update()
        {
            foreach(var row in Rows)
                row.PropertyChanged -= Row_PropertyChanged;
            Rows.Clear();
            foreach (var coloumDef in _tableDefinitionModel.Definition.ColumnDefinitions)
                Rows.Add(new FieldInfoViewModel(coloumDef));

            foreach (var row in Rows)
                row.PropertyChanged += Row_PropertyChanged;
        }

        private void Row_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _tableDefinitionModel.TriggerUpdates();
            Update();
        }
    }



    public class FieldInfoViewModel : NotifyPropertyChangedImpl
    {
        public Guid InternalId { get; set; } = Guid.NewGuid();
        bool _use = true;
        public bool Use { get { return _use; } set { _use = value; NotifyPropertyChanged(); } }
        public string Name { get { return _fieldInfo.Name; } set { _fieldInfo.Name = value; NotifyPropertyChanged(); } }
        public string Description { get { return _fieldInfo.Description; } set { _fieldInfo.Description = value; NotifyPropertyChanged(); } }
        public DbTypesEnum Type { get { return _fieldInfo.Type; } set { _fieldInfo.Type = value; NotifyPropertyChanged(); } }

        public FieldInfoViewModel(DbColumnDefinition fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }


        DbColumnDefinition _fieldInfo;
        public DbColumnDefinition GetFieldInfo() { return _fieldInfo; }
    }
}
