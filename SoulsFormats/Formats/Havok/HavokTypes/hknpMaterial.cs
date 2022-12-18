using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpMaterial : hkReferencedObject
    {
        public string name;
        public uint isExclusive;
        public int flags;
        //enum
        public byte triggerType;
        //float?
        public byte triggerManifoldTolerance;
        //float?
        public Half dynamicFriction;
        //float?
        public Half staticFriction;
        //float?
        public Half restitution;
        //enum
        public byte frictionCombinePolicy;
        public byte restitutionCombinePolicy;
        public float weldingTolerance;
        public float maxContactImpulse;
        public float fractionOfClippedImpulseToApply;
        //enum
        public byte massChangerCategory;
        //float?
        public Half massChangerHeavyObjectFactor;
        //float?
        public Half softContactForceFactor;
        //float?
        public Half softContactDampFactor;
        //float?
        public byte softContactSeperationVelocity;
        //ptr
        public object surfaceVelocity;
        //float?
        public Half disablingCollisionsBetweenCvxCvxDynamicObjectsDistance;
        public ulong userData;

        public hknpMaterial(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            name = ReadStr(r, f);
            isExclusive = r.ReadUInt32();
            flags = r.ReadInt32();
            triggerType = r.ReadByte();
            triggerManifoldTolerance = r.ReadByte();
            dynamicFriction = r.ReadHalf();
            staticFriction = r.ReadHalf();
            restitution = r.ReadHalf();
            frictionCombinePolicy = r.ReadByte();
            restitutionCombinePolicy = r.ReadByte();
            r.Skip(2);
            weldingTolerance = r.ReadSingle();
            maxContactImpulse = r.ReadSingle();
            fractionOfClippedImpulseToApply = r.ReadSingle();
            massChangerCategory = r.ReadByte();
            r.Skip(1);
            massChangerHeavyObjectFactor = r.ReadHalf();
            softContactForceFactor = r.ReadHalf();
            softContactDampFactor = r.ReadHalf();
            softContactSeperationVelocity = r.ReadByte();
            r.Skip(7);
            ReadPtr(out surfaceVelocity, r, f);
            disablingCollisionsBetweenCvxCvxDynamicObjectsDistance = r.ReadHalf();
            r.Skip(6);
            userData = r.ReadUInt64();

        }
    }
}
