using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Instrumentation;
using Microsoft.Win32;

namespace Common {

    public enum GameTypeEnum
    {
        Unknown = -1,
        Arena = 0,
        Attila,
        Empire,
        Napoleon,
        Rome_2,
        Shogun_2,
        ThreeKingdoms,
        ThronesOfBritannia,
        Warhammer1,
        Warhammer2,
        Troy
    }

    /*
     * Represents a single Warscape game along with some of its paths and settings.
     * Also keeps a collection of all Warscape games.
     */
    public class Game {
        private static string ROME_INSTALL_DIR = @"C:\Program Files (x86)\Steam\steamapps\common\Total War Rome II";
        public static readonly Game R2TW = new Game(GameTypeEnum.Rome_2, "R2TW", "214950", "Rome 2") {
            DefaultPfhType = "PFH4",
            GameDirectory = Directory.Exists(ROME_INSTALL_DIR) ? ROME_INSTALL_DIR : null,
            UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                   "The Creative Assembly", "Rome2")
        };
        public static readonly Game STW = new Game(GameTypeEnum.Shogun_2,"STW", "34330", "Shogun2");
        public static readonly Game NTW = new Game(GameTypeEnum.Napoleon,"NTW", "34030", "Napoleon");
        public static readonly Game ETW = new Game(GameTypeEnum.Empire, "ETW", "10500", "Empire") {
            ScriptFilename = "user.empire_script.txt"
        };
        public static readonly Game ATW = new Game(GameTypeEnum.Attila, "ATW", "325610", "Attila") {
            DefaultPfhType = "PFH4"
        };
        public static readonly Game TWH = new Game(GameTypeEnum.Warhammer1, "TWH", "459420", "Warhammer")
        {
            DefaultPfhType = "PFH4"
        };
        public static readonly Game TWH2 = new Game(GameTypeEnum.Warhammer2, "TWH2", "594570", "Warhammer2")
        {
            DefaultPfhType = "PFH5"
        };
        public static readonly Game TOB = new Game(GameTypeEnum.ThronesOfBritannia, "ToB", "712100", "ThronesofBritannia")
        {
            DefaultPfhType = "PFH4"
        };

        public static readonly Game TW3K = new Game(GameTypeEnum.ThreeKingdoms,"TW3K", "779340", "ThreeKingdoms")
        {
            DefaultPfhType = "PFH5"
        };

        public static readonly Game Troy = new Game(GameTypeEnum.Troy, "Troy", "779340", "TroySage")
        {
            DefaultPfhType = "PFH5"
        };

        private static readonly Game[] GAMES = new Game[] {TW3K, TOB, TWH2, TWH, ATW, R2TW, STW, NTW, ETW, Troy};

        /*
         * Constructor.
         * <param name="gameId">game name</param>
         * <param name="steam">steam id</param>
         * <param name="gameDir">game pathname below user dir</param>
         * <param name="scriptFile">name of the script file containing mod entries</param>
         */
        public Game(GameTypeEnum gameType, string gameId, string steam, string gameDir, string scriptFile = "user.script.txt") {
            GameType = gameType;
            Id = gameId;
            steamId = steam;
            UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                   "The Creative Assembly", gameDir);
            ScriptFilename = scriptFile;
            DefaultPfhType = "PFH3";

            retrievers = new RetrieveLocation[] {
                    delegate() { return gameDirectory; },
                    //delegate() { return GetInstallLocation(WOW_NODE); },
                    //delegate() { return GetInstallLocation(WIN_NODE); }
                };
        }


        public static Game GetByEnum(GameTypeEnum gameEnum)
        {
            foreach (Game game in GAMES)
            {
                if (game.GameType == gameEnum)
                    return game;
            }
            return null;
        }
        /*
         * Retrieve list of all known games.
         */
        public static List<Game> Games {
            get {
                List<Game> result = new List<Game>();
                foreach (Game g in GAMES) {
                    result.Add(g);
                }
                return result;
            }
        }
        /*
         * Retrieve Game by given ID.
         */
        public static Game ById(string id) {
            foreach(Game game in GAMES)
                if (game.Id.Equals(id))
                    return game;
            return null;
        }
        
        /*
         * Retrieve this game's ID.
         */
        public string Id {
            get; private set;
        }

        public GameTypeEnum GameType { get; private set; }
        /*
         * Retrieve this game's settings directory below the user directory.
         */
        public string UserDir {
            get; private set;
        }
        string gameDirectory;
        private string steamId;
        public static readonly string NOT_INSTALLED = "";
        
        public string DefaultPfhType {
            get; internal set;
        }

        /*
         * Returns the install location of this game or null if it is not installed.
         */
        public string GameDirectory {
            get {
                string dir = null;
                foreach (RetrieveLocation retrieveLocation in retrievers) {
                    dir = retrieveLocation();
                    if (dir != null) {
                        break;
                    }
                }
                return dir;
            }
            set {
                gameDirectory = value;
            }
        }
        
        delegate string RetrieveLocation();
        RetrieveLocation[] retrievers;
  
        /*
         * Retrieve this game's data directory (containing the game's pack files).
         */
        public string DataDirectory {
            get {
                return Path.Combine(GameDirectory, "data");
            }
        }
        /*
         * Retrieve the name of the script file for this game.
         */
        public string ScriptFilename {
            get;
            private set;
        }
        /*
         * Retrieve the path of the directory containing the script file for this game.
         */
        public string ScriptDirectory {
            get {
                return Path.Combine(UserDir, "scripts");
            }
        }
        /*
         * Retrieve the absolute path to the script file for this game.
         */
        public string ScriptFile {
            get {
                return Path.Combine(ScriptDirectory, ScriptFilename);
            }
        }
        /*
         * Query if this game is installed.
         */
        public bool IsInstalled {
            get {
                return Directory.Exists(GameDirectory)
                    && Directory.Exists(DataDirectory);
            }
        }
        /*
         * Retrieve the schema filename for this game.
         */

        // usual installation nodes in the registry (for installation autodetect)
        private static string WOW_NODE = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {0}";
        private static string WIN_NODE = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {0}";

        /*
         * Helper method to retrieve the install location.
         */
        private string GetInstallLocation(string node) {
            string str = null;
            try {
                string regKey = string.Format(WOW_NODE, steamId);
                str = (string) Registry.GetValue(regKey, "InstallLocation", "");
                // check if directory actually exists
                if (!string.IsNullOrEmpty(str) && !Directory.Exists(str)) {
                    str = null;
                }
            } catch {}
            return str;
        }
    }
}

