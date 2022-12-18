using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpBodyCinfo : HavokTypeBase
    {
        //ptr
        public hknpShape shape;
        public int flags;
        public short collisionCntrl;
        public uint collisionFilterInfo;
        public ushort materialId;
        public byte qualityId;
        public string name;
        public ulong userData;
        public byte motionType;
        public hkVector4f position;
        public hkQuaternionf orientation;
        public hkVector4f linearVelocity;
        public hkVector4f angularVelocity;
        public float mass;
        //ptr
        public hknpRefMassDistribution massDistribution;
        //ptr
        public hknpRefDragProperties dragProperties;
        public ushort motionPropertiesId;
        public uint desiredBodyId;
        public uint motionId;
        public float collisionLookAheadDistance;
        //ptr
        public hkLocalFrame localFrame;
        public sbyte activationPriority;

        public hknpBodyCinfo(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            ReadPtr(out shape, r, f);
            flags = r.ReadInt32();
            collisionCntrl = r.ReadInt16();
            r.Skip(2);
            collisionFilterInfo = r.ReadUInt32();
            materialId = r.ReadUInt16();
            qualityId = r.ReadByte();
            r.Skip(1);
            name = ReadStr(r, f);
            userData = r.ReadUInt64();
            motionType = r.ReadByte();
            r.Skip(7);
            position = hkVector4f.Read(r, f);
            orientation = hkQuaternionf.Read(r, f);
            linearVelocity = hkVector4f.Read(r, f);
            angularVelocity = hkVector4f.Read(r, f);
            mass = r.ReadSingle();
            r.Skip(4);
            ReadPtr(out massDistribution, r, f);
            ReadPtr(out dragProperties, r, f);
            motionPropertiesId = r.ReadUInt16();
            r.Skip(2);
            desiredBodyId = r.ReadUInt32();
            motionId = r.ReadUInt32();
            collisionLookAheadDistance = r.ReadSingle();
            ReadPtr(out localFrame, r, f);
            activationPriority = r.ReadSByte();
            r.Skip(15);
        }
    }
}
