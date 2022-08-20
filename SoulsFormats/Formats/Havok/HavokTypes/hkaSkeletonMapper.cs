using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkaSkeletonMapper : hkReferencedObject
    {
        public hkaSkeletonMapperData mapping;
        public hkaSkeletonMapper(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            r.Skip(8);
            mapping = new hkaSkeletonMapperData(ObjInfo.Type.members[3].type, r, f);
        }
    }

    public struct hkaSkeletonMapperData
    {
        //ptr
        public hkaSkeleton skeletonA;
        public hkaSkeleton skeletonB;
        public HavokTagObject[] partitionMap;
        public HavokTagObject[] simpleMappingPartitionRanges;
        public HavokTagObject[] chainMappingPartitionRanges;
        public SimpleMapping[] simpleMappings;
        public HavokTagObject[] chainMappings;
        public short[] unmappedBones;
        public hkQsTransform extractedMotionMapping;
        public bool keepUnmappedLocal;
        //enum
        public uint mappingType;
        public hkaSkeletonMapperData(HavokTagType t, BinaryReaderEx r, HavokFile f)
        {
            HavokTypeBase.ReadPtr(out skeletonA, r, f);
            HavokTypeBase.ReadPtr(out skeletonB, r, f);
            HavokTypeBase.ReadArr(out partitionMap, r, f);
            HavokTypeBase.ReadArr(out simpleMappingPartitionRanges, r, f);
            HavokTypeBase.ReadArr(out chainMappingPartitionRanges, r, f);
            HavokTypeBase.ReadArr(out simpleMappings, r, f);
            HavokTypeBase.ReadArr(out chainMappings, r, f);
            unmappedBones = HavokTypeBase.ReadArr<hkInt16>(r, f).Select(v => v.value).ToArray();
            var emmData = r.ReadBytes(48);
            var emmType = t.members[8].type;
            var emmInfo = new HavokTagObject(emmType, emmData, 0, 1);
            extractedMotionMapping = new hkQsTransform(emmInfo);
            extractedMotionMapping.Read(f);
            keepUnmappedLocal = r.ReadBoolean();
            r.Skip(3);
            mappingType = r.ReadUInt32();
        }
        public class SimpleMapping : HavokTypeBase
        {
            short boneA;
            short boneB;
            hkQsTransform aFromBTransform;
            public SimpleMapping(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                boneA = r.ReadInt16();
                boneB = r.ReadInt16();
                var tData = r.ReadBytes(48);
                var tType = ObjInfo.Type.members[2].type;
                var tInfo = new HavokTagObject(tType, tData, 0, 1);
                aFromBTransform = new hkQsTransform(tInfo);
                aFromBTransform.Read(f);
            }
        }
    }
}
