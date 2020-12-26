using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Common {

    /*
     * Reads and writes Pack files from and to filesystem files.
     * I guess we could generalize to streams, but not much point to that for now.
     */
    public class PackFileCodec {
        public delegate void HeaderLoadedEvent(PFHeader header);
        public delegate void PackedFileLoadedEvent(PackedFile packed);
        public delegate void PackFileLoadedEvent(PackFile pack);

        public event HeaderLoadedEvent HeaderLoaded;
        public event PackedFileLoadedEvent PackedFileLoaded;
        public event PackFileLoadedEvent PackFileLoaded;
		
        /*
         * Decode pack file at the given path.
         */
        public PackFile Open(string packFullPath) {
			PackFile file;
			long sizes = 0;
			using (var reader = new BinaryReader(new FileStream(packFullPath, FileMode.Open), Encoding.ASCII)) 
            {
				PFHeader header = ReadHeader (reader);
				file = new PackFile (packFullPath, header);
				OnHeaderLoaded (header);

				long offset = file.Header.DataStart;
				for (int i = 0; i < file.Header.FileCount; i++) 
                {
					uint size = reader.ReadUInt32 ();
					sizes += size;
                    if (file.Header.HasAdditionalInfo)
                    {
                        header.AdditionalInfo = reader.ReadUInt32();
                    }

                    if (file.Header.PackIdentifier == "PFH5")
                        reader.ReadByte();

                    string packedFileName = IOFunctions.TheadUnsafeReadZeroTerminatedAscii(reader);
                    // this is easier because we can use the Path methods
                    // under both Windows and Unix
                    packedFileName = packedFileName.Replace('\\', Path.DirectorySeparatorChar);

					PackedFile packed = new PackedFile (file.Filepath, packedFileName, offset, size);
                    file.Add (packed);
                    packed.IsAdded = false; // Reset the flag as the file is not really added as a new file, it is loaded for the first time
                    offset += size;
					OnPackedFileLoaded (packed);
				}
			}
			OnFinishedLoading (file);
			file.IsModified = false;
			return file;
		}
        /*
         * Reads pack header from the given file.
         */
        public static PFHeader ReadHeader(string filename) {
            using (var reader = new BinaryReader(File.OpenRead(filename))) {
                return ReadHeader(reader);
            }
        }
        /*
         * Reads pack header from the given reader.
         */
		public static PFHeader ReadHeader(BinaryReader reader) {
			PFHeader header;
			string packIdentifier = new string (reader.ReadChars (4));
			header = new PFHeader (packIdentifier);
			int packType = reader.ReadInt32 ();
            header.PrecedenceByte = (byte) packType;
            // header.Type = (PackType)packType;
			header.Version = reader.ReadInt32 ();
			int replacedPackFilenameLength = reader.ReadInt32 ();
			reader.BaseStream.Seek (0x10L, SeekOrigin.Begin);
			header.FileCount = reader.ReadUInt32 ();
			UInt32 indexSize = reader.ReadUInt32 ();
			header.DataStart = header.Length + indexSize;
            
            if (header.PackIdentifier == "PFH4" || header.PackIdentifier == "PFH5") {
                header.Unknown = reader.ReadUInt32();
            }

			// go to correct position
			reader.BaseStream.Seek (header.Length, SeekOrigin.Begin);
            for (int i = 0; i < header.Version; i++) {
                header.ReplacedPackFileNames.Add(IOFunctions.TheadUnsafeReadZeroTerminatedAscii(reader));
            }
            header.DataStart += replacedPackFilenameLength;
			return header;
		}
        /*
         * Encodes given pack file to given path.
         */
        public void WriteToFile(string FullPath, PackFile packFile) 
        {
            throw new Exception("Saving is disbaled for now");

            using (BinaryWriter writer = new BinaryWriter(new FileStream(FullPath, FileMode.Create), Encoding.ASCII)) {
                writer.Write (packFile.Header.PackIdentifier.ToCharArray ());
                writer.Write ((int)packFile.Header.PrecedenceByte);
                writer.Write ((int)packFile.Header.Version);
                writer.Write (packFile.Header.ReplacedFileNamesLength);
                UInt32 indexSize = 0;
                List<PackedFile> toWrite = new List<PackedFile> ((int)packFile.Header.FileCount);
                foreach (PackedFile file in packFile.Files) {
                    if (!file.Deleted) {
                        indexSize += (uint)file.FullPath.Length + 5;
                        if (packFile.Header.PackIdentifier == "PFH5")
                        {
                            indexSize += 1;
                        }
                        if (packFile.Header.HasAdditionalInfo) {
                            indexSize += 4;
                        }
                        toWrite.Add (file);
                    }
                }
                writer.Write (toWrite.Count);
                writer.Write (indexSize);

                // File Time
                if (packFile.Header.PackIdentifier == "PFH2" || packFile.Header.PackIdentifier == "PFH3") {
                    Int64 fileTime = DateTime.Now.ToFileTimeUtc ();
                    writer.Write (fileTime);
                } else if (packFile.Header.PackIdentifier == "PFH4" || packFile.Header.PackIdentifier == "PFH5") {
                    // hmmm
                    writer.Write(packFile.Header.Unknown);
                }

                // Write File Names stored from opening the file
                foreach (string replacedPack in packFile.Header.ReplacedPackFileNames) {
                    writer.Write (replacedPack.ToCharArray ());
                    writer.Write ((byte)0);
                }

                // pack entries are stored alphabetically in pack files
                toWrite.Sort (new PackedFileNameComparer ());

                // write file list
                string separatorString = "" + Path.DirectorySeparatorChar;
                foreach (PackedFile file in toWrite) {
                    writer.Write ((int)file.Size);
                    if (packFile.Header.HasAdditionalInfo) {
                        writer.Write(packFile.Header.AdditionalInfo);
                    }
                    // pack pathes use backslash, we replaced when reading
                    string packPath = file.FullPath.Replace (separatorString, "\\");
                    if (packFile.Header.PackIdentifier == "PFH5")
                    {
                        writer.Write((byte)0);
                    }
                    writer.Write (packPath.ToCharArray ());
                    writer.Write ('\0');
                }
                foreach (PackedFile file in toWrite) {
                    if (file.Size > 0) {
                        byte[] bytes = file.Data;
                        writer.Write (bytes);
                    }
                }
            }
        }
        
        /*
         * Save the given pack file to its current path.
         * Because some of its entries might still be drawing their data from the original pack,
         * we cannot just write over it.
         * Create a temp file, write into that, then delete the original and move the temp.
         */
        public void Save(PackFile packFile) {
            string tempFile = Path.GetTempFileName();
            WriteToFile(tempFile, packFile);
            if (File.Exists(packFile.Filepath)) {
                File.Delete(packFile.Filepath);
            }
            File.Move(tempFile, packFile.Filepath);
        }
        /*
         * Notify pack header having been decoded.
         */
        private void OnHeaderLoaded(PFHeader header) {
            if (this.HeaderLoaded != null) {
                this.HeaderLoaded(header);
            }
        }
        /*
         * Notify pack fully decoded.
         */
        private void OnFinishedLoading(PackFile pack) {
            if (this.PackFileLoaded != null) {
                this.PackFileLoaded(pack);
            }
        }
        /*
         * Notify single pack file having been loaded.
         */
        private void OnPackedFileLoaded(PackedFile packed) {
            if (this.PackedFileLoaded != null) {
                this.PackedFileLoaded(packed);
            }
        }
    }

    /*
     * Compares two PackedFiles by name.
     */
    class PackedFileNameComparer : IComparer<PackedFile> {
        public int Compare(PackedFile a, PackedFile b) {
            return a.FullPath.CompareTo(b.FullPath);
        }
    }
}
