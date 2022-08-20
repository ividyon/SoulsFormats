using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public struct hknpDragProperties
    {
        public (hkVector4f, hkVector4f, hkVector4f) centerAndOffset;
        public (hkVector4f, hkVector4f, hkVector4f, hkVector4f, hkVector4f, hkVector4f) angularEffectsAndArea;
        public (float, float, float, float, float, float, float, float, float, float, float, float) armUVs;
        public hknpDragProperties(BinaryReaderEx r, HavokFile f)
        {
            var readVec = () => hkVector4f.Read(r, f);
            centerAndOffset = (readVec(), readVec(), readVec());
            angularEffectsAndArea = (readVec(), readVec(), readVec(), readVec(), readVec(), readVec());
            var readF = () => r.ReadSingle();
            armUVs = (readF(), readF(), readF(), readF(), readF(), readF(), readF(), readF(), readF(), readF(), readF(), readF());
        }
    }

    public class hknpRefDragProperties : hkReferencedObject
    {
        public hknpDragProperties dragProperties;
        public hknpRefDragProperties(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            dragProperties = new hknpDragProperties(r, f);
        }
    }
}
