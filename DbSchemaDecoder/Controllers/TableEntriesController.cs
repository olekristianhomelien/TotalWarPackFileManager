﻿using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.Codecs;
using Filetypes.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace DbSchemaDecoder.Controllers
{
    public class TableEntriesController
    {
        public DbTableViewModel ViewModel { get; set; } = new DbTableViewModel();

        readonly DataGridItemSourceUpdater _dataGridUpdater;

        WindowState _windowState;
        public TableEntriesController(WindowState windowState, DataGridItemSourceUpdater dataGridUpdater)
        {
            _windowState = windowState;
            _dataGridUpdater = dataGridUpdater;
            _windowState.OnSetDbSchema += (sender, schema) => { Update(schema); };
        }

        void Update(IEnumerable<DbColumnDefinition> schema)
        {
            if (_windowState.SelectedFile == null)
                return;

            DataTable table = new DataTable();
            ViewModel.ParseResult = "";
            try
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(_windowState.SelectedFile.DbFile);
                TableEntriesParser parser = new TableEntriesParser(_windowState.SelectedFile.DbFile.Data, header.Length);
                var parseResult = parser.ParseTable(schema.ToList(), (int)header.EntryCount);
                if (parseResult.HasError)
                    ViewModel.ParseResult = parseResult.Error;

                try
                {
                    if (parseResult.ColumnNames != null)
                    {
                        foreach (var columnName in parseResult.ColumnNames)
                            table.Columns.Add(columnName);

                        if (parseResult.DataRows != null)
                        {
                            foreach (var row in parseResult.DataRows)
                                table.Rows.Add(row);
                        }
                    }
                }
                catch
                { 
                            
                }
             
            }
            catch (Exception e)
            {
                ViewModel.ParseResult = $"Error:{e.Message}"; 
            }
            finally
            {
                if (ViewModel.ParseResult == "")
                    ViewModel.ResultColour = new SolidColorBrush(Colors.LightGreen);
                else
                    ViewModel.ResultColour = new SolidColorBrush(Colors.Red);

                ViewModel.EntityTable = table;
                if(_dataGridUpdater != null)
                    _dataGridUpdater.SetData(table);
            }
        }
    }
}
