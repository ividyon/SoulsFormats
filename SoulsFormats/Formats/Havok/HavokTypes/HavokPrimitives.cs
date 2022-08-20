using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkUint8 : HavokTypeBase
    {
        public byte value;

        public hkUint8(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            value = r.ReadByte();
        }
    }
    public class hkUint16 : HavokTypeBase
    {
        public ushort value;

        public hkUint16(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            value = r.ReadUInt16();
        }
    }
    public class hkInt16 : HavokTypeBase
    {
        public short value;

        public hkInt16(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            value = r.ReadInt16();
        }
    }
    public class hkInt : HavokTypeBase
    {
        public int value;

        public hkInt(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            value = r.ReadInt32();
        }
    }
    public class hkUint32 : HavokTypeBase
    {
        public uint value;

        public hkUint32(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            value = r.ReadUInt32();
        }
    }
    public class hkUint64 : HavokTypeBase
    {
        public ulong value;

        public hkUint64(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            value = r.ReadUInt32();
        }
    }
}
