using CommonDialogs.Common;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbSchemaDecoder.Controllers
{
    public class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public NextItemController NextItemController { get; set; }
        public DbTableDefinitionController DbTableDefinitionController { get; set; }
        public BruteForceController BruteForceController { get; set; }
        public CaSchemaController CaSchemaController { get; set; }
        public TableEntriesController TableEntriesController { get; set; }
        public HeaderInformationController SelectedFileHeaderInformationController { get; set; }

        WindowState _windowState;

        public DbSchemaDecoderController(WindowState eventHub, DataGridItemSourceUpdater dbTableItemSourceUpdater)
        {
            _windowState = eventHub;

            TableEntriesController = new TableEntriesController(_windowState, dbTableItemSourceUpdater);
            NextItemController = new NextItemController(_windowState);
            BruteForceController = new BruteForceController(_windowState);
            CaSchemaController = new CaSchemaController(_windowState);
            DbTableDefinitionController = new DbTableDefinitionController(_windowState);
            SelectedFileHeaderInformationController = new HeaderInformationController(_windowState);
        }

       
    }
}
