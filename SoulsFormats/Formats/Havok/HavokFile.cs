using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok
{
    /// <summary>
    /// A Havok tagfile, either .compendium or .hkx/.hkt
    /// </summary>
    public class HavokFile : SoulsFile<HavokFile>
    {
        /// <summary>
        /// The types either described by this file or its compendium
        /// </summary>
        public List<HavokTagType> TagTypes { get; set; } = new();
        /// <summary>
        /// The strings listed in the TSTR section, used for type names and templates
        /// </summary>
        public List<string> TagTypeStrings { get; set; } = new();
        /// <summary>
        /// The strings listed in the FSTR section, used for everything TSTR isn't
        /// </summary>
        public List<string> TagFieldStrings { get; set; } = new();

        /// <summary>
        /// The compendium for this file, if it has one
        /// </summary>
        public HavokFile? Compendium { get; set; }

        public List<HavokTagObject>? Objects { get; set; }

        public byte[]? ObjectData { get; set; }

        /// <summary>
        /// Returns a string reperesentation of this object
        /// </summary>
        public override string? ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Creates a new, empty Havok file
        /// </summary>
        public HavokFile()
        {

        }

        /// <summary>
        /// Reads a Havok file that is either a compendium or does not have an associated compendium
        /// </summary>
        public HavokFile(string filename) : this(filename, null)
        {
        }

        /// <summary>
        /// Reads a Havok file that may have an associated compendium
        /// </summary>
        public HavokFile(string filename, HavokFile? compendium)
        {
            this.Compendium = compendium;
            if (compendium != null) {
                this.TagTypes = compendium.TagTypes;
                this.TagTypeStrings = compendium.TagTypeStrings;
                this.TagFieldStrings = compendium.TagFieldStrings;
            }
            var bytes = File.ReadAllBytes(filename);
            var reader = new BinaryReaderEx(false, bytes);
            Read(reader);

        }

        /// <summary>
        /// Loads file data from a BinaryReaderEx.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;
            var firstTag = br.GetASCII(4, 4);
            if (firstTag == "TCM0") {
                //compendium file
                var tmp = new HavokSectionTCM0();
                tmp.Read(br, this);
            } else if (firstTag == "TAG0") {
                var tmp = new HavokSectionTAG0();
                tmp.Read(br, this);
            } else throw new Exception("Unknown havok format");
        }

        /// <summary>
        /// Writes file data to a BinaryWriterEx.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
        }
    }

}
