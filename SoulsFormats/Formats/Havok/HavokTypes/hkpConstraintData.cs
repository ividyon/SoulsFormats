using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkpConstraintData : hkReferencedObject
    {
        public ulong userData;
        public hkpConstraintData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            userData = r.ReadUInt64();
        }
    }

    public class hkpRagdollConstraintData : hkpConstraintData
    {
        //TODO
        public byte[] atoms;

        public hkpRagdollConstraintData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            atoms = r.ReadBytes(384);
        }
    }
    public class hkpLimitedHingeConstraintData : hkpConstraintData
    {
        //TODO
        public byte[] atoms;

        public hkpLimitedHingeConstraintData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            atoms = r.ReadBytes(272);
        }
    }
}
