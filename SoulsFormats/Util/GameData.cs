using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Util
{
    /// <summary>
    /// A helper class that provides a convenient interface for loading the entirety of
    /// a game's data
    /// </summary>
    public class GameData
    {
        /// <summary>
        /// Represents which Souls game is loaded
        /// </summary>
        public enum GameVersion
        {
            /// <summary>
            /// Elden Ring
            /// </summary>
            EldenRing
        }

        /// <summary>
        /// The game this data is for
        /// </summary>
        public GameVersion Game { get; set; }
        /// <summary>
        /// The path to the game folder containing the data
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// The dvdbnds (.bhd and .bdt pair) that make up the game data
        /// </summary>
        public Dictionary<string, Dvdbnd> Dvdbnds { get; set; }

        /// <summary>
        /// The regulation file that contains game params
        /// </summary>
        public BND4? Regulation { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Creates a new GameData for a specific game and loads the data
        /// </summary>
        public GameData(string folderPath, GameVersion game)
        {
            Game = game;
            FolderPath = folderPath;
            LoadFiles();
        }

        /// <summary>
        /// Creates a new GameData, attempting to automatically detect the game version,
        /// and loads the data
        /// </summary>
        public GameData(string folderPath)
        {
            FolderPath = folderPath;
            if (File.Exists(Path.Combine(folderPath, "eldenring.exe"))) {
                Game = GameVersion.EldenRing;
            }
            LoadFiles();
        }
#pragma warning restore CS8618

        private void LoadFiles()
        {
            Dvdbnds = new Dictionary<string, Dvdbnd>();
            switch (Game) {
                case GameVersion.EldenRing:
                    LoadDvdbnd(Path.Combine(FolderPath, "data0.bhd"));
                    LoadDvdbnd(Path.Combine(FolderPath, "data1.bhd"));
                    LoadDvdbnd(Path.Combine(FolderPath, "data2.bhd"));
                    LoadDvdbnd(Path.Combine(FolderPath, "data3.bhd"));
                    LoadDvdbnd(Path.Combine(FolderPath, @"sd\sd.bhd"));
                    break;
            }
        }

        private void LoadDvdbnd(string pathToBhd)
        {
            string archiveName = Path.GetFileNameWithoutExtension(pathToBhd).ToLower();
            //replace "hd" with "dt"
            string bdtPath = pathToBhd[..(pathToBhd.Length - 2)] + "dt";
            var bhdGame = Game switch {
                GameVersion.EldenRing => BHD5.Game.EldenRing,
                _ => throw new NotImplementedException($"Game {Game} unrecognized")
            };
            BHD5 bhd;
            if (File.Exists(pathToBhd + ".decrypted")) {
                using var fs = File.OpenRead(pathToBhd + ".decrypted");
                bhd = BHD5.Read(fs, bhdGame, archiveName);
            } else {
                bhd = BHD5.Read(pathToBhd, bhdGame);
            }
            var bdt = File.OpenRead(bdtPath);
            Dvdbnds[archiveName] = new Dvdbnd(bhd, bdt);
        }

        /// <summary>
        /// Returns a list containing all file names in this game that match
        /// the given regex
        /// </summary>
        /// <returns>A list of tuples containing the archive name and the file path</returns>
        public List<(string, string)> FindFiles(string regex) =>
            Dvdbnds.AsEnumerable()
                .SelectMany(d => d.Value
                    .FindFiles(regex)
                    .Select(f => (d.Key, f)))
                .ToList();

        /// <summary>
        /// Returns a list of all files in this game whose file names match
        /// the given regex and are valid files for the specified file type
        /// </summary>
        public List<SoulsFile<T>> ReadFiles<T>(string regex) where T : SoulsFile<T>, new() =>
            FindFiles(regex)
                .Select(t => Dvdbnds[t.Item1].ReadFile<T>(t.Item2))
                .Where(f => f != null)
                .Select(f => f!)
                .ToList();
    }

    /// <summary>
    /// A collection of all game data for Elden Ring
    /// </summary>
    public class EldenRingData : GameData
    {

        /// <summary>
        /// The data0.bhd and data0.bdt
        /// </summary>
        public Dvdbnd Data0
        {
            get { return Dvdbnds["data0"]; }
            set { Dvdbnds["data0"] = value; }
        }
        /// <summary>
        /// The data1.bhd and data1.bdt
        /// </summary>
        public Dvdbnd Data1
        {
            get { return Dvdbnds["data1"]; }
            set { Dvdbnds["data1"] = value; }
        }
        /// <summary>
        /// The data2.bhd and data2.bdt
        /// </summary>
        public Dvdbnd Data2
        {
            get { return Dvdbnds["data2"]; }
            set { Dvdbnds["data2"] = value; }
        }
        /// <summary>
        /// The data3.bhd and data3.bdt
        /// </summary>
        public Dvdbnd Data3
        {
            get { return Dvdbnds["data3"]; }
            set { Dvdbnds["data3"] = value; }
        }
        /// <summary>
        /// The sd.bhd and sd.bdt
        /// </summary>
        public Dvdbnd Sd
        {
            get { return Dvdbnds["sd"]; }
            set { Dvdbnds["sd"] = value; }
        }

        /// <summary>
        /// Creates a new instance and loads the data
        /// </summary>
        public EldenRingData(string folderPath) : base(folderPath, GameVersion.EldenRing) { }
    }
}
