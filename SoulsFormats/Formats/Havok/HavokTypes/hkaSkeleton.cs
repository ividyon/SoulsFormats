using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkaSkeleton : hkReferencedObject
    {
        public string name;
        /// <summary>
        /// Probably the indices of which bone is the parent of the bone i.
        /// -1 = no parent.
        /// </summary>
        public short[] parentIndices;
        public hkaBone[] bones;
        public hkQsTransform[] referencePose;
        public HavokTagObject[] referenceFloats;
        public HavokTagObject[] floatSlots;
        public HavokTagObject[] localFrames;
        public HavokTagObject[] partitions;
        public hkaSkeleton(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            name = ReadStr(r.ReadInt64(), f);
            parentIndices = ReadArr<hkInt16>(r, f).Select(v => v.value).ToArray();
            ReadArr(out bones, r, f);
            ReadArr(out referencePose, r, f);
            ReadArr(out referenceFloats, r, f);
            ReadArr(out floatSlots, r, f);
            ReadArr(out localFrames, r, f);
            ReadArr(out partitions, r, f);
        }
    }
}
