using Common;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PackFileManager
{
    public class PackFileManagerSettings
    {
        public class GamePathPair
        {
            public string Game { get; set; }
            public string Path { get; set; }
        }

         public class CustomFileExtentionHighlightsMapping
         {
             public string Extention { get; set; }
             public string HighlightMapping { get; set; }
         }

        public GameTypeEnum CurrentGame { get; set; } = GameTypeEnum.Unknown;
        public string MyModDirectory { get; set; }
        public List<string> RecentUsedFiles { get; set; } = new List<string>();
        public List<GamePathPair> GameDirectories { get; set; } = new List<GamePathPair>();
        public List<CustomFileExtentionHighlightsMapping> CustomFileExtentionHighlightsMappings { get; set; } = new List<CustomFileExtentionHighlightsMapping>();

        public void SaveToLog(ILogger logger)
        {
            logger.Here().Information("PackFileManagerSettings content");

            logger.Here().Information($"CurrentGame:{CurrentGame}");
            logger.Here().Information($"MyModDirectory:{MyModDirectory}");

            foreach(var recentFile in RecentUsedFiles)
                logger.Here().Information($"RecentUsedFiles:{recentFile}");

            foreach (var gamedir in GameDirectories)
                logger.Here().Information($"GameDirectories:{gamedir.Game} - {gamedir.Path}");
        }
    }

    class PackFileManagerSettingService
    {
        public static string SettingsFile
        {
            get
            {
                return Path.Combine(DirectoryHelper.FpmDirectory, "PackFileManagerSettings.txt");
            }
        }

        public static PackFileManagerSettings CurrentSettings { get; set; }



        public static void AddLastUsedFile(string filePath)
        {
            int maxRecentFiles = 5;

            // Remove the file if it is add already
            var index = CurrentSettings.RecentUsedFiles.IndexOf(filePath);
            if (index != -1)
                CurrentSettings.RecentUsedFiles.RemoveAt(index);

            // Add the file
            CurrentSettings.RecentUsedFiles.Insert(0, filePath);

            // Ensure we only have maxRecentFiles in the list
            var currentFileCount = CurrentSettings.RecentUsedFiles.Count;
            if (currentFileCount > maxRecentFiles)
            {
                CurrentSettings.RecentUsedFiles.RemoveRange(maxRecentFiles, currentFileCount - maxRecentFiles);
            }
            Save();
        }

        public static void Save()
        {
            var jsonStr = JsonConvert.SerializeObject(CurrentSettings, Formatting.Indented);
            File.WriteAllText(SettingsFile, jsonStr);
        }

        public static PackFileManagerSettings Load()
        {
            ILogger logger = Logging.Create<PackFileManagerSettingService>();
            logger.Here().Information("Loading settings file");
            if (File.Exists(SettingsFile))
            {
                logger.Here().Information($"Loading existing settings file {SettingsFile}");
                
                var content = File.ReadAllText(SettingsFile);
                CurrentSettings = JsonConvert.DeserializeObject<PackFileManagerSettings>(content);

                CurrentSettings.SaveToLog(logger);
            }
            else
            {
                CurrentSettings = new PackFileManagerSettings();
                logger.Here().Warning("No settings found, creating new");
            }

            return CurrentSettings;
        }
    }
}
