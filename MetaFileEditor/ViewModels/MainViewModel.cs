using Common;
using CommonDialogs.Common;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using GalaSoft.MvvmLight.Command;
using MetaFileEditor.DataType;
using MetaFileEditor.ViewModels.Data;
using MetaFileEditor.Views.MetadataTableViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MetaFileEditor.ViewModels
{
    class MainViewModel : NotifyPropertyChangedImpl
    {
        public ActiveMetaDataContentModel ActiveMentaDataContent { get; set; } = new ActiveMetaDataContentModel();

        public TagBrowser MetaTagsView { get; set; }



        MetaDataTable _dataTable;
        public MetaDataTable DataTable { get { return _dataTable; } set { SetAndNotify(ref _dataTable, value); } }


        TableDefinitionEditor _tableDefinitionEditor;
        public TableDefinitionEditor TableDefinitionEditor { get { return _tableDefinitionEditor; } set { SetAndNotify(ref _tableDefinitionEditor, value); } }


        TableDefinitionModel _activeTableDefinition = new TableDefinitionModel();
        public TableDefinitionModel ActiveTableDefinition { get { return _activeTableDefinition; } set { SetAndNotify(ref _activeTableDefinition, value); } }


        public FieldExplorer FieldExplorer { get; set; }

        public MainViewModel(MetaDataFile metaDataFile, List<PackFile> packFiles)
        {
            MetaTagsView = new TagBrowser(ActiveMentaDataContent);
            TableDefinitionEditor = new TableDefinitionEditor(ActiveMentaDataContent, ActiveTableDefinition);
            DataTable = new MetaDataTable(ActiveTableDefinition, ActiveMentaDataContent, packFiles);
            FieldExplorer = new FieldExplorer(TableDefinitionEditor, ActiveMentaDataContent, ActiveTableDefinition);

            ActiveMentaDataContent.File = metaDataFile;
        }
    }
}
