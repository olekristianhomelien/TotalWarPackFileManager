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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{
    public class DbTableDefinitionController : NotifyPropertyChangedImpl
    {
        public ObservableCollection<CaSchemaEntry> CaMetaData { get; set; } = new ObservableCollection<CaSchemaEntry>();
        public ObservableCollection<FieldInfoViewModel> TableTypeInformationRows { get; set; } = new ObservableCollection<FieldInfoViewModel>();

        FieldInfoViewModel _selectedTypeInformationRow;
        public FieldInfoViewModel SelectedTypeInformationRow
        {
            get { return _selectedTypeInformationRow; }
            set
            {
                _selectedTypeInformationRow = value;
                _windowState.SelectedDbSchemaRow = _selectedTypeInformationRow;
                NotifyPropertyChanged();
            }
        }

        public ICommand DbDefinitionRemovedCommand { get; private set; }
        public ICommand DbDefinitionRemovedAllCommand { get; private set; }
        public ICommand DbDefinitionMovedUpCommand { get; private set; }
        public ICommand DbDefinitionMovedDownCommand { get; private set; }
        public ICommand CreateNewDbDefinitionCommand { get; private set; }
        public ICommand DeselectCommand { get; private set; }
        public ICommand DbMetaDataAppliedCommand { get; private set; }
        public ICommand OnRemoveMetaDataCommand { get; private set; }

        public ICommand OnSaveCommand { get; private set; }

        WindowState _windowState;

        public DbTableDefinitionController(WindowState windowState)
        {
            _windowState = windowState;
            _windowState.OnSetDbSchema +=(sender, newSchema) => { Set(newSchema); };
            _windowState.OnNewDbSchemaRowCreated += AppendRowOfTypeEventHandler;
            _windowState.OnCaSchemaLoaded += (sender, caSchemas) => { UpdateMetaDataList(); };

            DbDefinitionRemovedCommand = new RelayCommand(OnDbDefinitionRemoved);
            DbDefinitionRemovedAllCommand = new RelayCommand(OnDbDefinitionRemovedAll);
            DbDefinitionMovedUpCommand = new RelayCommand(OnDbDefinitionMovedUp);
            DbDefinitionMovedDownCommand = new RelayCommand(OnDbDefinitionMovedDown);
            CreateNewDbDefinitionCommand = new RelayCommand(OnCreateNewDbDefinitionCommand);
            DeselectCommand = new RelayCommand(OnDeselectCommand);
            OnRemoveMetaDataCommand = new RelayCommand(OnRemoveMetaData);
            DbMetaDataAppliedCommand = new RelayCommand<CaSchemaEntry>(OnMetaDataApplied);
            OnSaveCommand = new RelayCommand(OnSave);
        }

        void OnSave()
        {
            DBFileHeader header = PackedFileDbCodec.readHeader(_windowState.SelectedFile.DbFile);
            var def = new DbTableDefinition()
            {
                ColumnDefinitions = TableTypeInformationRows.Select(x => x.GetFieldInfo()).ToList(),
                Version = header.Version,
                TableName = _windowState.SelectedFile.TableType
            };

            SchemaManager.Instance.UpdateCurrentTableDefinition(def);

            _windowState.TriggerErrorCheck();
        }

        void UpdateMetaDataList()
        {
            if (_windowState.CaSchema == null || TableTypeInformationRows == null)
                return;

            CaMetaData.Clear();
            var unused = BuildUnusedMetaData(_windowState.CaSchema, TableTypeInformationRows);
            foreach (var item in unused)
                CaMetaData.Add(item);
        }

        List<CaSchemaEntry> BuildUnusedMetaData(List<CaSchemaEntry> caSchemaEntries, Collection<FieldInfoViewModel> fields)
        {
            List<CaSchemaEntry> notUsed = new List<CaSchemaEntry>();
            List<CaSchemaEntry> used = new List<CaSchemaEntry>();
            foreach (var caSchema in caSchemaEntries)
            {
                var isUsed = false;
                foreach (var field in fields)
                {
                    if (field.Name == caSchema.name)
                    {
                        isUsed = true;
                        break;
                    }
                }

                var clone = (CaSchemaEntry)caSchema.Clone();
                if (isUsed)
                {
                    clone.name += " {Used}";
                    used.Add(clone);
                }
                else
                {
                    notUsed.Add(clone);
                }
            }

            notUsed = notUsed.OrderBy(x=>x.name).ToList();
            used = used.OrderBy(x => x.name).ToList();

            foreach (var item in used)
                notUsed.Add(item);
            return notUsed;
        }


        bool AreEqual(List<DbColumnDefinition> newItems, ObservableCollection<FieldInfoViewModel> oldItems)
        {
            if (newItems.Count != oldItems.Count)
                return false;

            for (int i = 0; i < newItems.Count; i++)
            {
                var newItem = newItems[i];
                var oldItem = oldItems[i];
                if (newItem != oldItem.GetFieldInfo())
                    return false;
            }

            return true;
        }

        public void Set(List<DbColumnDefinition> fields)
        {
            // Avoid circular triggering
            if (AreEqual(fields, TableTypeInformationRows))
                return;

            TableTypeInformationRows.Clear();
            for (int i = 0; i < fields.Count(); i++)
            {
                var newFieldInfoViewModel = new FieldInfoViewModel(fields[i], i + 1);
                newFieldInfoViewModel.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
                TableTypeInformationRows.Add(newFieldInfoViewModel);
            }

            RecomputeIndexes();
            UpdateMetaDataList();
            OnDefinitionChanged();
        }

        private void NewFieldInfoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDefinitionChanged();
        }

        private void OnDefinitionChanged()
        {
            _windowState.DbSchemaFields = TableTypeInformationRows.Select(x => x.GetFieldInfo()).ToList();
        }

        private void OnCreateNewDbDefinitionCommand()
        {
            DbColumnDefinition typeDef = new DbColumnDefinition()
            {
                Name = "Column_" + TableTypeInformationRows.Count() + 1,                
                Type = DbTypesEnum.String_ascii
            };

            var item = new FieldInfoViewModel(typeDef, TableTypeInformationRows.Count() + 1);
            item.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
            TableTypeInformationRows.Add(item);
            OnDefinitionChanged();
        }

        private void OnDbDefinitionRemoved()
        {
            if (SelectedTypeInformationRow == null)
                return;
            var index = TableTypeInformationRows.IndexOf(SelectedTypeInformationRow);
            TableTypeInformationRows.RemoveAt(index);
            UpdateMetaDataList();
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        private void OnDbDefinitionRemovedAll()
        {
            TableTypeInformationRows.Clear();
            UpdateMetaDataList();
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        private void OnDbDefinitionMovedUp()
        {
            if (SelectedTypeInformationRow == null)
                return;
            var index = TableTypeInformationRows.IndexOf(SelectedTypeInformationRow);
            if (index == 0)
                return;
            var selectedRef = SelectedTypeInformationRow;
            TableTypeInformationRows.RemoveAt(index);
            TableTypeInformationRows.Insert(index - 1, selectedRef);
            SelectedTypeInformationRow = TableTypeInformationRows[index - 1];
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        private void OnDbDefinitionMovedDown()
        {
            if (SelectedTypeInformationRow == null)
                return;
            var index = TableTypeInformationRows.IndexOf(SelectedTypeInformationRow);
            if (index == TableTypeInformationRows.Count - 1)
                return;
            var selectedRef = SelectedTypeInformationRow;
            TableTypeInformationRows.RemoveAt(index);
            TableTypeInformationRows.Insert(index + 1, selectedRef);
            SelectedTypeInformationRow = TableTypeInformationRows[index + 1];
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        void OnDeselectCommand()
        {
            SelectedTypeInformationRow = null;
        }

        void OnRemoveMetaData()
        {
            if (SelectedTypeInformationRow != null)
            {
                SelectedTypeInformationRow.Name = "Unknown" + SelectedTypeInformationRow.Index;
                SelectedTypeInformationRow.Optional = false;
                SelectedTypeInformationRow.PrimaryKey = false;
                SelectedTypeInformationRow.ReferencedTable = "";
                UpdateMetaDataList();
            }
        }

        void OnMetaDataApplied(CaSchemaEntry metaData)
        {
            if (SelectedTypeInformationRow != null)
            {
                SelectedTypeInformationRow.Name = metaData.name;
                SelectedTypeInformationRow.Optional = metaData.required == "1";
                SelectedTypeInformationRow.PrimaryKey = metaData.primary_key == "1";

                if (!string.IsNullOrWhiteSpace(metaData.column_source_table) && !string.IsNullOrWhiteSpace(metaData.column_source_column))
                    SelectedTypeInformationRow.ReferencedTable = metaData.column_source_table + "." + metaData.column_source_column;
                else
                    SelectedTypeInformationRow.ReferencedTable = "";
                UpdateMetaDataList();
            }
        }

        void RecomputeIndexes()
        {
            for (int i = 0; i < TableTypeInformationRows.Count; i++)
            {
                TableTypeInformationRows[i].SetIndex(i + 1);
                if (TableTypeInformationRows[i].Name == "unknown")
                    TableTypeInformationRows[i].Name = "unknown_" + i;
            }
        }

        void AppendRowOfTypeEventHandler(object e, DbTypesEnum type)
        {
            DbColumnDefinition typeDef = new DbColumnDefinition()
            {
                Name = "Column_" + TableTypeInformationRows.Count() + 1,
                Type = type
            };
            var newFieldInfoViewModel = new FieldInfoViewModel(typeDef, 99);
            newFieldInfoViewModel.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
            TableTypeInformationRows.Add(newFieldInfoViewModel);
            
            RecomputeIndexes();
            OnDefinitionChanged();
        }
    }
}
