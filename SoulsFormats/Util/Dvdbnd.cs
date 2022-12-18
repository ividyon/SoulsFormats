using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoulsFormats.Util
{
    /// <summary>
    /// A pair of files, consisting of one .bhd and one .bdt
    /// </summary>
    public class Dvdbnd
    {
        /// <summary>
        /// The bhd, containing metadata, offsets, and encryption keys
        /// </summary>
        public BHD5 Bhd { get; set; }
        /// <summary>
        /// The bdt, containing the encrypted files
        /// </summary>
        public FileStream Bdt { get; set; }

        /// <summary>
        /// Creates a new Dvdbnd
        /// </summary>
        public Dvdbnd(BHD5 bhd, FileStream bdt)
        {
            Bhd = bhd;
            Bdt = bdt;
        }

        /// <summary>
        /// Returns a list of all known file paths contained in this dvdbnd
        /// </summary>
        public IEnumerable<string> AllFilePaths =>
            Bhd.fileHeaders.Keys;

        /// <summary>
        /// Determines whether a file is contained by this dvdbnd given its path 
        /// as it would appear in the .bhd.
        /// </summary>
        public bool FileExists(string fileName) => 
            Bhd.fileHeaders.ContainsKey(fileName);

        /// <summary>
        /// Returns a list containing all file names in this dvdbnd that match
        /// the given regex
        /// </summary>
        public List<string> FindFiles(string regex) => 
            Bhd.fileHeaders.Keys
                .Where(k => Regex.IsMatch(k, regex))
                .ToList();

        /// <summary>
        /// Returns a list of all files in this dvdbnd whose file names match
        /// the given regex and are valid files for the specified file type
        /// </summary>
        public List<SoulsFile<T>> ReadFiles<T>(string regex) where T : SoulsFile<T>, new() =>
            FindFiles(regex)
                .Select(ReadFile<T>)
                .Where(f => f != null)
                .Select(f => f!)
                .ToList();

        /// <summary>
        /// Gets the binary contents of a file given its path as it would appear
        /// in the .bhd. Returns null if the file cannot be found
        /// </summary>
        public byte[]? ReadFile(string fileName)
        {
            if (Bhd.fileHeaders.TryGetValue(fileName, out var header)) {
                return header.ReadFile(Bdt);
            } else return null;
        }

        /// <summary>
        /// Attempts to read and parse a SoulsFile given its path as it would appear
        /// in the .bhd. Returns null if the file cannot be found or is not of the
        /// specified type.
        /// </summary>
        public SoulsFile<T>? ReadFile<T>(string fileName) where T : SoulsFile<T>, new()
        {
            var data = ReadFile(fileName);
            if (data == null) return null;
            SoulsFile<T>.IsRead(data, out var file);
            return file;
        }
    }
}
