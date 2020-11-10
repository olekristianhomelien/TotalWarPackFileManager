using System;
using System.IO;
using Common;
using System.Diagnostics;
using System.Windows.Forms;
using Filetypes;

namespace PackFileManager {
    /*
     * An editor implementation that Opens a selected file in an external editor.
     * This is done by exporting the file to a temporary directory, then
     * invoking the Shell's "Open As" dialog and then waits for the started 
     * external process to finish to re-import the file.
     */
    public class ExternalEditor : IPackedFileEditor {
        private FileSystemWatcher openFileWatcher;
        string openFilePath;
        public Process ExternalProcess {
            get; set;
        }
        public bool CanEdit(PackedFile file) {
            return file != null && ExternalProcess == null;
        }
        bool modified = false;
        public bool Modified {
            get { 
                return modified; 
            }
            set { 
                modified = value; 
            }
        }
        public bool ReadOnly {
            get; set;
        }
        PackedFile packedFile;
        public PackedFile CurrentPackedFile {
            get { 
                return packedFile;
            }
            set {
                if (ExternalProcess != null) {
                    throw new InvalidOperationException("External editor already open.");
                }
                if (ReadOnly) {
                    if (MessageBox.Show("The current pack file is read-only.\n" +
                                        "You can open it, but changes you make will not get applied.\n" +
                                        "Continue?",
                                        "Opening read-only file in external editor.",
                                        MessageBoxButtons.OKCancel) == DialogResult.Cancel) {
                        return;
                    }
                }
                Modified = false;
                packedFile = value;
                openFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(packedFile.FullPath));
                File.WriteAllBytes(openFilePath, packedFile.Data);
                ProcessStartInfo startInfo = new ProcessStartInfo(openFilePath, "openas") {
                    ErrorDialog = true
                };
                ExternalProcess = Process.Start(startInfo);
                ExternalProcess.Exited += delegate(object sender, EventArgs e) {
                    Commit();
                    ExternalProcess = null;
                };
                
                openFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(openFilePath), Path.GetFileName(openFilePath));
                openFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                openFileWatcher.Changed += SetModifiedOnFileChange;
                openFileWatcher.EnableRaisingEvents = true;
            }
        }
        public void Commit() {
            if (Modified) {
                if (MessageBox.Show ("Changes were made to the extracted file. "+
                                     "Do you want to replace the packed file with the extracted file?", "Save changes?", 
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    
                    // replace data in pack
                    packedFile.Data = File.ReadAllBytes(openFilePath);
                    
                    // delete temporary file
                    while (File.Exists(openFilePath)) {
                        try {
                            File.Delete(openFilePath);
                        } catch (IOException) {
                            if (MessageBox.Show ("Unable to delete the temporary file; is it still in use by the external editor?" + 
                                                 "\r\n\r\nClick Retry to try deleting it again or Cancel to leave it in the temporary directory.", 
                                                 "Temporary file in use", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel) {
                                break;
                            }
                        }
                    }
                    Modified = false;
                }
            }
        }
        public void SetModifiedOnFileChange(object sender, FileSystemEventArgs args) {
            Modified = true;
        }

        public void Dispose()
        {
        }
    }
}

