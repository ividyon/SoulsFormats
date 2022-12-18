using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkQsTransform : HavokTypeBase
    {
        public hkVector4f translation;
        public hkQuaternionf rotation;
        public hkVector4f scale;

        public hkQsTransform(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var translationType = ObjInfo.Type.members[0].type;
            var translationInfo = new HavokTagObject(translationType, r.ReadBytes(16), 0, 1);
            translation = new hkVector4f(translationInfo);
            translation.Read(f);
            var rotationType = ObjInfo.Type.members[0].type;
            var rotationInfo = new HavokTagObject(rotationType, r.ReadBytes(16), 0, 1);
            rotation = new hkQuaternionf(rotationInfo);
            rotation.Read(f);
            var scaleType = ObjInfo.Type.members[0].type;
            var scaleInfo = new HavokTagObject(scaleType, r.ReadBytes(16), 0, 1);
            scale = new hkVector4f(scaleInfo);
            scale.Read(f);
        }
    }
}
