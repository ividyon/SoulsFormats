using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpConstraintCinfo : HavokTypeBase
    {
        //ptr
        public hkpConstraintData hkpConstraintData;
        public uint bodyA;
        public uint bodyB;
        public ushort flags;
        public string name;
        public uint desiredConstraintId;
        public uint constraintGroupId;

        public hknpConstraintCinfo(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            ReadPtr(out hkpConstraintData, r, f);
            bodyA = r.ReadUInt32();
            bodyB = r.ReadUInt32();
            flags = r.ReadUInt16();
            r.Skip(6);
            name = ReadStr(r, f);
            desiredConstraintId = r.ReadUInt32();
            constraintGroupId = r.ReadUInt32();
        }
    }
}
