using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkaBone : HavokTypeBase
    {
        public string name;
        public bool lockTranslation;

        public hkaBone(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            name = ReadStr(r.ReadInt64(), f);
            lockTranslation = r.ReadBoolean();
            r.Skip(7);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
