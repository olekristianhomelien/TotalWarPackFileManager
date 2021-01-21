using CommonDialogs.Common;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetaFileEditor.ViewModels
{
    class TableDefinitionEditorModel : NotifyPropertyChangedImpl
    {
        public ICommand RemoveDefinitionCommand { get; set; }
        public ICommand AddDefinitionCommand { get; set; }

        public ICommand MoveUpDefinitionCommand { get; set; }
        public ICommand MoveDownDefinitionCommand { get; set; }

        ObservableCollection<FieldInfoViewModel> _rows = new ObservableCollection<FieldInfoViewModel>();
        public ObservableCollection<FieldInfoViewModel> Rows { get { return _rows; } set { SetAndNotify(ref _rows, value); } }

        public event ValueChangedDelegate<FieldInfoViewModel> SelectionChanged;
        FieldInfoViewModel _selectedItem;
        public FieldInfoViewModel SelectedItem { get { return _selectedItem; } set { SetAndNotify(ref _selectedItem, value, SelectionChanged); } }

        DbTableDefinitionViewModel _tableDefinitionViewModel;

        public TableDefinitionEditorModel(DbTableDefinitionViewModel tableDefinitionViewModel)
        {
            AddDefinitionCommand = new RelayCommand(OnAdd);
            RemoveDefinitionCommand = new RelayCommand(OnRemoveSelected);


            _tableDefinitionViewModel = tableDefinitionViewModel;
            _tableDefinitionViewModel.DefinitionChanged += _tableDefinitionViewModel_DefinitionChanged;
        }

        private void _tableDefinitionViewModel_DefinitionChanged(DbTableDefinition newValue)
        {
            Update();
        }

        void OnAdd()
        {
            if (_tableDefinitionViewModel.Definition == null)
                return;

            _tableDefinitionViewModel.Definition.ColumnDefinitions.Add(new DbColumnDefinition() { Name = "New Field" ,Type = DbTypesEnum.Integer});
            _tableDefinitionViewModel.TriggerUpdates();
            Update();
        }

        void OnRemoveSelected()
        {
            if (_tableDefinitionViewModel.Definition == null || SelectedItem == null)
                return;

            var selectedViewModel = Rows.Where(X => X.InternalId == SelectedItem.InternalId).FirstOrDefault();
            if (selectedViewModel == null)
                return;

            _tableDefinitionViewModel.Definition.ColumnDefinitions.Remove(selectedViewModel.GetFieldInfo());
            _tableDefinitionViewModel.TriggerUpdates();
            Update();
        }

        void Update()
        {
            foreach(var row in Rows)
                row.PropertyChanged -= Row_PropertyChanged;
            Rows.Clear();
            foreach (var coloumDef in _tableDefinitionViewModel.Definition.ColumnDefinitions)
                Rows.Add(new FieldInfoViewModel(coloumDef));

            foreach (var row in Rows)
                row.PropertyChanged += Row_PropertyChanged;
        }

        private void Row_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _tableDefinitionViewModel.TriggerUpdates();
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
