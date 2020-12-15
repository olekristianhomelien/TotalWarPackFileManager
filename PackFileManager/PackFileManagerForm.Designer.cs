namespace PackFileManager
{
    partial class PackFileManagerForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                Utilities.DisposeHandlers(this);
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackFileManagerForm));
            this.packActionMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextAddMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextAddFromPackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextAddDirMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextAddEmptyDirMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextAddFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextImportTsvMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextRenameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.contextOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextOpenExternalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextOpenDecodeToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextOpenTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextExtractMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextExtractSelectedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextExtractAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractUnknownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFromPackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emptyDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._packTreeView = new PackFileManager.PackedTreeView.PackedTreeView();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.newModMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.editModMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installModMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallModMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openModPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.changePackTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shaderItemSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.setShaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.exportFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openExternalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDecodeToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openAsTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportUnknownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllTsv = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.createReadMeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.postProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameMultiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.loadGamePacksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.openGameDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDataDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEncyclopediaDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.openUserDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openScriptsDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openReplaysDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extrasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cAPacksAreReadOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateOnStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subscribeToBetaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDecodeToolOnErrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractTSVFileExtensionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.csvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateDBFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dBDecoderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.indexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.packStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.packActionProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.packTreeViewToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.packActionMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // packActionMenuStrip
            // 
            this.packActionMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextAddMenuItem,
            this.contextDeleteMenuItem,
            this.contextRenameMenuItem,
            this.toolStripSeparator10,
            this.contextOpenMenuItem,
            this.contextExtractMenuItem,
            this.expandToolStripMenuItem});
            this.packActionMenuStrip.Name = "packActionMenuStrip";
            this.packActionMenuStrip.Size = new System.Drawing.Size(132, 142);
            // 
            // contextAddMenuItem
            // 
            this.contextAddMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextAddFromPackToolStripMenuItem,
            this.contextAddDirMenuItem,
            this.contextAddEmptyDirMenuItem,
            this.contextAddFileMenuItem,
            this.contextImportTsvMenuItem});
            this.contextAddMenuItem.Name = "contextAddMenuItem";
            this.contextAddMenuItem.Size = new System.Drawing.Size(131, 22);
            this.contextAddMenuItem.Text = "Add";
            // 
            // contextAddFromPackToolStripMenuItem
            // 
            this.contextAddFromPackToolStripMenuItem.Name = "contextAddFromPackToolStripMenuItem";
            this.contextAddFromPackToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.contextAddFromPackToolStripMenuItem.Text = "From Pack";
            this.contextAddFromPackToolStripMenuItem.Click += new System.EventHandler(this.AddFromPack);
            // 
            // contextAddDirMenuItem
            // 
            this.contextAddDirMenuItem.Name = "contextAddDirMenuItem";
            this.contextAddDirMenuItem.ShortcutKeyDisplayString = "Shift+Ins";
            this.contextAddDirMenuItem.Size = new System.Drawing.Size(185, 22);
            this.contextAddDirMenuItem.Text = "&Directory...";
            this.contextAddDirMenuItem.Click += new System.EventHandler(this.addDirectoryToolStripMenuItem_Click);
            // 
            // contextAddEmptyDirMenuItem
            // 
            this.contextAddEmptyDirMenuItem.Name = "contextAddEmptyDirMenuItem";
            this.contextAddEmptyDirMenuItem.Size = new System.Drawing.Size(185, 22);
            this.contextAddEmptyDirMenuItem.Text = "Empty Directory";
            this.contextAddEmptyDirMenuItem.Click += new System.EventHandler(this.emptyDirectoryToolStripMenuItem_Click);
            // 
            // contextAddFileMenuItem
            // 
            this.contextAddFileMenuItem.Name = "contextAddFileMenuItem";
            this.contextAddFileMenuItem.ShortcutKeyDisplayString = "Ins";
            this.contextAddFileMenuItem.Size = new System.Drawing.Size(185, 22);
            this.contextAddFileMenuItem.Text = "&File(s)...";
            this.contextAddFileMenuItem.Click += new System.EventHandler(this.addFileToolStripMenuItem_Click);
            // 
            // contextImportTsvMenuItem
            // 
            this.contextImportTsvMenuItem.Name = "contextImportTsvMenuItem";
            this.contextImportTsvMenuItem.Size = new System.Drawing.Size(185, 22);
            this.contextImportTsvMenuItem.Text = "DB file from TSV";
            this.contextImportTsvMenuItem.Click += new System.EventHandler(this.dBFileFromTSVToolStripMenuItem_Click);
            // 
            // contextDeleteMenuItem
            // 
            this.contextDeleteMenuItem.Name = "contextDeleteMenuItem";
            this.contextDeleteMenuItem.ShortcutKeyDisplayString = "Del";
            this.contextDeleteMenuItem.Size = new System.Drawing.Size(131, 22);
            this.contextDeleteMenuItem.Text = "Delete";
            this.contextDeleteMenuItem.Click += new System.EventHandler(this.deleteFileToolStripMenuItem_Click);
            // 
            // contextRenameMenuItem
            // 
            this.contextRenameMenuItem.Name = "contextRenameMenuItem";
            this.contextRenameMenuItem.Size = new System.Drawing.Size(131, 22);
            this.contextRenameMenuItem.Text = "Rename";
            this.contextRenameMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(128, 6);
            // 
            // contextOpenMenuItem
            // 
            this.contextOpenMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextOpenExternalMenuItem,
            this.contextOpenDecodeToolMenuItem,
            this.contextOpenTextMenuItem});
            this.contextOpenMenuItem.Name = "contextOpenMenuItem";
            this.contextOpenMenuItem.Size = new System.Drawing.Size(131, 22);
            this.contextOpenMenuItem.Text = "Open";
            // 
            // contextOpenExternalMenuItem
            // 
            this.contextOpenExternalMenuItem.Name = "contextOpenExternalMenuItem";
            this.contextOpenExternalMenuItem.Size = new System.Drawing.Size(177, 22);
            this.contextOpenExternalMenuItem.Text = "Open External...";
            this.contextOpenExternalMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // contextOpenDecodeToolMenuItem
            // 
            this.contextOpenDecodeToolMenuItem.Name = "contextOpenDecodeToolMenuItem";
            this.contextOpenDecodeToolMenuItem.Size = new System.Drawing.Size(177, 22);
            this.contextOpenDecodeToolMenuItem.Text = "Open DecodeTool...";
            this.contextOpenDecodeToolMenuItem.Click += new System.EventHandler(this.openDecodeToolMenuItem_Click);
            // 
            // contextOpenTextMenuItem
            // 
            this.contextOpenTextMenuItem.Name = "contextOpenTextMenuItem";
            this.contextOpenTextMenuItem.Size = new System.Drawing.Size(177, 22);
            this.contextOpenTextMenuItem.Text = "Open as Text";
            this.contextOpenTextMenuItem.Click += new System.EventHandler(this.openAsTextMenuItem_Click);
            // 
            // contextExtractMenuItem
            // 
            this.contextExtractMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextExtractSelectedMenuItem,
            this.contextExtractAllMenuItem,
            this.extractUnknownToolStripMenuItem});
            this.contextExtractMenuItem.Name = "contextExtractMenuItem";
            this.contextExtractMenuItem.Size = new System.Drawing.Size(131, 22);
            this.contextExtractMenuItem.Text = "Extract";
            // 
            // contextExtractSelectedMenuItem
            // 
            this.contextExtractSelectedMenuItem.Name = "contextExtractSelectedMenuItem";
            this.contextExtractSelectedMenuItem.ShortcutKeyDisplayString = "Ctl+X";
            this.contextExtractSelectedMenuItem.Size = new System.Drawing.Size(203, 22);
            this.contextExtractSelectedMenuItem.Text = "Extract &Selected...";
            this.contextExtractSelectedMenuItem.Click += new System.EventHandler(this.extractSelectedToolStripMenuItem_Click);
            // 
            // contextExtractAllMenuItem
            // 
            this.contextExtractAllMenuItem.Name = "contextExtractAllMenuItem";
            this.contextExtractAllMenuItem.Size = new System.Drawing.Size(203, 22);
            this.contextExtractAllMenuItem.Text = "Extract &All...";
            this.contextExtractAllMenuItem.Click += new System.EventHandler(this.extractAllToolStripMenuItem_Click);
            // 
            // extractUnknownToolStripMenuItem
            // 
            this.extractUnknownToolStripMenuItem.Name = "extractUnknownToolStripMenuItem";
            this.extractUnknownToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.extractUnknownToolStripMenuItem.Text = "Extract Unknown...";
            this.extractUnknownToolStripMenuItem.Click += new System.EventHandler(this.exportUnknownToolStripMenuItem_Click);
            // 
            // expandToolStripMenuItem
            // 
            this.expandToolStripMenuItem.Name = "expandToolStripMenuItem";
            this.expandToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.expandToolStripMenuItem.Text = "Expand";
            this.expandToolStripMenuItem.Click += new System.EventHandler(this.expandToolStripMenuItem_Click);
            // 
            // extractSelectedToolStripMenuItem
            // 
            this.extractSelectedToolStripMenuItem.Name = "extractSelectedToolStripMenuItem";
            this.extractSelectedToolStripMenuItem.ShortcutKeyDisplayString = "Ctl+X";
            this.extractSelectedToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.extractSelectedToolStripMenuItem.Text = "Extract &Selected...";
            this.extractSelectedToolStripMenuItem.Click += new System.EventHandler(this.extractSelectedToolStripMenuItem_Click);
            // 
            // extractAllToolStripMenuItem
            // 
            this.extractAllToolStripMenuItem.Name = "extractAllToolStripMenuItem";
            this.extractAllToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.extractAllToolStripMenuItem.Text = "Extract &All...";
            this.extractAllToolStripMenuItem.Click += new System.EventHandler(this.extractAllToolStripMenuItem_Click);
            // 
            // renameSelectedToolStripMenuItem
            // 
            this.renameSelectedToolStripMenuItem.Name = "renameSelectedToolStripMenuItem";
            this.renameSelectedToolStripMenuItem.ShortcutKeyDisplayString = "Ctl+X";
            this.renameSelectedToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.renameSelectedToolStripMenuItem.Text = "Rename &Selected...";
            this.renameSelectedToolStripMenuItem.Click += new System.EventHandler(this.renameSelectedToolStripMenuItem_Click);
            // 
            // renameAllToolStripMenuItem
            // 
            this.renameAllToolStripMenuItem.Name = "renameAllToolStripMenuItem";
            this.renameAllToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.renameAllToolStripMenuItem.Text = "Rename &All...";
            this.renameAllToolStripMenuItem.Click += new System.EventHandler(this.renameAllToolStripMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFromPackToolStripMenuItem,
            this.emptyDirectoryToolStripMenuItem,
            this.addDirectoryToolStripMenuItem,
            this.addFileToolStripMenuItem,
            this.importTSVToolStripMenuItem});
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.addToolStripMenuItem.Text = "Add";
            // 
            // addFromPackToolStripMenuItem
            // 
            this.addFromPackToolStripMenuItem.Name = "addFromPackToolStripMenuItem";
            this.addFromPackToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.addFromPackToolStripMenuItem.Text = "From &Pack...";
            this.addFromPackToolStripMenuItem.Click += new System.EventHandler(this.AddFromPack);
            // 
            // emptyDirectoryToolStripMenuItem
            // 
            this.emptyDirectoryToolStripMenuItem.Name = "emptyDirectoryToolStripMenuItem";
            this.emptyDirectoryToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.emptyDirectoryToolStripMenuItem.Text = "Empty Directory";
            this.emptyDirectoryToolStripMenuItem.Click += new System.EventHandler(this.emptyDirectoryToolStripMenuItem_Click);
            // 
            // addDirectoryToolStripMenuItem
            // 
            this.addDirectoryToolStripMenuItem.Name = "addDirectoryToolStripMenuItem";
            this.addDirectoryToolStripMenuItem.ShortcutKeyDisplayString = "Shift+Ins";
            this.addDirectoryToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.addDirectoryToolStripMenuItem.Text = "&Directory...";
            this.addDirectoryToolStripMenuItem.Click += new System.EventHandler(this.addDirectoryToolStripMenuItem_Click);
            // 
            // addFileToolStripMenuItem
            // 
            this.addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            this.addFileToolStripMenuItem.ShortcutKeyDisplayString = "Ins";
            this.addFileToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.addFileToolStripMenuItem.Text = "&File(s)...";
            this.addFileToolStripMenuItem.Click += new System.EventHandler(this.addFileToolStripMenuItem_Click);
            // 
            // importTSVToolStripMenuItem
            // 
            this.importTSVToolStripMenuItem.Name = "importTSVToolStripMenuItem";
            this.importTSVToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.importTSVToolStripMenuItem.Text = "Import TSV";
            this.importTSVToolStripMenuItem.Click += new System.EventHandler(this.dBFileFromTSVToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(-2, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._packTreeView);
            this.splitContainer1.Size = new System.Drawing.Size(1772, 760);
            this.splitContainer1.SplitterDistance = 194;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 9;
            // 
            // _packTreeView
            // 
            this._packTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._packTreeView.Location = new System.Drawing.Point(0, 0);
            this._packTreeView.Name = "_packTreeView";
            this._packTreeView.Size = new System.Drawing.Size(190, 756);
            this._packTreeView.TabIndex = 0;
            this._packTreeView.TreeViewColourHelper = null;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.filesMenu,
            this.postProcessToolStripMenuItem,
            this.editToolStripMenuItem,
            this.gameToolStripMenuItem,
            this.extrasToolStripMenuItem,
            this.updateToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1769, 24);
            this.menuStrip.TabIndex = 10;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.modsToolStripMenuItem,
            this.openCAToolStripMenuItem,
            this.recentFilesMenuItem,
            this.toolStripSeparator1,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.changePackTypeToolStripMenuItem,
            this.toolStripSeparator9,
            this.exportFileListToolStripMenuItem,
            this.toolStripSeparator7,
            this.toolStripMenuItem1,
            this.toolStripSeparator12,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OnFileDropDownOpening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // modsToolStripMenuItem
            // 
            this.modsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator13,
            this.newModMenuItem,
            this.deleteCurrentToolStripMenuItem,
            this.toolStripSeparator11,
            this.editModMenuItem,
            this.installModMenuItem,
            this.uninstallModMenuItem,
            this.openModPathToolStripMenuItem});
            this.modsToolStripMenuItem.Name = "modsToolStripMenuItem";
            this.modsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.modsToolStripMenuItem.Text = "My Mods";
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(160, 6);
            // 
            // newModMenuItem
            // 
            this.newModMenuItem.Name = "newModMenuItem";
            this.newModMenuItem.Size = new System.Drawing.Size(163, 22);
            this.newModMenuItem.Text = "New";
            this.newModMenuItem.Click += new System.EventHandler(this.newModMenuItem_Click);
            // 
            // deleteCurrentToolStripMenuItem
            // 
            this.deleteCurrentToolStripMenuItem.Name = "deleteCurrentToolStripMenuItem";
            this.deleteCurrentToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.deleteCurrentToolStripMenuItem.Text = "Delete Current";
            this.deleteCurrentToolStripMenuItem.Click += new System.EventHandler(this.deleteCurrentToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(160, 6);
            // 
            // editModMenuItem
            // 
            this.editModMenuItem.Name = "editModMenuItem";
            this.editModMenuItem.Size = new System.Drawing.Size(163, 22);
            this.editModMenuItem.Text = "Edit Current";
            this.editModMenuItem.Visible = false;
            // 
            // installModMenuItem
            // 
            this.installModMenuItem.Name = "installModMenuItem";
            this.installModMenuItem.Size = new System.Drawing.Size(163, 22);
            this.installModMenuItem.Text = "Install Current";
            this.installModMenuItem.Click += new System.EventHandler(this.installModMenuItem_Click);
            // 
            // uninstallModMenuItem
            // 
            this.uninstallModMenuItem.Name = "uninstallModMenuItem";
            this.uninstallModMenuItem.Size = new System.Drawing.Size(163, 22);
            this.uninstallModMenuItem.Text = "Uninstall Current";
            this.uninstallModMenuItem.Click += new System.EventHandler(this.uninstallModMenuItem_Click);
            // 
            // openModPathToolStripMenuItem
            // 
            this.openModPathToolStripMenuItem.Name = "openModPathToolStripMenuItem";
            this.openModPathToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.openModPathToolStripMenuItem.Text = "Open Data Path";
            this.openModPathToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // openCAToolStripMenuItem
            // 
            this.openCAToolStripMenuItem.Enabled = false;
            this.openCAToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openCAToolStripMenuItem.Name = "openCAToolStripMenuItem";
            this.openCAToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.openCAToolStripMenuItem.Text = "Open CA pack...";
            // 
            // recentFilesMenuItem
            // 
            this.recentFilesMenuItem.Name = "recentFilesMenuItem";
            this.recentFilesMenuItem.Size = new System.Drawing.Size(170, 22);
            this.recentFilesMenuItem.Text = "Recent Files";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(167, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveCurrentPack);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.QueryNameAndSave);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(167, 6);
            // 
            // changePackTypeToolStripMenuItem
            // 
            this.changePackTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shaderItemSeparator,
            this.setShaderToolStripMenuItem});
            this.changePackTypeToolStripMenuItem.Enabled = false;
            this.changePackTypeToolStripMenuItem.Name = "changePackTypeToolStripMenuItem";
            this.changePackTypeToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.changePackTypeToolStripMenuItem.Text = "Change Pack &Type";
            // 
            // shaderItemSeparator
            // 
            this.shaderItemSeparator.Name = "shaderItemSeparator";
            this.shaderItemSeparator.Size = new System.Drawing.Size(107, 6);
            // 
            // setShaderToolStripMenuItem
            // 
            this.setShaderToolStripMenuItem.Enabled = false;
            this.setShaderToolStripMenuItem.Name = "setShaderToolStripMenuItem";
            this.setShaderToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.setShaderToolStripMenuItem.Text = "Shader";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(167, 6);
            // 
            // exportFileListToolStripMenuItem
            // 
            this.exportFileListToolStripMenuItem.Enabled = false;
            this.exportFileListToolStripMenuItem.Name = "exportFileListToolStripMenuItem";
            this.exportFileListToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.exportFileListToolStripMenuItem.Text = "Export File &List...";
            this.exportFileListToolStripMenuItem.Click += new System.EventHandler(this.ExportFileList);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(167, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(170, 22);
            this.toolStripMenuItem1.Text = "Settings";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuSettings_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(167, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // filesMenu
            // 
            this.filesMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.replaceFileToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.toolStripSeparator4,
            this.openMenuItem,
            this.extractToolStripMenuItem,
            this.toolStripSeparator8,
            this.createReadMeToolStripMenuItem,
            this.searchFileToolStripMenuItem});
            this.filesMenu.Enabled = false;
            this.filesMenu.Name = "filesMenu";
            this.filesMenu.Size = new System.Drawing.Size(42, 20);
            this.filesMenu.Text = "Files";
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Enabled = false;
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.ShortcutKeyDisplayString = "Del";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.deleteFileToolStripMenuItem.Text = "Delete";
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.deleteFileToolStripMenuItem_Click);
            // 
            // replaceFileToolStripMenuItem
            // 
            this.replaceFileToolStripMenuItem.Name = "replaceFileToolStripMenuItem";
            this.replaceFileToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.replaceFileToolStripMenuItem.Text = "&Replace File...";
            this.replaceFileToolStripMenuItem.Click += new System.EventHandler(this.replaceFileToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(151, 6);
            // 
            // openMenuItem
            // 
            this.openMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openExternalMenuItem,
            this.openDecodeToolMenuItem,
            this.openAsTextMenuItem});
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.Size = new System.Drawing.Size(154, 22);
            this.openMenuItem.Text = "Open";
            // 
            // openExternalMenuItem
            // 
            this.openExternalMenuItem.Name = "openExternalMenuItem";
            this.openExternalMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openExternalMenuItem.Text = "Open External...";
            this.openExternalMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openDecodeToolMenuItem
            // 
            this.openDecodeToolMenuItem.Name = "openDecodeToolMenuItem";
            this.openDecodeToolMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openDecodeToolMenuItem.Text = "Open DecodeTool...";
            this.openDecodeToolMenuItem.Click += new System.EventHandler(this.openDecodeToolMenuItem_Click);
            // 
            // openAsTextMenuItem
            // 
            this.openAsTextMenuItem.Name = "openAsTextMenuItem";
            this.openAsTextMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openAsTextMenuItem.Text = "Open as Text";
            this.openAsTextMenuItem.Click += new System.EventHandler(this.openAsTextMenuItem_Click);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractSelectedToolStripMenuItem,
            this.extractAllToolStripMenuItem,
            this.exportUnknownToolStripMenuItem,
            this.extractAllTsv});
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.extractToolStripMenuItem.Text = "Extract";
            // 
            // exportUnknownToolStripMenuItem
            // 
            this.exportUnknownToolStripMenuItem.Name = "exportUnknownToolStripMenuItem";
            this.exportUnknownToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.exportUnknownToolStripMenuItem.Text = "Extract Unknown...";
            this.exportUnknownToolStripMenuItem.Click += new System.EventHandler(this.exportUnknownToolStripMenuItem_Click);
            // 
            // extractAllTsv
            // 
            this.extractAllTsv.Name = "extractAllTsv";
            this.extractAllTsv.Size = new System.Drawing.Size(203, 22);
            this.extractAllTsv.Text = "Extract All as TSV...";
            this.extractAllTsv.Click += new System.EventHandler(this.extractAllTsv_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(151, 6);
            // 
            // createReadMeToolStripMenuItem
            // 
            this.createReadMeToolStripMenuItem.Enabled = false;
            this.createReadMeToolStripMenuItem.Name = "createReadMeToolStripMenuItem";
            this.createReadMeToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.createReadMeToolStripMenuItem.Text = "Create ReadMe";
            this.createReadMeToolStripMenuItem.Click += new System.EventHandler(this.createReadMeToolStripMenuItem_Click);
            // 
            // searchFileToolStripMenuItem
            // 
            this.searchFileToolStripMenuItem.Enabled = false;
            this.searchFileToolStripMenuItem.Name = "searchFileToolStripMenuItem";
            this.searchFileToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.searchFileToolStripMenuItem.Text = "Search Files...";
            // 
            // postProcessToolStripMenuItem
            // 
            this.postProcessToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameMultiToolStripMenuItem,
            this.minimizeToolStripMenuItem});
            this.postProcessToolStripMenuItem.Name = "postProcessToolStripMenuItem";
            this.postProcessToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.postProcessToolStripMenuItem.Text = "Postprocess";
            // 
            // renameMultiToolStripMenuItem
            // 
            this.renameMultiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameSelectedToolStripMenuItem,
            this.renameAllToolStripMenuItem});
            this.renameMultiToolStripMenuItem.Name = "renameMultiToolStripMenuItem";
            this.renameMultiToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.renameMultiToolStripMenuItem.Text = "Rename";
            // 
            // minimizeToolStripMenuItem
            // 
            this.minimizeToolStripMenuItem.Enabled = false;
            this.minimizeToolStripMenuItem.Name = "minimizeToolStripMenuItem";
            this.minimizeToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.minimizeToolStripMenuItem.Text = "Minimize DB Files";
            this.minimizeToolStripMenuItem.Click += new System.EventHandler(this.MinimizeDbFiles);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator3,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator6,
            this.selectAllToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            this.editToolStripMenuItem.Visible = false;
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.redoToolStripMenuItem.Text = "&Redo";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(141, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.cutToolStripMenuItem.Text = "Cu&t";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.pasteToolStripMenuItem.Text = "&Paste";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(141, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator14,
            this.loadGamePacksToolStripMenuItem,
            this.toolStripSeparator15,
            this.openGameDirToolStripMenuItem,
            this.openDataDirToolStripMenuItem,
            this.openEncyclopediaDirToolStripMenuItem,
            this.toolStripSeparator16,
            this.openUserDirToolStripMenuItem,
            this.openScriptsDirToolStripMenuItem,
            this.openReplaysDirToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.gameToolStripMenuItem.Text = "Game";
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(224, 6);
            // 
            // loadGamePacksToolStripMenuItem
            // 
            this.loadGamePacksToolStripMenuItem.Name = "loadGamePacksToolStripMenuItem";
            this.loadGamePacksToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.loadGamePacksToolStripMenuItem.Text = "Load all Game Packs";
            this.loadGamePacksToolStripMenuItem.Click += new System.EventHandler(this.loadGamePacksToolStripMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(224, 6);
            // 
            // openGameDirToolStripMenuItem
            // 
            this.openGameDirToolStripMenuItem.Name = "openGameDirToolStripMenuItem";
            this.openGameDirToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openGameDirToolStripMenuItem.Text = "Open Game Directory";
            this.openGameDirToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // openDataDirToolStripMenuItem
            // 
            this.openDataDirToolStripMenuItem.Name = "openDataDirToolStripMenuItem";
            this.openDataDirToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openDataDirToolStripMenuItem.Text = "Open Data Directory";
            this.openDataDirToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // openEncyclopediaDirToolStripMenuItem
            // 
            this.openEncyclopediaDirToolStripMenuItem.Name = "openEncyclopediaDirToolStripMenuItem";
            this.openEncyclopediaDirToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openEncyclopediaDirToolStripMenuItem.Text = "Open Encyclopedia Directory";
            this.openEncyclopediaDirToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(224, 6);
            // 
            // openUserDirToolStripMenuItem
            // 
            this.openUserDirToolStripMenuItem.Name = "openUserDirToolStripMenuItem";
            this.openUserDirToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openUserDirToolStripMenuItem.Text = "Open User Directory";
            this.openUserDirToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // openScriptsDirToolStripMenuItem
            // 
            this.openScriptsDirToolStripMenuItem.Name = "openScriptsDirToolStripMenuItem";
            this.openScriptsDirToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openScriptsDirToolStripMenuItem.Text = "Open Scripts Directory";
            this.openScriptsDirToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // openReplaysDirToolStripMenuItem
            // 
            this.openReplaysDirToolStripMenuItem.Name = "openReplaysDirToolStripMenuItem";
            this.openReplaysDirToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openReplaysDirToolStripMenuItem.Text = "Open Replays Directory";
            this.openReplaysDirToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // extrasToolStripMenuItem
            // 
            this.extrasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cAPacksAreReadOnlyToolStripMenuItem,
            this.updateOnStartupToolStripMenuItem,
            this.subscribeToBetaToolStripMenuItem,
            this.showDecodeToolOnErrorToolStripMenuItem,
            this.extractTSVFileExtensionToolStripMenuItem});
            this.extrasToolStripMenuItem.Name = "extrasToolStripMenuItem";
            this.extrasToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.extrasToolStripMenuItem.Text = "Options";
            // 
            // cAPacksAreReadOnlyToolStripMenuItem
            // 
            this.cAPacksAreReadOnlyToolStripMenuItem.Checked = true;
            this.cAPacksAreReadOnlyToolStripMenuItem.CheckOnClick = true;
            this.cAPacksAreReadOnlyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cAPacksAreReadOnlyToolStripMenuItem.Name = "cAPacksAreReadOnlyToolStripMenuItem";
            this.cAPacksAreReadOnlyToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.cAPacksAreReadOnlyToolStripMenuItem.Text = "CA Packs Are Read Only";
            this.cAPacksAreReadOnlyToolStripMenuItem.ToolTipText = "If checked, the original pack files for the game can be viewed but not edited.";
            this.cAPacksAreReadOnlyToolStripMenuItem.CheckedChanged += new System.EventHandler(this.cAPacksAreReadOnlyToolStripMenuItem_CheckStateChanged);
            // 
            // updateOnStartupToolStripMenuItem
            // 
            this.updateOnStartupToolStripMenuItem.CheckOnClick = true;
            this.updateOnStartupToolStripMenuItem.Name = "updateOnStartupToolStripMenuItem";
            this.updateOnStartupToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.updateOnStartupToolStripMenuItem.Text = "Update on Startup";
            this.updateOnStartupToolStripMenuItem.Click += new System.EventHandler(this.updateOnStartupToolStripMenuItem_Click);
            // 
            // subscribeToBetaToolStripMenuItem
            // 
            this.subscribeToBetaToolStripMenuItem.CheckOnClick = true;
            this.subscribeToBetaToolStripMenuItem.Name = "subscribeToBetaToolStripMenuItem";
            this.subscribeToBetaToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.subscribeToBetaToolStripMenuItem.Text = "Use Beta Schemas";
            this.subscribeToBetaToolStripMenuItem.Click += new System.EventHandler(this.subscribeToBetaToolStripMenuItem_Click);
            // 
            // showDecodeToolOnErrorToolStripMenuItem
            // 
            this.showDecodeToolOnErrorToolStripMenuItem.CheckOnClick = true;
            this.showDecodeToolOnErrorToolStripMenuItem.Name = "showDecodeToolOnErrorToolStripMenuItem";
            this.showDecodeToolOnErrorToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.showDecodeToolOnErrorToolStripMenuItem.Text = "Show Decode Tool on Error";
            this.showDecodeToolOnErrorToolStripMenuItem.Click += new System.EventHandler(this.showDecodeToolOnErrorToolStripMenuItem_Click);
            // 
            // extractTSVFileExtensionToolStripMenuItem
            // 
            this.extractTSVFileExtensionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.csvToolStripMenuItem,
            this.tsvToolStripMenuItem});
            this.extractTSVFileExtensionToolStripMenuItem.Name = "extractTSVFileExtensionToolStripMenuItem";
            this.extractTSVFileExtensionToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.extractTSVFileExtensionToolStripMenuItem.Text = "Extract TSV File Extension";
            // 
            // csvToolStripMenuItem
            // 
            this.csvToolStripMenuItem.Checked = true;
            this.csvToolStripMenuItem.CheckOnClick = true;
            this.csvToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.csvToolStripMenuItem.Name = "csvToolStripMenuItem";
            this.csvToolStripMenuItem.Size = new System.Drawing.Size(91, 22);
            this.csvToolStripMenuItem.Text = "csv";
            this.csvToolStripMenuItem.Click += new System.EventHandler(this.extensionSelectionChanged);
            // 
            // tsvToolStripMenuItem
            // 
            this.tsvToolStripMenuItem.CheckOnClick = true;
            this.tsvToolStripMenuItem.Name = "tsvToolStripMenuItem";
            this.tsvToolStripMenuItem.Size = new System.Drawing.Size(91, 22);
            this.tsvToolStripMenuItem.Text = "tsv";
            this.tsvToolStripMenuItem.Click += new System.EventHandler(this.extensionSelectionChanged);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reloadToolStripMenuItem,
            this.saveToDirectoryToolStripMenuItem,
            this.updateDBFilesToolStripMenuItem,
            this.dBDecoderToolStripMenuItem});
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(102, 20);
            this.updateToolStripMenuItem.Text = "DB Descriptions";
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.reloadToolStripMenuItem.Text = "Load from File";
            this.reloadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
            // 
            // saveToDirectoryToolStripMenuItem
            // 
            this.saveToDirectoryToolStripMenuItem.Name = "saveToDirectoryToolStripMenuItem";
            this.saveToDirectoryToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.saveToDirectoryToolStripMenuItem.Text = "Save to Directory";
            this.saveToDirectoryToolStripMenuItem.Click += new System.EventHandler(this.saveToDirectoryToolStripMenuItem_Click);
            // 
            // updateDBFilesToolStripMenuItem
            // 
            this.updateDBFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateCurrentToolStripMenuItem,
            this.updateAllToolStripMenuItem});
            this.updateDBFilesToolStripMenuItem.Name = "updateDBFilesToolStripMenuItem";
            this.updateDBFilesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.updateDBFilesToolStripMenuItem.Text = "Update DB Files";
            // 
            // updateCurrentToolStripMenuItem
            // 
            this.updateCurrentToolStripMenuItem.Name = "updateCurrentToolStripMenuItem";
            this.updateCurrentToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.updateCurrentToolStripMenuItem.Text = "Update Current";
            this.updateCurrentToolStripMenuItem.Click += new System.EventHandler(this.updateCurrentToolStripMenuItem_Click);
            // 
            // updateAllToolStripMenuItem
            // 
            this.updateAllToolStripMenuItem.Name = "updateAllToolStripMenuItem";
            this.updateAllToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.updateAllToolStripMenuItem.Text = "Update All";
            this.updateAllToolStripMenuItem.Click += new System.EventHandler(this.updateAllToolStripMenuItem_Click);
            // 
            // dBDecoderToolStripMenuItem
            // 
            this.dBDecoderToolStripMenuItem.Name = "dBDecoderToolStripMenuItem";
            this.dBDecoderToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.dBDecoderToolStripMenuItem.Text = "DB Decoder";
            this.dBDecoderToolStripMenuItem.Click += new System.EventHandler(this.dBDecoderToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.indexToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.searchForUpdateToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.contentsToolStripMenuItem.Text = "&Contents";
            this.contentsToolStripMenuItem.Visible = false;
            // 
            // indexToolStripMenuItem
            // 
            this.indexToolStripMenuItem.Name = "indexToolStripMenuItem";
            this.indexToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.indexToolStripMenuItem.Text = "&Index";
            this.indexToolStripMenuItem.Visible = false;
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.searchToolStripMenuItem.Text = "&Search";
            this.searchToolStripMenuItem.Visible = false;
            // 
            // searchForUpdateToolStripMenuItem
            // 
            this.searchForUpdateToolStripMenuItem.Name = "searchForUpdateToolStripMenuItem";
            this.searchForUpdateToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.searchForUpdateToolStripMenuItem.Text = "Search for Update";
            this.searchForUpdateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(165, 6);
            this.toolStripSeparator5.Visible = false;
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.packStatusLabel,
            this.packActionProgressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 785);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1769, 22);
            this.statusStrip.TabIndex = 11;
            this.statusStrip.Text = "statusStrip1";
            // 
            // packStatusLabel
            // 
            this.packStatusLabel.Name = "packStatusLabel";
            this.packStatusLabel.Size = new System.Drawing.Size(1632, 17);
            this.packStatusLabel.Spring = true;
            this.packStatusLabel.Text = "Use the File menu to create a new pack file or open an existing one.";
            this.packStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // packActionProgressBar
            // 
            this.packActionProgressBar.Name = "packActionProgressBar";
            this.packActionProgressBar.Size = new System.Drawing.Size(120, 16);
            // 
            // PackFileManagerForm
            // 
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(1769, 807);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(192, 114);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "PackFileManagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pack File Manager";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PackFileManagerForm_FormClosing);
            this.packActionMenuStrip.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem indexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip packActionMenuStrip;
        private System.Windows.Forms.ToolStripProgressBar packActionProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel packStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator shaderItemSeparator;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchForUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filesMenu;
        private System.Windows.Forms.ToolStripMenuItem changePackTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setShaderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFromPackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDecodeToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openExternalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openAsTextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameMultiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportUnknownToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem searchFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateDBFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFileListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createReadMeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extrasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cAPacksAreReadOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateOnStartupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subscribeToBetaToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem contextAddMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextAddFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextAddDirMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextDeleteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextRenameMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem contextOpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextOpenExternalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextOpenDecodeToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextOpenTextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextExtractMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextExtractSelectedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextExtractAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractUnknownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emptyDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextAddEmptyDirMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextImportTsvMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDecodeToolOnErrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem extractAllTsv;
        private System.Windows.Forms.ToolStripMenuItem modsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newModMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem editModMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installModMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallModMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractTSVFileExtensionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem csvToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsvToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem postProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem loadGamePacksToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem openGameDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDataDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openEncyclopediaDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripMenuItem openUserDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openReplaysDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openScriptsDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openModPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextAddFromPackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentFilesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandToolStripMenuItem;
        private System.Windows.Forms.ToolTip packTreeViewToolTip;
        private PackedTreeView.PackedTreeView _packTreeView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem dBDecoderToolStripMenuItem;
    }
}