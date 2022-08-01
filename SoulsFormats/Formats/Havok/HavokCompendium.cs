using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok
{
    /// <summary>
    /// A Havok tagfile, either .compendium or .hkx/.hkt
    /// </summary>
    public class HavokCompendium : SoulsFile<HavokCompendium>
    {
        /// <summary>
        /// The types either described by this file
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
        /// Returns a string reperesentation of this object
        /// </summary>
        public override string? ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Loads file data from a BinaryReaderEx.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;
            if (br.GetASCII(4, 4) == "TCM0") {
                var tmp = new HavokSectionTCM0();
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
