using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Common;
using Filetypes.Codecs;

namespace Filetypes {
    /*
     * An interface for classes able to edit a file type contained in a pack file.
     */
    public interface IPackedFileEditor : IDisposable
    {
        PackedFile CurrentPackedFile { get; set;}
        
        bool ReadOnly { get;set;}

        bool CanEdit(PackedFile file);

        /* Write back changes after finished editing. */
        void Commit();
    }

    public class PackedFileEditorRegistry
    {
        static List<IPackedFileEditor> editors = new List<IPackedFileEditor>();
        public static List<IPackedFileEditor> Editors
        {
            get
            {
                return editors;
            }
        }
    }
    
    /*
     * An abstract implementation of the IPackedFileEditor interface, able to edit type T.
     */
    public  class PackedFileEditor<T> : UserControl, IPackedFileEditor {
        protected readonly ICodec<T> codec;
        public virtual T EditedFile { get; set; }
        PackedFile currentPacked;
  
        /*
         * True if editor has changed data in the current file.
         */
        protected virtual bool DataChanged {
            get;
            set;
        }
  
        /*
         * Use the given codec to decode files.
         */
        protected PackedFileEditor(ICodec<T> c = null) {
            codec = c;
        }

        public PackedFileEditor() { }

        bool readOnly = true;
        public virtual bool ReadOnly {
            get {
                return readOnly;
            }
            set {
                readOnly = value;
            }
        }
        
        // interface method to give the editor something to edit
        public virtual PackedFile CurrentPackedFile {
            set {
                if (currentPacked != null && DataChanged) {
                    Commit();
                }
                if (value != null) {
                    byte[]data = value.Data;
                    using (MemoryStream stream = new MemoryStream(data, 0, data.Length)) {
                        EditedFile = codec.Decode(stream);
                    }
                } else {
                    EditedFile = default(T);
                }
                DataChanged = false;
                currentPacked = value;
            }
            get {
                return currentPacked;
            }
        }

        // interface to query if given file can be edited
        public virtual bool CanEdit(PackedFile file) { return false; }

        // interface method to save to pack if data has changed in this editor
        public void Commit() {
            if (DataChanged && !ReadOnly) {
                SetData();
                DataChanged = false;
            }
        }

        // implementation method to actually save data
        protected virtual void SetData() {
            using (MemoryStream stream = new MemoryStream()) {
                codec.Encode(stream, EditedFile);
                CurrentPackedFile.Data = stream.ToArray();
            }
        }
        

    }

    public static class PackedFileEditorHelper
    {
        // utility method for tsv export
        public static void WriteToTSVFile(List<string> strings)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = IOFunctions.TSV_FILTER
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(dialog.FileName))
                {
                    foreach (string str in strings)
                    {
                        writer.WriteLine(str);
                    }
                }
            }
        }

        /*
         * Utility method to determine if the given file has one of the given extensions.
         */
        public static bool HasExtension(PackedFile file, IEnumerable<string> extensions)
        {
            bool result = false;
            if (file != null)
            {
                foreach (string ext in extensions)
                {
                    if (Path.GetExtension(file.FullPath).Equals(ext.Trim()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
