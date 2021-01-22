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
        public TableDefinitionModel ActiveTableDefinition = new TableDefinitionModel();

        public MetaDataTable DataTable { get; set; }
        public TableDefinitionEditor TableDefinitionEditor { get; set; }
        public FieldExplorer FieldExplorer { get; set; }

        public MainViewModel(MetaDataFile metaDataFile, List<PackFile> packFiles, bool allTablesReadOnly)
        {
            TableDefinitionEditor = new TableDefinitionEditor(ActiveMentaDataContent, ActiveTableDefinition);
            DataTable = new MetaDataTable(ActiveTableDefinition, ActiveMentaDataContent, packFiles, allTablesReadOnly);
            FieldExplorer = new FieldExplorer(TableDefinitionEditor, ActiveMentaDataContent, ActiveTableDefinition);

            ActiveMentaDataContent.File = metaDataFile;
        }
    }
}
