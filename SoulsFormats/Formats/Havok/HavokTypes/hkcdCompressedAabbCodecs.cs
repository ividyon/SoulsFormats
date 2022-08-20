using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes.hkcdCompressedAabbCodecs
{
    public abstract class AabbCodecBase : HavokTypeBase {
        public AabbCodecBase(HavokTagObject o) : base(o) { }
    }

    public class CompressedAabbCodec : AabbCodecBase
    {
        public (byte, byte, byte) xyz;
        public CompressedAabbCodec(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            xyz = (r.ReadByte(), r.ReadByte(), r.ReadByte());
        }
    }
    public class Aabb4BytesCodec : CompressedAabbCodec
    {
        public byte data;

        public Aabb4BytesCodec(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            this.data = r.ReadByte();
        }
    }

    public class Aabb5BytesCodec : CompressedAabbCodec
    {
        public byte hiData;
        public byte loData;

        public Aabb5BytesCodec(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            hiData = r.ReadByte();
            loData = r.ReadByte();
        }
    }
}
