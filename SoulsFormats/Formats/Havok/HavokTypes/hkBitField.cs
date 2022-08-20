using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkBitField
    {
        public uint[] words;
        public int numBits;
        public hkBitField(BinaryReaderEx r, HavokFile f)
        {
            words = HavokTypeBase.ReadArr<hkUint32>(r, f).Select(v => v.value).ToArray();
            numBits = r.ReadInt32();
            r.Skip(4);
        }
    }
}
