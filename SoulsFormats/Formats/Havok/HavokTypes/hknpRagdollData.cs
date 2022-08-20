using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpRagdollData : hkReferencedObject
    {
        public hknpMaterial[] materials;
        public hknpMotionProperties[] motionProperties;
        public bodyCinfoWithAttachment[] bodyCinfos;
        public hknpConstraintCinfo[] constraintCinfos;
        public string name;
        public byte microStepMultiplier;
        public hkaSkeleton skeleton;
        public int[] boneToBodyMap;
        public HavokTagObject[] bodyTags;

        public hknpRagdollData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            ReadArr(out materials, r, f);
            ReadArr(out motionProperties, r, f);
            ReadArr(out bodyCinfos, r, f);
            ReadArr(out constraintCinfos, r, f);
            name = ReadStr(r, f);
            microStepMultiplier = r.ReadByte();
            r.Skip(7);
            ReadPtr(out skeleton, r, f);
            boneToBodyMap = ReadArr<hkInt>(r, f).Select(v => v.value).ToArray();
            ReadArr(out bodyTags, r, f);
        }
    }
}
