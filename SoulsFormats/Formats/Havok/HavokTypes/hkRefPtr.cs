using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkRefPtr : HavokTypeBase
    {
        public object val;

        public hkRefPtr(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var ptr = r.ReadInt64();
            var ind = f.ObjectPtrs[(int)ptr];
            val = f.Objects[ind];
        }
    }
}
