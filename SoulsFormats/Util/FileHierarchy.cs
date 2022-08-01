using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Util
{
    /// <summary>
    /// The full file system of a Souls game
    /// </summary>
    public class FileHierarchy
    {

    }

    /// <summary>
    /// A file in a <code>FileHierarchy</code>
    /// </summary>
    public class FileHierarchyFile
    {
        /// <summary>
        /// The name of the containing .bdt file
        /// </summary>
        public string BdtName { get; set; }
        /// <summary>
        /// The name of the file and its extension (if known)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The path of the file in its .bdt as it would appear before being hashed in the .bhd
        /// </summary>
        public string PathInBdt { get; set; }
        /// <summary>
        /// The path of the file as it would appear on the filesystem relative to the fs root
        /// </summary>
        public string PathInFs { get; set; }
        /// <summary>
        /// The path of this file's containing file, only if this file is contained in another file (e.g. inside a bhd or tpf)
        /// </summary>
        public string? ParentPathInBdt { get; set; }

        /// <summary>
        /// The encryption key(s) for this file if the file is encrypted inside a bdt
        /// </summary>
        public BHD5.AESKey? Key { get; set; }

        /// <summary>
        /// Creates a new FileHierarchyFile from literal data
        /// </summary>
        public FileHierarchyFile(string bdtName, string name, string pathInBdt, string pathInFs, string? parentPathInBdt, BHD5.AESKey? key)
        {
            BdtName = bdtName;
            Name = name;
            PathInBdt = pathInBdt;
            PathInFs = pathInFs;
            ParentPathInBdt = parentPathInBdt;
            Key = key;
        }


    }
}
