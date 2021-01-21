using CommonDialogs.Common;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using GalaSoft.MvvmLight.Command;
using MetaFileEditor.DataType;
using MetaFileEditor.Views.MetadataTableViews;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MetaFileEditor.ViewModels
{

    class DbTableDefinitionViewModel : NotifyPropertyChangedImpl
    {
        public event ValueChangedDelegate<DbTableDefinition> DefinitionChanged;

        DbTableDefinition _definition = new DbTableDefinition();
        public DbTableDefinition Definition { get { return _definition; } set { SetAndNotify(ref _definition, value, DefinitionChanged); } }

        public void TriggerUpdates()
        {
            DefinitionChanged?.Invoke(Definition);
        }
    }


    class MainViewModel : NotifyPropertyChangedImpl
    {
        MetaDataFile _file;
        public MetaDataFile File { get { return _file; } set { SetAndNotify(ref _file, value); } }

        MetaDataTagItem _selectedMetaTagItem;
        public MetaDataTagItem SelectedMetaTagItem { get { return _selectedMetaTagItem; } set { SetAndNotify(ref _selectedMetaTagItem, value); OnItemSelected(value); }  }


        DataTableViewModel _dataTable;
        public DataTableViewModel DataTable { get { return _dataTable; } set { SetAndNotify(ref _dataTable, value); } }


        TableDefinitionEditorModel _tableDefinitionEditor;
        public TableDefinitionEditorModel TableDefinitionEditor { get { return _tableDefinitionEditor; } set { SetAndNotify(ref _tableDefinitionEditor, value); } }


        DbTableDefinitionViewModel _activeTableDefinition = new DbTableDefinitionViewModel();
        public DbTableDefinitionViewModel ActiveTableDefinition { get { return _activeTableDefinition; } set { SetAndNotify(ref _activeTableDefinition, value); } }


        public FieldExplorerController FieldExplorer { get; set; }

        public MainViewModel()
        {
            SchemaManager.Instance.GetTableDefinitionsForTable("", 123);

            TableDefinitionEditor = new TableDefinitionEditorModel(ActiveTableDefinition);
            DataTable = new DataTableViewModel(ActiveTableDefinition);
            FieldExplorer = new FieldExplorerController(DataTable, ActiveTableDefinition);
            
        }

        void OnItemSelected(MetaDataTagItem file)
        {
            DataTable.SetCurrentFile(file);

            // Find active schema from schema db
            ActiveTableDefinition.Definition.TableName = file.Name;
            ActiveTableDefinition.Definition.Version = file.Version;



            ActiveTableDefinition.DisableCallbacks = true;
            ActiveTableDefinition.Definition.ColumnDefinitions.Clear();
            ActiveTableDefinition.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "Version",
                    Description = "This is the version",
                    Type = DbTypesEnum.Integer
                });

            ActiveTableDefinition.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "PropBone",
                    Description = "This is a test PropBone",
                    Type = DbTypesEnum.Single
                });

            ActiveTableDefinition.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "OffsetX",
                    Description = "This is a test OffsetX",
                    Type = DbTypesEnum.Integer
                });

            ActiveTableDefinition.Definition.ColumnDefinitions.Add(
                new DbColumnDefinition()
                {
                    Name = "BoneName",
                    Description = "This is a test BoneName",
                    Type = DbTypesEnum.Integer
                });

            ActiveTableDefinition.DisableCallbacks = false;
            ActiveTableDefinition.TriggerUpdates();

            FieldExplorer.SetSelectedFile(file);
            FieldExplorer.TriggerUpdate();
        }
    }






   



}
