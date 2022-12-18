using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkStringPtr : HavokTypeBase
    {
        public string value;
        public hkStringPtr(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var ptr = r.ReadInt64();
            value = ReadStr(ptr, f);
        }
    }
}
