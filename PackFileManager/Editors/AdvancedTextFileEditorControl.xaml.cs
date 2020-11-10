using Common;
using Filetypes;
using Filetypes.Codecs;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PackFileManager.Editors
{
    /// <summary>
    /// Interaction logic for AdvancedTextFileEditorControl.xaml
    /// </summary>
    public partial class AdvancedTextFileEditorControl : UserControl, IPackedFileEditor
    {
        static string[] DEFAULT_EXTENSIONS = { ".txt", ".lua", ".csv", ".fx", ".fx_fragment",
                ".h", ".battle_script", ".xml", ".tai", ".xml.rigging", ".placement", ".hlsl"
            };
        static readonly string EXTENSION_FILENAME = "text_extensions.txt";
        List<string> textExtensions = new List<string>();

        bool _isReadOnly = false;
        PackedFile _packedFile;
        bool _dataChanged = false;

        public AdvancedTextFileEditorControl()
        {


            InitializeComponent();
            this.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            SearchPanel.Install(textEditor);

            textEditor.TextChanged += (b, e) => _dataChanged = true;
            textEditor.ShowLineNumbers = true;

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
            foldingUpdateTimer.Start();

            try
            {
                string extensionFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), EXTENSION_FILENAME);
                textExtensions.AddRange(File.ReadAllLines(extensionFilePath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (textExtensions.Count == 0)
            {
                textExtensions.AddRange(DEFAULT_EXTENSIONS);
            }
        }

        public PackedFile CurrentPackedFile { get { return _packedFile; } set { SetCurrentPackFile(value); } }

        public bool ReadOnly { get { return _isReadOnly; } set { SetReadOnly(value); } }

        public bool DataChanged { get; set; }

        public bool CanEdit(PackedFile file)
        {
            return PackedFileEditorHelper.HasExtension(file, DEFAULT_EXTENSIONS); ;
        }

        void SetReadOnly(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
            textEditor.IsReadOnly = isReadOnly;
            saveButton.IsEnabled = !isReadOnly;
        }

        void SetCurrentPackFile(PackedFile packedFile)
        {
            if (_packedFile != null && _dataChanged)
                Commit();

            _packedFile = packedFile;
            if (packedFile != null)
            {
                byte[] data = packedFile.Data;
                using (MemoryStream stream = new MemoryStream(data, 0, data.Length))
                {
                    var codec = new TextCodec();
                    var decodedData = codec.Decode(stream);
                    textEditor.Text = decodedData;
                    var extention = Path.GetExtension(_packedFile.Name);

                    

                    textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(extention);
                    HighlightingComboBox_SelectionChanged(null, null);
                }
            }
        }

        public void Commit()
        {
            if (DataChanged && !ReadOnly)
            {
                SetData();
                DataChanged = false;
            }
        }

        protected virtual void SetData()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var codec = new TextCodec();
                codec.Encode(stream, textEditor.Text);
                CurrentPackedFile.Data = stream.ToArray();
            }
        }

        void saveFileClick(object sender, EventArgs e)
        {
            Commit();
        }

        #region Folding
        FoldingManager foldingManager;
        object foldingStrategy;

        void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textEditor.SyntaxHighlighting == null)
            {
                foldingStrategy = null;
            }
            else
            {
                switch (textEditor.SyntaxHighlighting.Name)
                {
                    case "XML":
                        foldingStrategy = new XmlFoldingStrategy();
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        break;
                    default:
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        foldingStrategy = null;
                        break;
                }
            }
            if (foldingStrategy != null)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(textEditor.TextArea);
                UpdateFoldings();
            }
            else
            {
                if (foldingManager != null)
                {
                    FoldingManager.Uninstall(foldingManager);
                    foldingManager = null;
                }
            }
        }

        void UpdateFoldings()
        {
            if (foldingStrategy is XmlFoldingStrategy)
            {
                ((XmlFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
            }
        }

        public void Dispose()
        {
        }


        #endregion
    }
}
