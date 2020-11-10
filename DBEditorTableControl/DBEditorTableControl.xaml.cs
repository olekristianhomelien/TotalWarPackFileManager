using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using DBEditorTableControl;
using DBEditorTableControl.Dialogs;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.Codecs;
using static DBEditorTableControl.FilterHelper;

namespace DBTableControl
{
    /// <summary>
    /// Interaction logic for DBEditorTableControl.xaml
    /// </summary>
    public partial class DBEditorTableControl : UserControl, INotifyPropertyChanged, IPackedFileEditor
    {
        DataSet _loadedDataSet;

        // Data Source Properties
        DataTable _currentTable;
        public DataTable CurrentTable
        {
            get { return _currentTable; }
            set
            {
                dbDataGrid.ItemsSource = null;

                _currentTable = value;

                // Reset event handlers
                CurrentTable.ColumnChanged -= new DataColumnChangeEventHandler(CurrentTable_ColumnChanged);
                CurrentTable.ColumnChanged += new DataColumnChangeEventHandler(CurrentTable_ColumnChanged);
                CurrentTable.RowDeleting -= new DataRowChangeEventHandler(CurrentTable_RowDeleting);
                CurrentTable.RowDeleting += new DataRowChangeEventHandler(CurrentTable_RowDeleting);
                CurrentTable.RowDeleted -= new DataRowChangeEventHandler(CurrentTable_RowDeleted);
                CurrentTable.RowDeleted += new DataRowChangeEventHandler(CurrentTable_RowDeleted);
                CurrentTable.TableNewRow -= new DataTableNewRowEventHandler(CurrentTable_TableNewRow);
                CurrentTable.TableNewRow += new DataTableNewRowEventHandler(CurrentTable_TableNewRow);

                // Generate a new table format for the new table.
                GenerateColumns();

                // Re-enable export control if it was disabled
                exportAsButton.IsEnabled = true;

                // Clear and regenerate the observable error list for the new table.
                _errorList.Clear();
                UpdateErrorView();

                // Filters
                var filterController = filterDockPanel.Children[0] as FilterController;
                filterController.LoadFilters(this);

                // Make sure the control knows it's table has changed.
                NotifyPropertyChanged(this, "CurrentTable");

                dbDataGrid.ItemsSource = CurrentTable.DefaultView;
            }
        }

        bool _moveAndFreezeKeys;
        public bool MoveAndFreezeKeys
        {
            get { return _moveAndFreezeKeys; }
            set
            {
                _moveAndFreezeKeys = value;
                NotifyPropertyChanged(this, "MoveAndFreezeKeys");
                UpdateConfig();

                if (EditedFile != null)
                {
                    if (_moveAndFreezeKeys)
                    {
                        FreezeKeys();
                    }
                    else
                    {
                        UnfreezeKeys();
                    }
                }
            }
        }

        bool _useComboBoxes;
        public bool UseComboBoxes
        {
            get { return _useComboBoxes; }
            set 
            { 
                _useComboBoxes = value; 
                NotifyPropertyChanged(this, "UseComboBoxes");
                UpdateConfig();

                if (EditedFile != null)
                {
                    // Generate new columns
                    GenerateColumns(false);
                }
            }
        }

        bool _showAllColumns;
        public bool ShowAllColumns
        {
            get { return _showAllColumns; }
            set
            {
                _showAllColumns = value; 
                NotifyPropertyChanged(this, "ShowAllColumns");
                UpdateConfig();

                // Set all columns to visible, but do not reset currentTable's extended properties.
                foreach (DataGridColumn col in dbDataGrid.Columns)
                {
                    if (_currentTable.Columns[(string)col.Header].ExtendedProperties.ContainsKey("Hidden"))
                    {
                        bool ishidden = (bool)_currentTable.Columns[(string)col.Header].ExtendedProperties["Hidden"];

                        if (ishidden && !_showAllColumns)
                        {
                            col.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            col.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        bool _readOnly;
        public bool ReadOnly
        {
            get { return _readOnly; }
            set 
            { 
                _readOnly = value; 
                NotifyPropertyChanged(this, "TableReadOnly");

                // Set whether we can add rows via button based on readonly.
                addRowButton.IsEnabled = !_readOnly;
                importFromButton.IsEnabled = !_readOnly;
                dbDataGrid.CanUserAddRows = !_readOnly;
                dbDataGrid.CanUserDeleteRows = !_readOnly;

                if (findButton.IsEnabled)
                {
                    replaceButton.IsEnabled = !_readOnly;
                }

                BuiltTablesSetReadOnly(_readOnly);
            }
        }

        bool _showFilters;
        public bool ShowFilters 
        { 
            get { return _showFilters; } 
            set 
            { 
                _showFilters = value;
                NotifyPropertyChanged(this, "ShowFilters");
                UpdateConfig();

                if (_showFilters)
                {
                    filterDockPanel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    filterDockPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
            } 
        }

        // PFM needed Properties
        PackedFile _currentPackedFile;
        public PackedFile CurrentPackedFile
        {
            get { return _currentPackedFile; }
            set
            {
                if (_currentPackedFile != null && DataChanged)
                {
                    Commit();
                }

                if (EditedFile != null)
                {
                    // Save off the editor configuration.
                    UpdateConfig();
                }
                
                DataChanged = false;
                _currentPackedFile = value;

                if (_currentPackedFile != null)
                {
                    try
                    {
                        // Reset and re-register for packedfile events.
                        CurrentPackedFile.RenameEvent -= new PackEntry.Renamed(CurrentPackedFile_RenameEvent);
                        CurrentPackedFile.RenameEvent += new PackEntry.Renamed(CurrentPackedFile_RenameEvent);

                        _codec = PackedFileDbCodec.FromFilename(_currentPackedFile.FullPath);
                        EditedFile = _codec.Decode(_currentPackedFile.Data);
                    }
                    catch (DBFileNotSupportedException exception)
                    {
                        showDBFileNotSupportedMessage(exception.Message);
                    }
                }

                // Create and set CurrentTable
                CurrentTable = CreateTable(EditedFile);

                NotifyPropertyChanged(this, "CurrentPackedFile");
            }
        }

        public PackedFileDbCodec _codec;

        public DBFile EditedFile { get; private set; }
        public bool DataChanged { get; private set; }

        public string _importDirectory;
        public string _exportDirectory;

        // Configuration data
        public DBTableEditorConfig _savedconfig;
        public List<string> _hiddenColumns;
        public List<Visibility> _visibleRows;
        public List<DBFilter> _autofilterList;
		public ObservableCollection<DBError> _errorList;

        //FindAndReplaceWindow _findReplaceWindow;
        FindReplaceHelper _findReplaceHelper;

        public DBEditorTableControl()
        {
            InitializeComponent();

            // Attempt to load configuration settings, loading default values if config file doesn't exist.
            _savedconfig = new DBTableEditorConfig();
            _savedconfig.Load();

            // Instantiate default datatable, and others.
            _currentTable = new DataTable();
            _loadedDataSet = new DataSet("Loaded Tables");
            _loadedDataSet.EnforceConstraints = false;
            _hiddenColumns = new List<string>();
            _visibleRows = new List<System.Windows.Visibility>();
            _autofilterList = new List<DBFilter>();
			_errorList = new ObservableCollection<DBError>();
            dberrorsListView.ItemsSource = _errorList;

            // Transfer saved settings.
            _moveAndFreezeKeys = _savedconfig.FreezeKeyColumns;
            _useComboBoxes = _savedconfig.UseComboBoxes;
            _showAllColumns = _savedconfig.ShowAllColumns;
            _importDirectory = _savedconfig.ImportDirectory;
            _exportDirectory = _savedconfig.ExportDirectory;
            ShowFilters = _savedconfig.ShowFilters;

            // Set Initial checked status.
            moveAndFreezeKeysCheckBox.IsChecked = _moveAndFreezeKeys;
            useComboBoxesCheckBox.IsChecked = _useComboBoxes;
            showAllColumnsCheckBox.IsChecked = _showAllColumns;

            // Register for Datatable events
            CurrentTable.ColumnChanged += new DataColumnChangeEventHandler(CurrentTable_ColumnChanged);
            CurrentTable.RowDeleting += new DataRowChangeEventHandler(CurrentTable_RowDeleting);
            CurrentTable.RowDeleted += new DataRowChangeEventHandler(CurrentTable_RowDeleted);
            CurrentTable.TableNewRow += new DataTableNewRowEventHandler(CurrentTable_TableNewRow);

            _findReplaceHelper = new FindReplaceHelper(this);

            // Default the below buttons to false for all tables.
            cloneRowButton.IsEnabled = false;
            insertRowButton.IsEnabled = false;
            findButton.IsEnabled = false;
            replaceButton.IsEnabled = false; ;

            // Route the Paste event here so we can do it ourselves.
            CommandManager.RegisterClassCommandBinding(typeof(DataGrid), 
                new CommandBinding(ApplicationCommands.Paste, 
                    new ExecutedRoutedEventHandler(OnExecutedPaste), 
                    new CanExecuteRoutedEventHandler(OnCanExecutePaste)));

            
        }

        #region IPackedFileEditor Implementation
        public bool CanEdit(PackedFile file)
        {
            return PackedFileDbCodec.CanDecode(file, out _);
        }

        public void Commit()
        {
            // Ignore Commit call if there is nothing to commit, or if the user simply wandered to another table.
            if (EditedFile == null || (!DataChanged))
            {
                return;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    _codec.Encode(stream, EditedFile);
                    _currentPackedFile.Data = stream.ToArray();
                }

                // Also save off the configuration.
                UpdateConfig();

                DataChanged = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                ErrorDialog.ShowDialog(ex);
#endif
            }
        }
        #endregion

        #region Create Table
        /********************************************************************************************
         * This function uses the schema of currentTable to generate dbDataGrid's columns           *
         * programmatically.  This is necessary to not only create the combo box cells properly     *
         * but to set up more complex bindings than are allowed in the XAML.                        *
         ********************************************************************************************/
        void GenerateColumns(bool clearHidden = true)
        {
            dbDataGrid.Columns.Clear();

            if (clearHidden)
            {
                _hiddenColumns.Clear();

                if (_savedconfig.HiddenColumns.ContainsKey(EditedFile.CurrentType.TableName))
                {
                    _hiddenColumns = new List<string>(_savedconfig.HiddenColumns[EditedFile.CurrentType.TableName]);
                }
            }

            foreach (DataColumn column in CurrentTable.Columns)
            {
                bool isRelated = false;
                List<string> referencevalues = new List<string>();
                Visibility columnvisibility = System.Windows.Visibility.Visible;

                // Set initial column visibility
                if (!column.ExtendedProperties.ContainsKey("Hidden"))
                {
                    column.ExtendedProperties.Add("Hidden", false);
                }

                if (_hiddenColumns.Contains(column.ColumnName))
                {
                    if (!_showAllColumns)
                    {
                        columnvisibility = System.Windows.Visibility.Hidden;
                    }

                    column.ExtendedProperties["Hidden"] = true;
                }

                // Determine relations as assigned by PFM
                if (column.ExtendedProperties.ContainsKey("FKey") && _useComboBoxes && !_readOnly)
                {
                    referencevalues = DBReferenceMap.Instance.ResolveReference(column.ExtendedProperties["FKey"].ToString()).ToList();

                    if (referencevalues.Count() > 0 && ReferenceContainsAllValues(referencevalues, column))
                    {
                        isRelated = true;
                    }
                }

                if (isRelated && !column.ReadOnly && UseComboBoxes)
                {
                    // Combobox Column
                    DataGridComboBoxColumn constructionColumn = new DataGridComboBoxColumn();
                    constructionColumn.Header = column.ColumnName;
                    constructionColumn.IsReadOnly = column.ReadOnly;

                    // Setup the column header's tooltip.
                    string headertooltip = String.Format("Column Data Type: {0}\nReference Table: {1}\nReference Column: {2}",
                                                    EditedFile.CurrentType.ColumnDefinitions[CurrentTable.Columns.IndexOf(column)].Type.ToString(),
                                                    column.ExtendedProperties["FKey"].ToString().Split('.')[0],
                                                    column.ExtendedProperties["FKey"].ToString().Split('.')[1]);

                    // Set the combo boxes items source to the already tested list.
                    constructionColumn.ItemsSource = referencevalues;

                    Binding constructionBinding = new Binding(String.Format("{0}", column.ColumnName));
                    constructionBinding.Mode = BindingMode.TwoWay;
                    constructionBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    constructionColumn.SelectedItemBinding = constructionBinding;

                    // Setup the column context menu
                    // TODO: programatically create context menu so hidden item can be bound to current state.
                    Style tempstyle = new System.Windows.Style(typeof(DataGridColumnHeader), (Style)this.Resources["GridHeaderStyle"]);
                    tempstyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, headertooltip));
                    constructionColumn.HeaderStyle = tempstyle;

                    // Setup the AutoFilter
                    DataTemplate temptemplate = (DataTemplate)this.Resources["AutoFilterGridHeader"];
                    constructionColumn.HeaderTemplate = temptemplate;

                    // Set visibility
                    constructionColumn.Visibility = columnvisibility;

                    dbDataGrid.Columns.Add(constructionColumn);
                }
                else if(EditedFile.CurrentType.ColumnDefinitions.First(n => n.Name.Equals(column.ColumnName)).Type == DbTypesEnum.Boolean)
                {
                    // Checkbox Column
                    DataGridCheckBoxColumn constructionColumn = new DataGridCheckBoxColumn();
                    constructionColumn.Header = column.ColumnName;
                    constructionColumn.IsReadOnly = column.ReadOnly;

                    // Setup the column header's tooltip.
                    string headertooltip = String.Format("Column Data Type: {0}",
                                                    EditedFile.CurrentType.ColumnDefinitions[CurrentTable.Columns.IndexOf(column)].Type.ToString());

                    Binding constructionBinding = new Binding(String.Format("{0}", column.ColumnName));
                    if (!column.ReadOnly)
                    {
                        constructionBinding.Mode = BindingMode.TwoWay;
                    }
                    else
                    {
                        constructionBinding.Mode = BindingMode.OneWay;
                    }

                    constructionBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    constructionColumn.Binding = constructionBinding;

                    // Setup the column context menu
                    // TODO: programatically create context menu so hidden item can be bound to current state.
                    Style tempstyle = new System.Windows.Style(typeof(DataGridColumnHeader), (Style)this.Resources["GridHeaderStyle"]);
                    tempstyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, headertooltip));
                    constructionColumn.HeaderStyle = tempstyle;

                    // Setup the AutoFilter
                    DataTemplate temptemplate = (DataTemplate)this.Resources["AutoFilterGridHeader"];
                    constructionColumn.HeaderTemplate = temptemplate;

                    // Set visibility
                    constructionColumn.Visibility = columnvisibility;

                    dbDataGrid.Columns.Add(constructionColumn);
                }
                else
                {
                    // Textbox Column
                    DataGridTextColumn constructionColumn = new DataGridTextColumn();
                    constructionColumn.Header = column.ColumnName;
                    constructionColumn.IsReadOnly = column.ReadOnly;

                    // Setup the column header's tooltip.
                    string headertooltip = String.Format("Column Data Type: {0}",
                                                    EditedFile.CurrentType.ColumnDefinitions[CurrentTable.Columns.IndexOf(column)].Type.ToString());

                    Binding constructionBinding = new Binding(String.Format("{0}", column.ColumnName));
                    if (!column.ReadOnly)
                    {
                        constructionBinding.Mode = BindingMode.TwoWay;
                    }
                    else
                    {
                        constructionBinding.Mode = BindingMode.OneWay;
                    }

                    constructionColumn.Binding = constructionBinding;

                    // Setup the column context menu
                    // TODO: programatically create context menu so hidden item can be bound to current state.
                    Style tempstyle = new System.Windows.Style(typeof(DataGridColumnHeader), (Style)this.Resources["GridHeaderStyle"]);
                    tempstyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, headertooltip));
                    constructionColumn.HeaderStyle = tempstyle;

                    // Setup the AutoFilter
                    DataTemplate temptemplate = (DataTemplate)this.Resources["AutoFilterGridHeader"];
                    constructionColumn.HeaderTemplate = temptemplate;

                    // Set visibility
                    constructionColumn.Visibility = columnvisibility;

                    dbDataGrid.Columns.Add(constructionColumn);
                }
            }

            // Finally, based on whether we are moving and freezing columns, rearrange their order.
            if (EditedFile != null)
            {
                if (_moveAndFreezeKeys)
                {
                    FreezeKeys();
                }
            }
        }

        /********************************************************************************************
         * This function constructs the System.Data.DataTable we use to not only store our data,    *
         * but to bind as our visuals data source.                                                  *
         ********************************************************************************************/
        private DataTable CreateTable(DBFile table)
        {
            DataTable constructionTable = new DataTable(_currentPackedFile.Name);

            // Clear the dataset and create the data table.
            _loadedDataSet.Tables.Clear();
            _loadedDataSet.Tables.Add(constructionTable);

            DataColumn constructionColumn;
            List<DataColumn> keyList = new List<DataColumn>();
            constructionTable.BeginLoadData();

            foreach (var columnInfo in table.CurrentType.ColumnDefinitions)
            {
                // Create the new column, using object as the data type for all columns, this way we avoid the WPF DataGrid's built in
                // data validation abilities in favor of our own implementation.
                constructionColumn = new DataColumn(columnInfo.Name, typeof(string));

                if (columnInfo.Type == DbTypesEnum.Short || columnInfo.Type == DbTypesEnum.Integer || columnInfo.Type == DbTypesEnum.Single)
                {
                    constructionColumn = new DataColumn(columnInfo.Name, typeof(double));
                }

                constructionColumn.AllowDBNull = true;
                constructionColumn.Unique = false;
                constructionColumn.ReadOnly = _readOnly;

                // Save the FKey if it exists
                if (!String.IsNullOrEmpty(columnInfo.TableReference))
                {
                    constructionColumn.ExtendedProperties.Add("FKey", columnInfo.TableReference);
                }

                // If the column is a primary key, save it for later adding
                if (columnInfo.IsKey)
                {
                    keyList.Add(constructionColumn);
                }

                constructionTable.Columns.Add(constructionColumn);
            }

            // If the table has primary keys, set them.
            if (keyList.Count > 0)
            {
                constructionTable.PrimaryKey = keyList.ToArray();
            }

            // Now that the DataTable schema is constructed, add in all the data.
            foreach (List<DbField> rowentry in table.Entries)
            {
                constructionTable.Rows.Add(rowentry.Select(n => n.Value).ToArray<object>());
            }

            constructionTable.EndLoadData();
            constructionTable.AcceptChanges();

            // Finally, generate the visibleRows list based on total number of rows in the new table.
            _visibleRows.Clear();
            for (int i = 0; i < constructionTable.Rows.Count; i++)
            {
                _visibleRows.Add(System.Windows.Visibility.Visible);
            }

            return constructionTable;
        }

        public void Import(DBFile importfile)
        {
            // If we are here, then importfile has already been imported into editfile, so no need to do any type checking.
            // The old DBE would check for matching keys and overwrite any it found, data validation means this is no longer
            // necessary to maintain GUI integrity.
            
            // Unbind the GUI datasource, and tell currentTable to get ready for new data.
            dbDataGrid.ItemsSource = null;
            _currentTable.BeginLoadData();

			MessageBoxResult question = MessageBox.Show("Replace the current data?", "Replace data?", 
                                                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (question == MessageBoxResult.Cancel) {
                return;
            } else if (question == MessageBoxResult.Yes) {
                EditedFile.Entries.Clear();
                _currentTable.Clear();
            }

            // Since Data.Rows lacks an AddRange method, enumerate through the entries manually.
            foreach (var entry in importfile.Entries)
            {
                DataRow row = _currentTable.NewRow();
                row.ItemArray = entry.Select(n => n.Value).ToArray();
                CurrentTable.Rows.Add(row);
            }

            _currentTable.EndLoadData();
            dbDataGrid.ItemsSource = CurrentTable.DefaultView;
        }

        private bool ReferenceContainsAllValues(List<string> referencevalues, DataColumn column)
        {
            foreach (object item in column.GetItemArray())
            {
                if (!referencevalues.Contains(item.ToString()))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region PackedFile Events

        // When a packed file is renamed, update the cached table name.
        void CurrentPackedFile_RenameEvent(PackEntry dir, string newName)
        {
            _currentTable.TableName = newName;
        }

        #endregion

        #region UserControl Events

        private void dbeControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DebugHelper.PreviewKeyDown(this, e);
        }

        private void TableControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UpdateConfig();
        }

        #endregion

        #region Toolbar Events
        private void AddRowButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Add a new row with default values.
            DataRow row = _currentTable.NewRow();
            List<object> items = new List<object>();
            
            for (int i = 0; i < EditedFile.CurrentType.ColumnDefinitions.Count; i++)
            {
                items.Add(GetDefaultValue(i));
            }

            row.ItemArray = items.ToArray();
            _currentTable.Rows.Add(row);
            CheckRowForErrors(_currentTable.Rows.Count - 1);

            DataChanged = true;
            SendDataChanged();
        }

        private void CloneRowButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Only do anything if atleast 1 row is selected.
            if (dbDataGrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView rowview in dbDataGrid.SelectedItems.OfType<DataRowView>())
                {
                    DataRow row = _currentTable.NewRow();
                    List<object> items = new List<object>();
                    items.AddRange(rowview.Row.ItemArray);

                    // If the current row has an error logged, modify that column to default value before attempting to put it in editedFile.
                    if (rowview.Row.HasErrors && rowview.Row.GetColumnsInError().Count(n => rowview.Row.GetColumnError(n).Contains("Error:")) > 0)
                    {
                        foreach (int colindex in rowview.Row.GetColumnsInError().Where(n => rowview.Row.GetColumnError(n).Contains("Error:"))
                                                                                .Select(n => n.Ordinal))
                        {
                            items[colindex] = GetDefaultValue(colindex);
                        }
                    }

                    row.ItemArray = items.ToArray();
                    _currentTable.Rows.Add(row);
                    CheckRowForErrors(_currentTable.Rows.Count - 1);
                }
            }

            DataChanged = true;
            SendDataChanged();
        }

        private void insertRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (dbDataGrid.SelectedItems.OfType<DataRowView>().Count() > 0)
            {
                int datarowindex = -1;
                int visualrowindex = -1;

                // First, find the lowest visual row index.
                foreach (DataRowView rowview in dbDataGrid.SelectedItems.OfType<DataRowView>())
                {
                    if (visualrowindex == -1)
                    {
                        visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                        datarowindex = _currentTable.Rows.IndexOf(rowview.Row);
                        continue;
                    }

                    if (visualrowindex > dbDataGrid.Items.IndexOf(rowview))
                    {
                        visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                        datarowindex = _currentTable.Rows.IndexOf(rowview.Row);
                    }
                }

                // Now that we have the lowest selected row index, and it's corresponding location in our data source, we can insert.
                InsertRow(datarowindex);
            }
            else if (dbDataGrid.SelectedItems.Count == 1 && !(dbDataGrid.SelectedItems[0] is DataRowView))
            {
                // We should only hit this code if the user is attempting to insert rows with only the blank row selected
                // in this case we want to simply add the rows on to the end of the table.
                InsertRow();
            }
        }

        private void findButton_Click(object sender, RoutedEventArgs e)
        {
            _findReplaceHelper.ShowSearch();
        }

        private void replaceButton_Click(object sender, RoutedEventArgs e)
        {
            _findReplaceHelper.ShowReplace();
        }

        private void ExportAsButton_Clicked(object sender, RoutedEventArgs e)
        {
            ExportContextMenu.IsOpen = true;
        }

        private void ImportFromButton_Clicked(object sender, RoutedEventArgs e)
        {
            ImportContextMenu.IsOpen = true;
        }

        private void ExportTSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ExportTSVMenu(CurrentPackedFile, _exportDirectory);
        }

        private void ExportBinaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ExportBinary(CurrentPackedFile, _exportDirectory);
        }

        private void ExportCAXmlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ExportCAXml(CurrentPackedFile, _exportDirectory);
        }

        private void ImportTSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ImportTSV(this);
        }

        private void ImportCSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ImportCSV(this);
        }

        private void ImportBinaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ImportBinary(this);
        }

        private void ImportCAXmlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportExportHelper.ImportCAXml(this);
        }
        #endregion

        #region DataGrid Events

        private void dbDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Resetting the cell's Style forces a UI ReDraw to occur without disturbing the rest of the datagrid.
            DataGridCell cell = e.EditingElement.Parent as DataGridCell;

            // Used to circumvent the DataTable limitation of only updating cells after the row finishes an edit operation
            // causing the visual grid to only commit changes the editedFile and table state once the user navigates to another row.
            if (e.Row.Item is DataRowView)
            {
                int colindex = _currentTable.Columns.IndexOf((string)e.Column.Header);
                int rowindex = _currentTable.Rows.IndexOf((e.Row.Item as DataRowView).Row);
                string proposedvalue = "";

                if (cell.Content is TextBox)
                {
                    proposedvalue = (cell.Content as TextBox).Text;
                }
                else if (cell.Content is ComboBox)
                {
                    proposedvalue = (cell.Content as ComboBox).SelectedValue.ToString();
                }
                else if (cell.Content is CheckBox)
                {
                    proposedvalue = (cell.Content as CheckBox).IsChecked.Value.ToString();
                }

                // If the user is editing the blank row to create a new one, do not attempt to edit the value directly since the
                // blank row has not been added to the currentTable's row collection yet.
                if ((e.Row.Item as DataRowView).Row.RowState != DataRowState.Detached)
                {
                    if (ValueIsValid(proposedvalue, colindex))
                    {
                        _currentTable.Rows[rowindex].BeginEdit();
                        _currentTable.Rows[rowindex][colindex] = proposedvalue;
                        _currentTable.Rows[rowindex].EndEdit();
                    }
                    else
                    {
                        // If the proposed value for this cell is invalid we should add an error here manually for the user since it will not
                        // always generate one if the DataGrid's data validation catches it.
                        AddError(rowindex, colindex, String.Format("'{0}' is not a valid value for '{1}'", proposedvalue,
                                                                                                        _currentTable.Columns[colindex].ColumnName));
                    }
                }
            }

            Style TempStyle = cell.Style;
            cell.Style = null;
            cell.Style = TempStyle;
        }

        private void dbDataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Used to set up the 2-click edit for ComboBox and CheckBox cells.
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly && !CurrentTable.HasErrors)
            {
                if (cell.Content is CheckBox || cell.Content is ComboBox)
                {
                    cell.Focus();
                    cell.IsEditing = true;
                }
            }

            // Set the find and replace button IsEnabled, once user clicks on grid.
            if (!findButton.IsEnabled)
            {
                findButton.IsEnabled = true;
                replaceButton.IsEnabled = !_readOnly;
            }
        }

        private void dbDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                // User clicked 'outside' the datagrid, deselect everthing.
                dbDataGrid.UnselectAll();
                dbDataGrid.UnselectAllCells();
            }
        }

        private void dbDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable/Disable Clone Row button based on current selection.
            if (!_readOnly)
            {
                cloneRowButton.IsEnabled = dbDataGrid.SelectedItems.Count > 0;
                insertRowButton.IsEnabled = dbDataGrid.SelectedItems.Count > 0;
            }
        }

        private void dbDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var typetest = e.Row.Item;

            if (!(e.Row.Item is DataRowView))
            {
                e.Row.Header = "*";
            }
            else if((e.Row.Item as DataRowView).Row.RowState != DataRowState.Detached)
            {
                int datarowindex = CurrentTable.Rows.IndexOf((e.Row.Item as DataRowView).Row);
                e.Row.Header = datarowindex + 1;

                // Additional error checking on the visibleRows internal list.
                if (datarowindex >= _visibleRows.Count)
                {
                    UpdateVisibleRows();
                }
                e.Row.Visibility = _visibleRows[datarowindex];
            }// The user just tried to modify the blank row, adding a new detached row to our collection, act accordingly.
            else if ((e.Row.Item as DataRowView).Row.RowState == DataRowState.Detached)
            {
                int datarowindex = _currentTable.Rows.Count;
                e.Row.Header = datarowindex + 1;

                // Additional error checking on the visibleRows internal list.
                if (datarowindex >= _visibleRows.Count)
                {
                    UpdateVisibleRows();
                }
                e.Row.Visibility = _visibleRows[datarowindex];
            }
        }

        private void dbDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _findReplaceHelper.dbDataGrid_PreviewKeyDown(sender, e);

            // Look for Insert key press, and check if a row is selected.
            if (!_readOnly && e.Key == Key.Insert && dbDataGrid.SelectedItems.Count > 0)
            {
                DataRow newrow = _currentTable.NewRow();

                if (dbDataGrid.SelectedItems.OfType<DataRowView>().Count() > 0)
                {
                    int datarowindex = -1;
                    int visualrowindex = -1;

                    // First, find the lowest visual row index.
                    foreach (DataRowView rowview in dbDataGrid.SelectedItems.OfType<DataRowView>())
                    {
                        if (visualrowindex == -1)
                        {
                            visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                            datarowindex = _currentTable.Rows.IndexOf(rowview.Row);
                            continue;
                        }

                        if (visualrowindex > dbDataGrid.Items.IndexOf(rowview))
                        {
                            visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                            datarowindex = _currentTable.Rows.IndexOf(rowview.Row);
                        }
                    }

                    InsertRow(datarowindex);
                }
                else if (dbDataGrid.SelectedItems.Count == 1 && !(dbDataGrid.SelectedItems[0] is DataRowView))
                {
                    // We should only hit this code if the user is attempting to insert rows with only the blank row selected
                    // in this case we want to simply add the rows on to the end of the table.
                    try
                    {
                        InsertRow();
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        ErrorDialog.ShowDialog(ex);
#endif
                    }
                }
            }

            // F6 and F7 will be the shortcut to check for errors.
            if (!_readOnly && (e.Key == Key.F6 || e.Key == Key.F7))
            {
                _errorList.Clear();
                _currentTable.ClearErrors();
                CheckTableForErrors();
            }
        }

        private void dbDataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            if (e.Item is DataRowView)
            {
                // Clear copy data if row is collapsed from filtering.
                int datarowindex = _currentTable.Rows.IndexOf((e.Item as DataRowView).Row);
                if (datarowindex >= _visibleRows.Count)
                {
                    UpdateVisibleRows();
                }

                if (_visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                {
                    e.ClipboardRowContent.Clear();
                }
            }
        }

        private void dbDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
        }

        private void dbDataGridColumnFilterComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox autofilter = (sender as ComboBox);
            string colname = (string)autofilter.DataContext;

            // Create the values we will use for the filter, along with a blank (disable) and an Any and No value filter.
            List<string> filtervalues = new List<string>();
            filtervalues.Add("");

            // If we are dealing with an optional column, add the options to filter by cells that either have any, or no value.
            if (EditedFile.CurrentType.ColumnDefinitions[_currentTable.Columns.IndexOf(colname)].Type == DbTypesEnum.Optstring ||
                EditedFile.CurrentType.ColumnDefinitions[_currentTable.Columns.IndexOf(colname)].Type == DbTypesEnum.Optstring_ascii)
            {
                filtervalues.Add("Any Value");
                filtervalues.Add("No Value");
            }

            // Now that we have the column name, determine if we need to get the possible values from the table, or it's reference.
            if (dbDataGrid.Columns.Single(n => colname.Equals((string)n.Header)) is DataGridComboBoxColumn)
            {
                // We have a combo box column, use all possible values.
                filtervalues.AddRange((dbDataGrid.Columns.Single(n => colname.Equals((string)n.Header)) as DataGridComboBoxColumn).ItemsSource.Cast<string>().Distinct());
            }
            else
            {
                // We have a regular column, get the existing values for the list.
                List<object> possiblevalues = _currentTable.Columns[colname].GetItemArray(false).Distinct().ToList();
                filtervalues.AddRange(possiblevalues.Select(n => n.ToString()));
            }

            autofilter.ItemsSource = filtervalues;
        }

        private void dbDataGridColumnFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Grab the information we'll need.
            ComboBox autofilter = (sender as ComboBox);
            string colname = (string)autofilter.DataContext;
            string filtervalue = (string)autofilter.SelectedValue;

            // Remove any previously set autofilters.
            if (_autofilterList.Count(n => n.ApplyToColumn.Equals(colname)) > 0)
            {
                _autofilterList.RemoveAll(n => n.ApplyToColumn.Equals(colname));
            }

            // If the user selects the blank item, simply remove the existing filter.
            if (!String.IsNullOrEmpty(filtervalue))
            {
                // Construct a new filter and add it to the autofilterList.
                DBFilter newfilter = new DBFilter();
                newfilter.ApplyToColumn = colname;
                newfilter.IsActive = true;
                newfilter.Name = String.Format("Auto_{0}_{1}", colname, filtervalue);
                newfilter.FilterValue = filtervalue;
                newfilter.MatchMode = MatchType.Exact;

                if (filtervalue.Equals("Any Value"))
                {
                    newfilter.MatchMode = MatchType.NotEmpty;
                }
                else if (filtervalue.Equals("No Value"))
                {
                    newfilter.MatchMode = MatchType.Empty;
                }
                else
                {
                    
                }

                _autofilterList.Add(newfilter);
            }

            UpdateVisibleRows();
            dbDataGrid.Items.Refresh();
        }

        #endregion

        #region Paste Code
        protected virtual void OnExecutedPaste(object sender, ExecutedRoutedEventArgs args)
        {
            ClipboardHelper.OnExecutedPaste(this);
        }

        protected virtual void OnCanExecutePaste(object sender, CanExecuteRoutedEventArgs args)
        {
            ClipboardHelper.OnCanExecutePaste(this, args);
        }

        #endregion

        #region Context Menu Events

        private void ColumnHeaderContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.Open(sender, this);
        }

        private void SelectColumnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.SelectColumn(sender, this);
        }

        private void RemoveSortingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.RemoveSorting(sender, this);
        }

        private void ColumnApplyExpressionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.ColumnApplyExpression(sender, this);
        }

        private void ColumnMassEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.ColumnMassEdit(sender, this);
        }

        private void RenumberMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.RenumberMenuItem(sender, this);
        }

        private void EditVisibleListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.EditVisibleListMenuItem(sender, this);
        }

        private void ClearTableHiddenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.ClearTableHiddenMenuItem(this);
        }

        private void ClearAllHiddenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.ClearAllHiddenMenuItem(this);
        }

        private void RowHeaderContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.RowHeaderContextMenu(sender);
        }

        private void RowHeaderInsertRow_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.RowHeaderInsertRow(sender, this);
        }

        private void RowHeaderInsertManyRows_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.RowHeaderInsertManyRows(sender, this);
        }

        void DataGridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.DataGridContextMenu(sender, this);
        }

        private void DataGridCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.DataGridCopyMenuItem();
        }

        private void DataGridPasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.DataGridPasteMenuItem();
        }

        private void DataGridApplyExpressionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.DataGridApplyExpressionMenuItem(sender, this);
        }

        private void DataGridMassEditStringsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.DataGridMassEditStringsMenuItem(sender, this);
        }

        private void DataGridRevertCellMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuHelper.DataGridRevertCellMenuItem(sender, this);
        }

        #endregion

        #region Datatable Events

        void CurrentTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            // Add the new row to editedfile.
            var dbfileconstructionRow = new List<DbField>();
            for (int i = 0; i < e.Row.ItemArray.Length; i++)
            {
                dbfileconstructionRow.Add(new DbField(EditedFile.CurrentType.ColumnDefinitions[i].Type));
            }

            // Modify the new row to have default data initially.
            List<object> vals = new List<object>();
            for (int i = 0; i < EditedFile.CurrentType.ColumnDefinitions.Count; i++)
            {
                vals.Add(GetDefaultValue(i));
            }

            // Since we have no idea where a new row should go yet, append it and rely on others to move it.
            EditedFile.Entries.Add(new DBRow(EditedFile.CurrentType, dbfileconstructionRow));
            _visibleRows.Add(System.Windows.Visibility.Visible);

            // Do not set the new itemarray until editedFile has been updated.
            e.Row.ItemArray = vals.ToArray();

            DataChanged = true;
            SendDataChanged();
        }

        private int deletingindex = -1;
        void CurrentTable_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            deletingindex = _currentTable.Rows.IndexOf(e.Row);

            EditedFile.Entries.RemoveAt(deletingindex);

            // Additional error checking for the visibleRows internal list.
            if (deletingindex >= _visibleRows.Count)
            {
                UpdateVisibleRows();
            }

            _visibleRows.RemoveAt(deletingindex);

            DataChanged = true;
            SendDataChanged();
        }

        void CurrentTable_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            if (e.Row.RowState != DataRowState.Detached)
            {
                int removalindex = _currentTable.Rows.IndexOf(e.Row);
                // Remove the row, because otherwise there will be indexing issues due to how the DataTable class handles row deletion.
                _currentTable.Rows.Remove(e.Row);
            }

            // Grab the deleting error's error message if it is a key warning.
            string errormessage = "";
            if (_errorList.Count(n => n.ErrorMessage.Contains("key sequence") && n.RowIndex == deletingindex) > 0)
            {
                errormessage = _errorList.First(n => n.ErrorMessage.Contains("key sequence") && n.RowIndex == deletingindex).ErrorMessage;
            }

            // Next remove any errors generated by the deleting row in our observable collection.
            List<int> errorindicies = new List<int>();
            errorindicies.AddRange(_errorList.Where(n => n.RowIndex == deletingindex).Select(n => _errorList.IndexOf(n)).Distinct());
            errorindicies.Sort();
            errorindicies.Reverse();
            errorindicies.ForEach(n => _errorList.RemoveAt(n));

            // If we have a key warning in the row, update it and test the original key for uniqueness, but only if it hasn't already been deleted.
            if (!String.IsNullOrEmpty(errormessage) && _errorList.Count(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != deletingindex) > 0)
            {
                // We then need to call CheckCellForWarnings again to test if the original (first appearance of) key is now unique.
                int firstrowindex = _errorList.Where(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != deletingindex).Select(n => n.RowIndex).Min();
                if (firstrowindex < deletingindex)
                {
                    // Only check for warnings if the deleting row isn't the first occurence of the key.
                    int firstcolindex = _currentTable.Columns.IndexOf(_currentTable.PrimaryKey[0]);
                    CheckCellForWarnings(firstrowindex, firstcolindex);
                }
            }

            // Update every error whose row index just changed.
            foreach (DBError err in _errorList.Where(n => n.RowIndex > deletingindex))
            {
                err.RowIndex = err.RowIndex - 1;
            }

            deletingindex = -1;
            DataChanged = true;
            SendDataChanged();
            RefreshRowHeaders();
        }

        void CurrentTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            // Set row index as either the last row in edited file if we are creating a new row.
            int rowIndex = e.Row.RowState == DataRowState.Detached ? EditedFile.Entries.Count - 1 : e.Row.Table.Rows.IndexOf(e.Row);
            int colIndex = e.Column.Ordinal;

            // If the row is detached, then is recently added or cloned and not part of currentTable, so if it contains an error use the default value.
            if (e.Row.RowState == DataRowState.Detached)
            {
                if (ValueIsValid(e.ProposedValue, colIndex))
                {
                    EditedFile.Entries[rowIndex][colIndex].Value = e.ProposedValue.ToString();
                }
                else
                {
                    EditedFile.Entries[rowIndex][colIndex].Value = GetDefaultValue(colIndex).ToString();
                    List<object> templist = new List<object>();
                    templist.AddRange(e.Row.ItemArray);
                    templist[colIndex] = GetDefaultValue(colIndex);
                    e.Row.ItemArray = templist.ToArray();
                }
            }
            else
            {
                // Only modify editedFile if the cell does not have an error.
                if (!CheckCellForErrors(rowIndex, colIndex))
                {
                    EditedFile.Entries[rowIndex][colIndex].Value = e.ProposedValue.ToString();
                }
            }

            DataChanged = true;
            SendDataChanged();
            RefreshCell(rowIndex, colIndex);
        }



        #endregion

        #region INotifyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private void SendDataChanged()
        {
            // This method is used to trip the packedFile's data changed notification, so that the PFM tree list updates
            // when data is changed, instead of once a user navigates away.
            if (DataChanged)
            {
                _currentPackedFile.Modified = true;
            }
        }
        #endregion

        #region UI Helper Methods

        public DataGridCell GetCell(int row, int column, bool onlyvisible = false)
        {
            DataGridRow rowContainer = GetRow(row, onlyvisible);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // UI Virtualization may interfere with the presenter, so scroll to the item and try again.
                if (presenter == null)
                {
                    // If vitrualized and not in view, ignore based on optional paramater.
                    if (onlyvisible)
                    {
                        return null;
                    }

                    dbDataGrid.ScrollIntoView(rowContainer, dbDataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                // try to get the cell but it may possibly be virtualized
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // If virtualized possibly ignore
                    if (onlyvisible)
                    {
                        return null;
                    }

                    // now try to bring into view and retreive the cell
                    dbDataGrid.ScrollIntoView(rowContainer, dbDataGrid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public DataGridRow GetRow(int index, bool onlyvisible = false)
        {
            DataGridRow row = (DataGridRow)dbDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null && !onlyvisible)
            {
                // may be virtualized, bring into view and try again
                dbDataGrid.ScrollIntoView(dbDataGrid.Items[index]);
                row = (DataGridRow)dbDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            var test = LogicalTreeHelper.GetChildren(parent);
            LogicalTreeHelper.BringIntoView(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }

                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        static List<T> GetVisualChildren<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            List<T> returnlist = new List<T>();
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            var jtest = LogicalTreeHelper.GetChildren(parent);
            LogicalTreeHelper.BringIntoView(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    var test = GetVisualChildren<T>(v);
                    if (test == null)
                    {
                        continue;
                    }
                    returnlist.AddRange(GetVisualChildren<T>(v));
                }

                if (child != null)
                {
                    returnlist.Add(child as T);
                }
            }

            return returnlist;
        }

        public IEnumerable<DependencyObject> FindInVisualTreeDown(DependencyObject obj, string type)
        {
            if (obj != null)
            {
                if (obj.GetType().ToString().EndsWith(type))
                {
                    yield return obj;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    foreach (var child in FindInVisualTreeDown(VisualTreeHelper.GetChild(obj, i), type))
                    {
                        if (child != null)
                        {
                            yield return child;
                        }
                    }
                }
            }
            yield break;
        }

        public void Refresh(bool onlyvisible = false)
        {
            if (!onlyvisible)
            {
                // TODO: Try to find another way to force refresh the screen.
                var selectedItems = dbDataGrid.SelectedItems;
                List<DataGridCellInfo> selectedCells = dbDataGrid.SelectedCells.ToList();
                List<int> selectedItemsIndicies = dbDataGrid.SelectedItems.OfType<DataRowView>().Select(n => dbDataGrid.Items.IndexOf(n)).ToList();

                dbDataGrid.ItemsSource = null;
                dbDataGrid.ItemsSource = CurrentTable.DefaultView;

                foreach (int index in selectedItemsIndicies)
                {
                    dbDataGrid.SelectedItems.Add(dbDataGrid.Items[index]);
                }

                foreach (DataGridCellInfo cellinfo in selectedCells)
                {
                    DataGridCellInfo cellToAdd = new DataGridCellInfo(cellinfo.Item, cellinfo.Column);
                    if (!dbDataGrid.SelectedCells.Contains(cellToAdd))
                    {
                        dbDataGrid.SelectedCells.Add(cellToAdd);
                    }
                }
            }
            else // Refresh only visible elements, column by column.
            {
                for (int i = 0; i < _currentTable.Columns.Count; i++)
                {
                    RefreshColumn(i);
                }
            }
        }

        public void RefreshColumn(int column)
        {
            for (int i = 0; i < dbDataGrid.Items.Count; i++)
            {
                DataGridCell cell = GetCell(i, column, true);

                if (cell != null)
                {
                    // Resetting the cell's Style forces a UI ReDraw to occur without disturbing the rest of the datagrid.
                    Style TempStyle = cell.Style;
                    cell.Style = null;
                    cell.Style = TempStyle;
                }
            }
        }

        public void RefreshCell(int row, int column)
        {
            DataGridCell cell = GetCell(row, column, true);

            if (cell != null)
            {
                // Resetting the cell's Style forces a UI ReDraw to occur without disturbing the rest of the datagrid.
                Style TempStyle = cell.Style;
                cell.Style = null;
                cell.Style = TempStyle;
            }
        }

        public void SelectCell(int rowindex, int colindex, bool scrollview = false)
        {
            // Add the cell to the selected cells list.
            DataGridCellInfo cellinfo = new DataGridCellInfo(dbDataGrid.Items[rowindex], dbDataGrid.Columns[colindex]);
            if (!dbDataGrid.SelectedCells.Contains(cellinfo))
            {
                dbDataGrid.SelectedCells.Add(cellinfo);
            }

            // Scroll cell into view if asked.
            if (scrollview)
            {
                dbDataGrid.ScrollIntoView(dbDataGrid.Items[rowindex], dbDataGrid.Columns[colindex]);
            }
        }

        private void RefreshRowHeaders()
        {
            for (int i = 0; i < dbDataGrid.Items.Count; i++)
            {
                DataGridRow row = GetRow(i, true);

                if (row == null)
                {
                    continue;
                }

                if (dbDataGrid.Items[i] is DataRowView)
                {
                    row.Header = _currentTable.Rows.IndexOf((dbDataGrid.Items[i] as DataRowView).Row) + 1;
                }
                else
                {
                    row.Header = "*";
                }
            }
        }

        #endregion

        #region Utility Functions

        private List<string> GetPrimaryKeySequence(int rowindex)
        {
            List<string> pksequence = new List<string>();

            for (int i = 0; i < _currentTable.Columns.Count; i++)
            {
                if (EditedFile.CurrentType.ColumnDefinitions[i].IsKey)
                {
                    pksequence.Add(_currentTable.Rows[rowindex][i].ToString());
                }
            }

            return pksequence;
        }

        private bool TableContainsKeySequence(List<string> pksequence)
        {
            List<List<string>> pklist = new List<List<string>>();

            // Construct a list of pk sequences in the table.
            for (int i = 0; i < _currentTable.Rows.Count; i++)
            {
                pklist.Add(GetPrimaryKeySequence(i));
            }

            // Test to see if there are more than 1 occurence of a key sequence.
            if (pklist.Count(n => n.SequenceEqual(pksequence)) > 1)
            {
                return true;
            }

            return false;
        }

        private IEnumerable<int> GetMatchingKeyIndicies(List<string> pksequence)
        {
            for (int i = 0; i < _currentTable.Rows.Count; i++)
            {
                if (GetPrimaryKeySequence(i).SequenceEqual(pksequence))
                {
                    yield return i;
                }
            }
        }

        public void UpdateConfig()
        {
            if (EditedFile == null)
            {
                return;
            }

            // Save off any required information before changing anything.
            _savedconfig.FreezeKeyColumns = _moveAndFreezeKeys;
            _savedconfig.UseComboBoxes = _useComboBoxes;
            _savedconfig.ShowAllColumns = _showAllColumns;
            _savedconfig.ImportDirectory = _importDirectory;
            _savedconfig.ExportDirectory = _exportDirectory;
            _savedconfig.ShowFilters = _showFilters;

            if (_savedconfig.HiddenColumns.ContainsKey(EditedFile.CurrentType.TableName))
            {
                // Overwrite the old hidden column list for this table.
                _savedconfig.HiddenColumns[EditedFile.CurrentType.TableName].Clear();
                _savedconfig.HiddenColumns[EditedFile.CurrentType.TableName].AddRange(_hiddenColumns);
            }
            else
            {
                // Create a new list for the table.
                _savedconfig.HiddenColumns.Add(new KeyValuePair<string, List<string>>(EditedFile.CurrentType.TableName, new List<string>(_hiddenColumns)));
            }
          

            _savedconfig.Save();
        }

        public void showDBFileNotSupportedMessage(string message)
        {
            throw new NotImplementedException("TODO");
            
            /*// Set the warning box as visible.
            dbDataGrid.Visibility = System.Windows.Visibility.Hidden;
            unsupportedDBErrorTextBox.Visibility = System.Windows.Visibility.Visible;

            // Set the message
            unsupportedDBErrorTextBox.Text = string.Format("{0}{1}", message, string.Join("\r\n", DBTypeMap.Instance.DBFileTypes));

            // Modify controls accordingly
            // Most controls useability are bound by TableReadOnly, so set it.
            _readOnly = true;
            // Modify the remaining controls manually.
            exportAsButton.IsEnabled = false;*/
        }

        static public Type GetTypeFromCode(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                    return typeof(bool);

                case TypeCode.Byte:
                    return typeof(byte);

                case TypeCode.Char:
                    return typeof(char);

                case TypeCode.DateTime:
                    return typeof(DateTime);

                case TypeCode.DBNull:
                    return typeof(DBNull);

                case TypeCode.Decimal:
                    return typeof(decimal);

                case TypeCode.Double:
                    return typeof(double);

                case TypeCode.Empty:
                    return typeof(string);

                case TypeCode.Int16:
                    return typeof(short);

                case TypeCode.Int32:
                    return typeof(int);

                case TypeCode.Int64:
                    return typeof(long);

                case TypeCode.Object:
                    return typeof(object);

                case TypeCode.SByte:
                    return typeof(sbyte);

                case TypeCode.Single:
                    return typeof(Single);

                case TypeCode.String:
                    return typeof(string);

                case TypeCode.UInt16:
                    return typeof(UInt16);

                case TypeCode.UInt32:
                    return typeof(UInt32);

                case TypeCode.UInt64:
                    return typeof(UInt64);
            }

            return null;
        }

        private void BuiltTablesSetReadOnly(bool tablesreadonly)
        {
            foreach (DataTable table in _loadedDataSet.Tables)
            {
                foreach (DataColumn column in table.Columns)
                {
                    column.ReadOnly = tablesreadonly;
                }
            }
        }



        private object GetDefaultValue(int colindex)
        {
            throw new NotImplementedException("TODO");
            /*
            if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.String)
            {
                if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type.Contains("optstring"))
                {
                    return "";
                }// We have a combo box column, so default any non optional string to the first value in the list.
                else if (dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
                {
                    List<string> templist = (List<string>)(dbDataGrid.Columns[colindex] as DataGridComboBoxColumn).ItemsSource;
                    return templist[0];
                }
                else
                {
                    return "";
                }
            }
            else if (EditedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Boolean)
            {
                return false;
            }
            else
            {
                return 0;
            }*/
        }

        public void InsertRow(int rowindex = -1)
        {
            // Create a new row with default values.
            DataRow newrow = _currentTable.NewRow();
            List<object> defaultvalues = new List<object>();
            for (int i = 0; i < EditedFile.CurrentType.ColumnDefinitions.Count; i++)
            {
                defaultvalues.Add(GetDefaultValue(i));
            }

            newrow.ItemArray = defaultvalues.ToArray();

            if (rowindex > -1)
            {
                _currentTable.Rows.InsertAt(newrow, rowindex);

                // Once the new row is added to the table, we need to move the row in the editedFile.
                DBRow temprow = EditedFile.Entries[EditedFile.Entries.Count - 1];
                EditedFile.Entries.Remove(temprow);
                EditedFile.Entries.Insert(rowindex, temprow);

                // Now that everything is in its proper place, check for errors.
                CheckRowForErrors(rowindex);
            }
            else
            {
                // If the blank row is calling us, add to the end of the table.
                _currentTable.Rows.Add(newrow);
                CheckRowForErrors(_currentTable.Rows.Count - 1);
            }

            UpdateVisibleRows();
            dbDataGrid.Items.Refresh();
        }

        #endregion

        #region Filter Methods
        public void UpdateVisibleRows()
        {
            FilterSettings settings = new FilterSettings()
            {
                ErrorDockPanelVisible = dberrorsDockPanel.Visibility == System.Windows.Visibility.Visible,
                ErrorFilter = dberrorfilterCheckBox.IsChecked.Value,
                WarningFilter = dbwarningfilterCheckBox.IsChecked.Value
            };

            var filterList = (filterDockPanel.Children[0] as FilterController).filterList;
            FilterHelper.UpdateVisibleRows(this, filterList, settings);

        }
        #endregion

        #region Frozen Key Column Methods

        private void FreezeKeys()
        {
            // If there are no keys columns specified, return.
            if (_currentTable.PrimaryKey.Count() == 0)
            {
                return;
            }

            // Figure out which columns are key columns.
            List<string> keycolumns = new List<string>();
            keycolumns.AddRange(_currentTable.PrimaryKey.Select(n => n.ColumnName));

            for (int i = 0; i < keycolumns.Count; i++)
            {
                // Set the display index of the columns to left most column, retaining key order.
                dbDataGrid.Columns.Single(n => keycolumns[i].Equals((string)n.Header)).DisplayIndex = i;
            }

            dbDataGrid.FrozenColumnCount = keycolumns.Count;
        }

        private void UnfreezeKeys()
        {
            // If there are no keys columns specified, return.
            if (_currentTable.PrimaryKey.Count() == 0)
            {
                return;
            }

            // Figure out which columns are key columns.
            List<string> keycolumns = new List<string>();
            keycolumns.AddRange(_currentTable.PrimaryKey.Select(n => n.ColumnName));

            for (int i = 0; i < keycolumns.Count; i++)
            {
                // Reset the display index of the key columns back to their original positions.
                dbDataGrid.Columns.Single(n => keycolumns[i].Equals((string)n.Header)).DisplayIndex = _currentTable.Columns.IndexOf(keycolumns[i]);
            }

            dbDataGrid.FrozenColumnCount = 0;
        }

        #endregion

        #region Error Checking and Data Validation Methods

        private void checktableforerrorsButton_Click(object sender, RoutedEventArgs e)
        {
            _errorList.Clear();
            _currentTable.ClearErrors();
            CheckTableForErrors();

            if (_errorList.Count == 0)
            {
                // We didn't find any errors, inform the user.
                MessageBox.Show("No errors found in the current table!");
            }
        }

        // This method will be called when a table is loaded, generating an observable error list from the cached error data.
        private void RegenerateErrorList()
        {
            // If the table has no errors it either hasn't been checked or is fine, so skip it.
            if (!_currentTable.HasErrors)
            {
                return;
            }

            foreach (DataRow row in _currentTable.Rows)
            {
                if (!row.HasErrors)
                {
                    continue;
                }

                int rowindex = _currentTable.Rows.IndexOf(row);
                bool handledduplicatekey = false;
                foreach (DataColumn column in row.GetColumnsInError())
                {
                    DBError error = new DBError(rowindex, _currentTable.DefaultView[rowindex], column.ColumnName, row.GetColumnError(column));

                    if (error.ErrorMessage.Contains("key sequence"))
                    {
                        if (!handledduplicatekey)
                        {
                            _errorList.Add(error);
                        }

                        handledduplicatekey = true;
                    }
                    else
                    {
                        _errorList.Add(error);
                    }
                }
            }
        }

        private bool CheckTableForErrors(bool checkwarnings = true)
        {
            bool haserrors = false;
            _currentTable.ClearErrors();
            _errorList.Clear();

            for (int i = 0; i < _currentTable.Rows.Count; i++)
            {
                for (int j = 0; j < _currentTable.Columns.Count; j++)
                {
                    if (CheckCellForErrors(i, j, checkwarnings))
                    {
                        haserrors = true;
                    }
                    RefreshCell(i, j);
                }
            }

            return haserrors;
        }

        private bool CheckRowForErrors(int rowindex, bool checkwarnings = true)
        {
            bool haserrors = false;

            for (int i = 0; i < _currentTable.Columns.Count; i++)
            {
                if (CheckCellForErrors(rowindex, i, checkwarnings))
                {
                    haserrors = true;
                }
                RefreshCell(rowindex, i);
            }

            return haserrors;
        }

        // This methods is used to check for errors only, meaning: invalid data types and missing entries.
        private bool CheckCellForErrors(int rowindex, int colindex, bool checkwarnings = true)
        {
            bool haserrors = false;
            object currentvalue = _currentTable.Rows[rowindex][colindex];

            // Test value against required data type.
            if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Short)
            {
                short test;
                haserrors = !short.TryParse(currentvalue.ToString(), out test);

                if (haserrors)
                {
                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs a whole number between -32,768 and 32,767.",
                                                        currentvalue, _currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }
            else if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Integer)
            {
                int test;
                haserrors = !int.TryParse(currentvalue.ToString(), out test);

                if (haserrors)
                {
                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs a whole number.",
                                                        currentvalue, _currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }
            else if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Single)
            {
                float test;
                haserrors = !float.TryParse(currentvalue.ToString(), out test);

                if (haserrors)
                {
                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs a number (can have decimal point).",
                                                        currentvalue, _currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }
            else if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Boolean)
            {
                if(!(currentvalue.ToString().Equals("True") || currentvalue.ToString().Equals("False")))
                {
                    haserrors = true;

                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs 'True' or 'False'.",
                                                        currentvalue, _currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }

            // Next up, if we have a ComboBox column, make sure the current value exists.
            if (dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
            {
                if (!((dbDataGrid.Columns[colindex] as DataGridComboBoxColumn).ItemsSource as List<string>).Contains(currentvalue.ToString()))
                {
                    haserrors = true;

                    // Add an error to our internal list for display.
                    List<string> fkey = _currentTable.Columns[colindex].ExtendedProperties["FKey"].ToString().Split('.').ToList();
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', no matching value in column:'{3}' of '{2}' tables found.",
                                                        currentvalue, _currentTable.Columns[colindex].ColumnName, fkey[0], fkey[1]);
                    AddError(rowindex, colindex, errormessage);
                }
            }

            if (checkwarnings)
            {
                // Test the cell for warnings next, passing along our current cell's error state.
                CheckCellForWarnings(rowindex, colindex, haserrors);
            }

            // Lastly, update the visual error list and refresh the cell.
            UpdateErrorView();
            RefreshCell(rowindex, colindex);

            return haserrors;
		}

        // This method is designed to test for warnings only, meaning: duplicate keys.
        private bool CheckCellForWarnings(int rowindex, int colindex, bool haserrors = false)
        {
            // Check for duplications if this cell is a key.
            if (_currentTable.PrimaryKey.Contains(_currentTable.Columns[colindex]))
            {
                if (TableContainsKeySequence(GetPrimaryKeySequence(rowindex)))
                {
                    // Add an error to our internal list for display.
                    string keysequence = "";
                    foreach (string key in GetPrimaryKeySequence(rowindex))
                    {
                        keysequence += String.Format("'{0}' ", key);
                    }
                    string errormessage = String.Format("Warning: The key sequence {0} is already in this table.", keysequence);

                    // Log a warning for each row with a matching key.
                    foreach (int keyindex in GetMatchingKeyIndicies(GetPrimaryKeySequence(rowindex)))
                    {
                        // If the observable error list already has this error leave it be.
                        if (_errorList.Count(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex == keyindex) == 0)
                        {
                            AddError(keyindex, colindex, errormessage);

                            // There are more than 1 primary key, we need to create an internal error for each.
                            if (_currentTable.PrimaryKey.Count() > 1)
                            {
                                // Enumerate through the key columns that we have not added an error to yet.
                                foreach (DataColumn column in _currentTable.PrimaryKey)
                                {
                                    if (column.Ordinal != colindex)
                                    {
                                        _currentTable.Rows[keyindex].SetColumnError(column, errormessage);
                                    }

                                    // Also, refresh those additional cells.
                                    RefreshCell(keyindex, column.Ordinal);
                                }
                            }
                        }
                    }
                    haserrors = true;
                }
            }

            if (!haserrors)
            {
                // The cell is fine, make sure to remove any current errors.
                RemoveError(rowindex, colindex);
            }

            // Lastly, update the visual error list and refresh the cell.
            UpdateErrorView();
            RefreshCell(rowindex, colindex);

            return haserrors;
        }

        private bool ValueIsValid(object testval, int colindex)
        {
            // Test value against required data type.
            if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Short)
            {
                short test;
                return short.TryParse(testval.ToString(), out test);
            }
            else if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Integer)
            {
                int test;
                return int.TryParse(testval.ToString(), out test);
            }
            else if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Single)
            {
                float test;
                return float.TryParse(testval.ToString(), out test);
            }
            else if (EditedFile.CurrentType.ColumnDefinitions[colindex].Type == DbTypesEnum.Boolean)
            {
                if (!(testval.ToString().Equals("True") || testval.ToString().Equals("False")))
                {
                    return false;
                }
            }

            return true;
        }
		
		private void dberrorsListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dberrorsListView.SelectedItems.Count == 1)
            {
                DBError error = (DBError)dberrorsListView.SelectedItem;
                int rowindex = dbDataGrid.Items.IndexOf(error.GridRow);
                int colindex = dbDataGrid.Columns.IndexOf(dbDataGrid.Columns.Single(n => error.ColumnName.Equals((string)n.Header)));

                dbDataGrid.UnselectAllCells();
                dbDataGrid.ScrollIntoView(error.GridRow, dbDataGrid.Columns.Single(n => error.ColumnName.Equals((string)n.Header)));
                SelectCell(rowindex, colindex, true);
            }
        }

        private void AddError(int rowindex, int colindex, string errormessage)
        {
            // First, determine we have an error already logged for this cell and clear it.
            if (_currentTable.Rows[rowindex].GetColumnsInError().Contains(_currentTable.Columns[colindex]) &&
                _errorList.Count(n => n.ColumnName.Equals(_currentTable.Columns[colindex].ColumnName) && n.RowIndex == rowindex) > 0)
            {
                int errorindex = _errorList.IndexOf(_errorList.First(n => n.ColumnName.Equals(_currentTable.Columns[colindex].ColumnName) && n.RowIndex == rowindex));
                _currentTable.Rows[rowindex].RemoveColumnError(_currentTable.Columns[colindex].ColumnName);
                _errorList.RemoveAt(errorindex);

                // If we have an error, re-add the new error to the same location as the old one.
                DBError error = new DBError(rowindex, _currentTable.DefaultView[rowindex], _currentTable.Columns[colindex].ColumnName, errormessage);
                _errorList.Insert(errorindex, error);
            }
            else
            {
                // Add an error to our internal list for display.
                DBError error = new DBError(rowindex, _currentTable.DefaultView[rowindex], _currentTable.Columns[colindex].ColumnName, errormessage);
                _errorList.Add(error);
            }

            // Add an error into currentTable so that the background is updated accordingly.
            _currentTable.Rows[rowindex].SetColumnError(_currentTable.Columns[colindex], errormessage);

            UpdateErrorView();
        }

        private void RemoveError(int rowindex, int colindex)
        {
            // If this cell has an error, remove it.
            if (_currentTable.Rows[rowindex].GetColumnsInError().Contains(_currentTable.Columns[colindex]) &&
                (_errorList.Count(n => _currentTable.Rows[rowindex].GetColumnsInError().Count(t => t.ColumnName.Equals(n.ColumnName) && t.Ordinal == colindex) > 0) > 0))
            {
                string errormessage = _currentTable.Rows[rowindex].GetColumnError(colindex);
                if (_currentTable.PrimaryKey.Count() > 1 && errormessage.Contains("key sequence"))
                {
                    // If we are dealing with a duplicate key error in a table with multiple primary keys, we need to clear all key errors from the row.
                    foreach (string columnname in _currentTable.PrimaryKey.Select(n => n.ColumnName))
                    {
                        _currentTable.Rows[rowindex].RemoveColumnError(columnname);
                        RefreshCell(rowindex, _currentTable.Columns.IndexOf(columnname));
                    }
                }
                else
                {
                    // Other wise just remove the error.
                    _currentTable.Rows[rowindex].RemoveColumnError(_currentTable.Columns[colindex].ColumnName);
                    RefreshCell(rowindex, colindex);
                }

                // Check to see if there are any observable errors that match our current error and check them again.
                if (_errorList.Count(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != rowindex) > 0)
                {
                    // We then need to call CheckCellForWarnings again to test if the original (first appearance of) key is now unique.
                    int firstrowindex = _errorList.Where(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != rowindex).Select(n => n.RowIndex).Min();
                    int firstcolindex = _currentTable.Columns.IndexOf(_currentTable.PrimaryKey[0]);
                    CheckCellForWarnings(firstrowindex, firstcolindex);
                }

                // Remove the error from the observable list.
                _errorList.Remove(_errorList.First(n => n.ColumnName.Equals(_currentTable.Columns[colindex].ColumnName) && n.RowIndex == rowindex));
            }
        }

        private void UpdateErrorView()
        {
            // Update our visual error list, collapsing and expanding it as necessary.
            if (_errorList.Count == 0)
            {
                errornumberTextBlock.Text = "0";
                warningnumberTextBlock.Text = "0";
                dberrorsDockPanel.Visibility = System.Windows.Visibility.Collapsed;
                dberrorfilterCheckBox.IsChecked = false;
                dbwarningfilterCheckBox.IsChecked = false;
            }
            else
            {
                errornumberTextBlock.Text = _errorList.Count(n => n.ErrorMessage.Contains("Error:")).ToString();
                warningnumberTextBlock.Text = _errorList.Count(n => n.ErrorMessage.Contains("Warning:")).ToString();
                dberrorsDockPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void dberrorwarningfilterCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Update the error list visibility properties.
            UpdateErrorView();

            UpdateVisibleRows();
            dbDataGrid.Items.Refresh();
        }

        private void minimizeerrorlistCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            dberrorsListView.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void minimizeerrorlistCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            dberrorsListView.Visibility = System.Windows.Visibility.Visible;
        }

        public void Dispose()
        {
        }

        #endregion
    }

    public static class Extensions
    {
        public static List<object> GetItemArray(this DataColumn column, bool getblanks = true)
        {
            DataTable table = column.Table;
            List<object> Items = new List<object>();

            foreach (DataRow row in table.Rows)
            {
                if (String.IsNullOrEmpty(row[column].ToString()) && !getblanks)
                {
                    continue;
                }
                Items.Add(row[column]);
            }

            Items.Sort();

            return Items;
        }

        public static void RemoveColumnError(this DataRow row, string columnName)
        {
            // Grab a list of columns in error for this row.
            List<DataColumn> columnsinerror = new List<DataColumn>();
            columnsinerror = row.GetColumnsInError().ToList();
            columnsinerror.Remove(row.Table.Columns[columnName]);

            // Since we are not working with a reference, we need to clear and re-add every error in the row.
            List<KeyValuePair<DataColumn, string>> columnanderrorlist = new List<KeyValuePair<DataColumn, string>>();
            foreach (DataColumn column in columnsinerror)
            {
                columnanderrorlist.Add(new KeyValuePair<DataColumn, string>(column, row.GetColumnError(column)));
            }

            // Now that we have this list, clear all errors and re-add.
            row.ClearErrors();

            foreach (KeyValuePair<DataColumn, string> columnanderror in columnanderrorlist)
            {
                row.SetColumnError(columnanderror.Key, columnanderror.Value);
            }
        }

        public static void ClearErrors(this DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                row.ClearErrors();
            }
        }
    }
}
