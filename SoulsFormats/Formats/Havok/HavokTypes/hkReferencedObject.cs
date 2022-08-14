using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkReferencedObject : HavokTypeBase
    {
        hkPropertyBag? propertyBag;
        ushort memSizeAndFlags;
        ushort refCount;

        public hkReferencedObject(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var propBagPtr = r.ReadUInt64();
            if (propBagPtr != 0) throw new Exception("Property bag was not null in hkReferencedObject");
            memSizeAndFlags = r.ReadUInt16();
            refCount = r.ReadUInt16();
        }
    }

    public class hkPropertyBag : HavokTypeBase
    {
        public struct hkPropertyDesc
        {
            //ptr
            public object type;
            //ptr
            public string name;
            public int flags;
        }

        public Dictionary<(int, hkPropertyDesc), object> propertyMap = new();

        public hkPropertyBag(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
        }

    }
}
