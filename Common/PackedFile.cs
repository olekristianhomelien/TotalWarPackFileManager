using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Common {
 
    /*
     * Any entry in the Pack file.
     * Has a name and a path designating its full position.
     */
    public abstract class PackEntry : IComparable<PackEntry> {
        // Event triggered when name is about to be changed (called before actual change)
        public delegate void Renamed(PackEntry dir, String newName);
        public event Renamed RenameEvent;

        // Event triggered if any modification occurred on this entry
        // (called before modification is committed)
        public delegate void Modification(PackEntry file);
        public event Modification ModifiedEvent;

        // This Entry's Parent Entry
        public PackEntry Parent { get; set; }

        // Name; calls RenameEvent if changed
        string name;
        public string Name {
            get {
                return name;
            }
            set {
                name = value;
                if (RenameEvent != null) {
                    RenameEvent(this, value);
                }
                IsRenamed = true;
            }
        }

        public bool IsRenamed { get; set; } = false;
        public bool IsAdded { get; set; } = false;
        // Path
        string _fullPathCachedValue;
        public virtual string FullPath 
        {
            get 
            {
                if (_fullPathCachedValue != null)
                    return _fullPathCachedValue;
                string result = Name;
                PackEntry p = Parent;
                while (p != null) {
                    result = p.Name + Path.DirectorySeparatorChar + result;
                    p = p.Parent;
                }
                int index = result.IndexOf(Path.DirectorySeparatorChar);
                if (index != -1) {
                    result = result.Substring(index + 1);
                }
                _fullPathCachedValue = result;
                return result;
            }
        }

        // Tag whether entry is tagged for deletion
        private bool deleted = false;
        public virtual bool Deleted {
            get {
                return deleted;
            }
            set {
                deleted = value;
                Modified = true;
            }
        }

        // Tag whether entry has been modified
        bool modified;
        public bool Modified {
            get { return modified; }
            set {
                if (modified != value) 
                {
                    if(modified == false)
                        IsRenamed = false;
                    modified = value;
                    if (ModifiedEvent != null) {
                        ModifiedEvent(this);
                    }
                    if (Parent != null) {
                        Parent.Modified = value;
                    }
                }
            }
        }

        // Compare by entry names
        public int CompareTo(PackEntry entry) {
            return entry != null ? Name.CompareTo(entry.Name) : 0;
        }

        public override string ToString()
        {
            return FullPath;
        }
    }

    /*
     * A pack entry containing actual data.
     * Data is provided by a DataSource object.
     */
    [DebuggerDisplay("{Name}")]
    public class PackedFile : PackEntry {

        string _fileExtention;
        public string FileExtention { get { return _fileExtention; } }


        string _fileSystemPath;
        DateTime? _editTime = null;
        public DateTime EditTime {
            get
            {
                if(_editTime == null)
                    _editTime = File.GetLastWriteTime(_fileSystemPath);
                return _editTime.Value;
            }
            set { _editTime = value; }
        }

        private static readonly byte[] EMPTY = new byte[0];

        // a PackedFile can exist alone, without its parent
        string fullPath;
        public override string FullPath {
            get {
                if (Parent != null) {
                    return base.FullPath;
                } else if (fullPath != null) {
                    return fullPath;
                } else {
                    return Name;
                }
            }
        }

        #region File Data access
        // retrieve the amount of available data
        public long Size {
            get { return Source.Size; }
        }
        // Retrieve the data from this object's source
        public byte[] Data {
            get {
                return Source == null ? EMPTY : Source.ReadData();
            }
            set {
                Source = new MemorySource(value);
                Modified = true;
                EditTime = DateTime.Now;
            }
        }
        // the data source object itself
        DataSource source;

        public DataSource Source {
            get { return source; }
            set {
                source = value;
                Modified = true;
                EditTime = DateTime.Now;
            }
        }
        #endregion

        #region Constructors
        public PackedFile() { }
        public PackedFile(string filename, bool fileSource = true) {
            _fileSystemPath = filename;
            fullPath = filename;
            Name = Path.GetFileName(filename);
            if (fileSource) {
                Source = new FileSystemSource(filename);
            } else {
                Source = new MemorySource(new byte[0]);
            }
            Modified = false;
 
        }
        public PackedFile(string packFile, string packedName, long offset, long len) {
            _fileSystemPath = packedName;
            fullPath = packedName;
            Name = Path.GetFileName(packedName);
            Source = new PackedFileSource(packFile, offset, len);
            Modified = false;
            _fileExtention = Path.GetExtension(packedName);
            if(!string.IsNullOrEmpty(_fileExtention))
                _fileExtention = _fileExtention.Remove(0, 1);
        }
        #endregion
    }

    /*
     * A pack file entry that can contain other entries.
     * Provides additional events for content changes.
     * Note that entries are usually not removed from this; instead they are tagged
     * with "Deleted" and just not added anymore when the full model is rebuilt
     * the next time around.
     */
    public class VirtualDirectory : PackEntry, IEnumerable<PackedFile> {
        public delegate void ContentsEvent(PackEntry entry);
        // triggered when content is added
        public event ContentsEvent DirectoryAdded;
        public event ContentsEvent FileAdded;
        // triggered when file is removed
        public event ContentsEvent FileRemoved;

        public VirtualDirectory()
        {
        }

        // override deletion to tag all contained objects as deleted as well
        public override bool Deleted {
            get {
                return base.Deleted;
            }
            set {
                if (Deleted != value) {
                    base.Deleted = value;
                    AllEntries.ForEach(e => e.Deleted = value);
                }
            }
        }

        #region Contained entry access
        // the contained directories
        private Dictionary<string, VirtualDirectory> subdirectories = new Dictionary<string, VirtualDirectory>();
        public Dictionary<string, VirtualDirectory> Subdirectories {
            get {
                return subdirectories;
            }
        }

        // the contained files
        private Dictionary<string, PackedFile> containedFiles = new Dictionary<string, PackedFile>();
        public Dictionary<string, PackedFile> Files {
            get {
                return containedFiles;
            }
        }
  
        // retrieve all files contained in this and all subdirectories
        public List<PackedFile> AllFiles {
            get {
                List<PackedFile> files = new List<PackedFile>();
                foreach(var subDirectory in Subdirectories.Values) 
                    files.AddRange(subDirectory.AllFiles);
                files.AddRange(Files.Values);
                return files;
            }
        }
        public List<PackEntry> AllEntries {
            get {
                List<PackEntry> result = new List<PackEntry>();
                result.Add(this);
                foreach(var directory in Subdirectories.Values) 
                    result.AddRange(directory.AllEntries);
                result.AddRange(Files.Values);
                return result;
            }
        }
  
        // enumerates all files
        public IEnumerator<PackedFile> GetEnumerator() {
            return AllFiles.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        /*
         * Retrieve a list with all contained entries (files and directories).
         */
        public List<PackEntry> Entries {
            get {
                List<PackEntry> entries = new List<PackEntry> ();
                entries.AddRange (containedFiles.Values);
                entries.AddRange (subdirectories.Values);
                return entries;
            }
        }

        /*
         * Retrieve the directory with the given name.
         * Will create and add an empty one if it doesn't already exists.
         */
        public VirtualDirectory GetSubdirectory(string subDir) 
        {
            subdirectories.TryGetValue(subDir, out VirtualDirectory result);
            if (result == null) 
            {
                result = new VirtualDirectory { Parent = this, Name = subDir, IsRenamed = false };
                Add (result);
            }
            return result;
        }

        /*
         * Retrieve the contained entry with the given name.
         * Will return null if entry does not exist.
         */
        public PackEntry GetEntry(string name) {
            PackEntry result = null;
            foreach(PackEntry entry in Entries) {
                if (entry.Name.Equals (name)) {
                    result = entry;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region Add entries
        /*
         * Add the given directory to this.
         * Will notify the DirectoryAdded event after adding.
         */
        public void Add(VirtualDirectory dir) 
        {
            subdirectories.Add(dir.Name, dir);
            dir.Parent = this;
            if (DirectoryAdded != null) {
                DirectoryAdded(dir);
            }
        }
        /*
         * Add the given content file.
         * Will notify the FileAdded event after adding.
         * If a file with the given name already exists and is deleted,
         * it is replaced with the given one.
         * If a file with the given name already exists and is not deleted,
         * it will not be replaced unless the "overwrite" parameter is set to true.
         */
        public void Add(PackedFile file, bool overwrite = false) 
        {
            if (containedFiles.ContainsKey(file.Name)) 
            {
                PackedFile contained = null;
                if (containedFiles.ContainsKey(file.Name))
                {
                    contained = containedFiles[file.Name];
                }
                /*foreach (var f in containedFiles) 
                {
                    if (f.Value.Name == file.Name) 
                    {
                        contained = f.Value;
                        break;
                    }
                }*/
                if (contained.Deleted || overwrite) 
                {
                    containedFiles.Remove (file.Name);
                    FileRemoved?.Invoke(contained);
                } 
                else 
                {
                    // don't add the file
                    return;
                }
            }
            containedFiles.Add(file.Name, file);
            file.IsAdded = true;
            file.Parent = this;
            FileAdded?.Invoke(file);
        }

        /*
         * Adds all file from the given directory path.
         */
        public void Add(string basePath) {
            string[] files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
            foreach (string filepath in files) {
                string relativePath = filepath.Replace(Path.GetDirectoryName(basePath), "");
                Add(relativePath, new PackedFile(filepath));
            }
        }
        /*
         * Adds the given file to the given path, relative to this directory.
         */
        public void Add(string relativePath, PackedFile file, bool overwrite = false)
        {
            VirtualDirectory current = this;
            char[] splitAt = { Path.DirectorySeparatorChar };
            var parts = relativePath.Split(splitAt);
            for (int i = 0; i < parts.Count() - 1; i++)
                current = current.GetSubdirectory(parts[i]);

            file.Parent = current;
            current.Add(file, overwrite);
        }
        #endregion
    }
 
    #region Data Sources
    /*
     * A class providing data for a PackedFile content object.
     */
    public abstract class DataSource {
        /*
         * Retrieve the amount of data in this source in bytes.
         * Has an attribute of its own to avoid having to call ReadData().length
         * every time which might be time-consuming.
         */
        public long Size {
            get;
            protected set;
        }
        public abstract byte[] ReadData();
    }

    /* Provides data from the local filesystem */
    [DebuggerDisplay("From file {filepath}")]
    public class FileSystemSource : DataSource {
        protected string filepath;
        public FileSystemSource(string filepath)
            : base() {
            Size = new FileInfo(filepath).Length;
            this.filepath = filepath;
        }
        public override byte[] ReadData() {
            return File.ReadAllBytes(filepath);
        }
    }

    /* Provides data from heap memory */
    [DebuggerDisplay("From Memory")]
    public class MemorySource : DataSource {
        private byte[] data;
        public MemorySource(byte[] data) {
            Size = data.Length;
            this.data = data;
        }
        public override byte[] ReadData() {
            return data;
        }
    }

    /* Provides data from within a pack file */
    [DebuggerDisplay("{Offset}@{filepath}")]
    public class PackedFileSource : DataSource {
        private string filepath;
        public long Offset {
            get;
            private set;
        }
        public PackedFileSource(string packfilePath, long offset, long length) {
            Offset = offset;
            filepath = packfilePath;
            Size = length;
        }
        public override byte[] ReadData() {
            byte[] data = new byte[Size];
            using (Stream stream = File.OpenRead(filepath)) {
                stream.Seek(Offset, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }
            return data;
        }
    }
    #endregion
}
