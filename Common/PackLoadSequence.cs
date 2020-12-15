using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Common {

    /*
     * Class sorting the packs in a directory in the order they are loaded.
     */
    public class PackLoadSequence 
    {
        ILogger _logger = Logging.Create<PackLoadSequence>();

        /*
         * Predicate function that can decide whether or not to ignore a given
         * pack file altogether.
         */
        private Predicate<string> ignorePack;
        public Predicate<string> IgnorePack {
            get { 
                return ignorePack ?? Keep;
            }
            set {
                ignorePack = value ?? Keep;
            }
        }
        
        /*
         * Predicate function that can decide whether or not a pack
         * will be included in the result depending on a specific file it contains.
         * Default is including all packs containing db files.
         */
        private Predicate<string> keepPacksContaining;
        public Predicate<string> IncludePacksContaining {
            get {
                return keepPacksContaining ?? KeepDbFiles;
            }
            set {
                keepPacksContaining = value ?? KeepDbFiles;
            }
        }
        
        /*
         * Retrieve the packs loaded from the given directory.
         */
        public List<string> GetPacksLoadedFrom(string directory) 
        {
            _logger.Here().Information($"Loading packs from {directory}");

            List<string> result = new List<string>();
            if (directory == null) {
                return result;
            }
            directory = Path.Combine(directory, "data");
            List<string> paths = new List<string>();
            if (Directory.Exists(directory)) {
                // remove obsoleted packs
                List<string> obsoleted = new List<string>();
                foreach (string filename in Directory.EnumerateFiles(directory, "*.pack")) {
                    if (!IgnorePack(filename)) {
                        paths.Add(filename);
                    }
                }
#if DEBUG
                Console.WriteLine("obsoleted: {0}", string.Join(",", obsoleted));
                Console.WriteLine("from files in {1}: {0}", string.Join(",", Directory.EnumerateFiles(directory, "*.pack")), directory);
#endif
                paths.RemoveAll(delegate(string pack) {
                    return obsoleted.Contains(pack);
                });
                paths.ForEach(p => {
#if DEBUG
                    DateTime start = DateTime.Now;
#endif
                    try {
                        // more efficient to use the enumerable class instead of instantiating an actual PackFile
                        // prevents having to parse all contained file names
                        PackedFileEnumerable packedFiles = new PackedFileEnumerable(p);
                        foreach (PackedFile file in packedFiles) {
                            if (IncludePacksContaining(file.FullPath)) {
                                result.Add(p);
                                break;
                            }
                        }
                    } catch {
                        // invalid pack file probably... ignore it
                    }
#if DEBUG
                    Console.WriteLine("{0} for {1}", DateTime.Now.Subtract(start), Path.GetFileName(p));
#endif
                });
                try {
                    result.Sort(new PackLoadOrder(result));
                } catch {
                    // should do something here...
                }
            } else {
                throw new FileNotFoundException(string.Format("Game directory not found: {0}", directory));
            }
            return result;
        }
        
        #region Pack filtering
        // Don't ignore any packs.
        static bool Keep(string f) { return false; }
        static bool KeepDbFiles(string f) { return f.StartsWith("db"); }
        static readonly string[] EXCLUDE_PREFIXES = { 
                                                        "local", "models", "sound", "terrain", 
                                                        "anim", "ui", "voices" };
        /*
         * Query if a pack file is provided by CA.
         */
        public static bool IsDbCaPack(string filename) {
            foreach (string exclude in EXCLUDE_PREFIXES) {
                if (Path.GetFileName(filename).StartsWith(exclude)) {
                    return true;
                }
            }
            bool result = false;
            try {
                PFHeader pack = PackFileCodec.ReadHeader(filename);
                result = (pack.Type != PackType.Patch) && (pack.Type != PackType.Release);
            } catch {} // not a valid pack: not a CA pack
            return result;
        }
        #endregion
    }
    /*
     * Sorts pack files in the order they are loaded in
     * (roughly Boot->Release->Patch->Mod->Movie).
     * Amongst packs of the same type, they are ordered by checking the obsoletion path
     * (packs may contain a list of packs they override).
     */
    public class PackLoadOrder : Comparer<string> {
        /*private static List<PackType> Ordered = new List<PackType>(new PackType[] {
            PackType.Boot, PackType.BootX, PackType.Shader1, PackType.Shader2,
            PackType.Release, PackType.Patch,
            PackType.Mod, PackType.Movie, 
            PackType.Music, PackType.Music1, 
            PackType.Sound, PackType.Sound1
        });*/

        Dictionary<string, PFHeader> nameToHeader = new Dictionary<string, PFHeader>();
        Dictionary<string, string> nameToPath = new Dictionary<string, string>();
        public PackLoadOrder(ICollection<string> files) {
            foreach (string file in files) {
                try {
                    PFHeader header = PackFileCodec.ReadHeader(file);
                    nameToHeader.Add(file, header);
                    nameToPath.Add(Path.GetFileName(file), file);
                } catch { } // couldn't read header probably; just ignore the file
            }
        }
        /*
         * Compare the two strings by interpreting them as pack file paths and
         * establishing the load order between those.
         */
        public override int Compare(string p1, string p2) {
            // sort by type
            PFHeader p1Header = nameToHeader[p1];
            PFHeader p2Header = nameToHeader[p2];
            int index1 = p1Header.LoadOrder; // Ordered.IndexOf(p1Header.Type);
            int index2 = p2Header.LoadOrder; // Ordered.IndexOf(p2Header.Type);
            int result = index2 - index1;
            // same type? Check obsoletion path
            if (result == 0) {
                if (Obsoletes(p1, p2)) {
                    result = -1;
                } else if (Obsoletes(p2, p1)) {
                    result = 1;
                }
            }
            return result;
        }
        /*
         * Query if p2 obsoletes p1.
         * Will return true if p1 is contained in any obsoleted file list in a pack obsoleted by p2, 
         * otherwise false.
         */
        bool Obsoletes(string p1, string p2) {
            PFHeader p1Header = nameToHeader[p1];
            if (p1Header.ReplacedPackFileNames.Count == 0) {
                return false;
            }
            bool result = p1Header.ReplacedPackFileNames.Contains(Path.GetFileName(p2));
            if (!result) {
                foreach (string name in p1Header.ReplacedPackFileNames) {
                    string otherCandidate;
                    if (nameToPath.TryGetValue(name, out otherCandidate)) {
                        result = Obsoletes(otherCandidate, p2);
                    }
                    if (result) {
                        break;
                    }
                }
            }
            return result;
        }
    }

    public static class PackFileLoadHelper
    {
        public static List<PackFile> LoadCaPackFilesForGame(Game game)
        {
            var output = new List<PackFile>();
            PackLoadSequence allFiles = new PackLoadSequence
            {
                IncludePacksContaining = delegate (string s) { return true; }
            };

            List<string> packPaths = allFiles.GetPacksLoadedFrom(game.GameDirectory);
            packPaths.Reverse();
            PackFile file = new PackFile("All Packs");
            PackFileCodec codec = new PackFileCodec();
            foreach (string path in packPaths)
            {
                PackFile pack = codec.Open(path);
                pack.Files.ForEach(f =>
                {
                    file.Add(f, true);
                    f.IsAdded = false;
                });
            }

            return new List<PackFile>() { file};
        }

        public static PackedFile FindFile(List<PackFile> loadedPackFiles, string filename)
        {
            filename = filename.ToLower();
            filename = filename.Replace(@"/", @"\");

            foreach (var directory in loadedPackFiles)
            {
                var f = directory[filename];
                if (f != null)
                    return f as PackedFile;
            }
            return null;
        }

        public static List<PackedFile> GetAllFilesInDirectory(List<PackFile> loadedPackFiles, string sourceDirectory)
        {
            var output = new List<PackedFile>();
            sourceDirectory = sourceDirectory.ToLower();
            sourceDirectory = sourceDirectory.Replace(@"/", @"\");

            foreach (var packedFile in loadedPackFiles)
            {
                foreach (var file in packedFile.Files)
                {
                    var idx = file.FullPath.IndexOf(sourceDirectory);
                    if (idx == 0)
                        output.Add(file);
                }
            }
            return output;
        }

        public static List<PackedFile> GetAllWithExtention(List<PackFile> loadedPackFiles, string extention)
        {
            var res = loadedPackFiles.SelectMany(x => x.Files.Where(file => file.FileExtention == extention)).ToList();
            return res;
        }
    }
}

