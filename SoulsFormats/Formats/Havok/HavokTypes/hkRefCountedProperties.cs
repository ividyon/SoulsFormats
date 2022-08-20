using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkRefCountedProperties : hkReferencedObject
    {
        public Entry[] entries;
        public hkRefCountedProperties(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            ReadArr(out entries, r, f);
        }

        public class Entry : HavokTypeBase
        {
            //ptr
            public hkReferencedObject @object;
            public ushort key;
            public ushort flags;

            public Entry(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                ReadPtr(out @object, r, f);
                key = r.ReadUInt16();
                flags = r.ReadUInt16();
                r.Skip(4);
            }
        }
    }
}
