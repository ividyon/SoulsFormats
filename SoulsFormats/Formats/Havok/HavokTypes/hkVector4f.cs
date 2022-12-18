using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkVector4f : HavokTypeBase
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public hkVector4f(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            x = r.ReadSingle();
            y = r.ReadSingle();
            z = r.ReadSingle();
            w = r.ReadSingle();
        }

        public static hkVector4f Read(BinaryReaderEx r, HavokFile f)
        {
            var ans = new hkVector4f(null);
            var data = r.ReadBytes(16);
            var r2 = new BinaryReaderEx(true, data);
            ans.Read(data, r2, f);
            return ans;
        }

        public static hkVector4f ReadFromPackedVector3(BinaryReaderEx r, HavokFile f)
        {
            //may be wrong
            var a = r.ReadBytes(2);
            var end = r.ReadBytes(2);
            var b = r.ReadBytes(2);
            var c = r.ReadBytes(2);
            var actual = new byte[] { 
                a[0], a[1], end[0], end[1],
                b[0], b[1], end[0], end[1],
                c[0], c[1], end[0], end[1],
                0x3f, 0x80, 0, 0
            };
            return Read(new BinaryReaderEx(true, actual), f);
        }

        public static hkVector4f FromFloats(float x, float y, float z, float w)
        {
            var ans = new hkVector4f(null);
            ans.x = x;
            ans.y = y;
            ans.z = z;
            ans.w = w;
            return ans;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }

    public class hkAabb : HavokTypeBase
    {
        public hkVector4f min;
        public hkVector4f max;
        public hkAabb(BinaryReaderEx r, HavokFile f) : base(null)
        {
            var data2 = r.ReadBytes(32);
            var r2 = new BinaryReaderEx(false, data2);
            min = hkVector4f.FromFloats(r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle());
            max = hkVector4f.FromFloats(r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle());
        }
        public hkAabb(HavokTagObject o) : base(o) { }
        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var data2 = r.ReadBytes(32);
            var r2 = new BinaryReaderEx(false, data2);
            min = hkVector4f.FromFloats(r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle());
            max = hkVector4f.FromFloats(r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle(), r2.ReadSingle());
        }
    }

    public class hkcdFourAabb : HavokTypeBase
    {
        public hkVector4f lx;
        public hkVector4f hx;
        public hkVector4f ly;
        public hkVector4f hy;
        public hkVector4f lz;
        public hkVector4f hz;
        public hkcdFourAabb(HavokTagObject o) : base(o) { }
        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            lx = hkVector4f.Read(r, f);
            hx = hkVector4f.Read(r, f);
            ly = hkVector4f.Read(r, f);
            hy = hkVector4f.Read(r, f);
            lz = hkVector4f.Read(r, f);
            hz = hkVector4f.Read(r, f);
        }
    }
}
