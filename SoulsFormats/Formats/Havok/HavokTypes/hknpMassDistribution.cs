using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public struct hknpMassDistribution
    {
        public hkVector4f centerOfMassAndVolume;
        public hkQuaternionf majorAxisSpace;
        public hkVector4f inertiaTensor;
        public hknpMassDistribution(BinaryReaderEx r, HavokFile f)
        {
            centerOfMassAndVolume = hkVector4f.Read(r, f);
            majorAxisSpace = hkQuaternionf.Read(r, f);
            inertiaTensor = hkVector4f.Read(r, f);
        }
    }

    public class hknpRefMassDistribution : hkReferencedObject
    {
        public hknpMassDistribution massDistribution;
        public hknpRefMassDistribution(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            massDistribution = new hknpMassDistribution(r, f);
        }
    }
}
