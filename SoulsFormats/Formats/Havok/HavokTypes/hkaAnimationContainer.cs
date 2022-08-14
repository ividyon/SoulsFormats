using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkaAnimationContainer : hkReferencedObject
    {
        hkArray skeletons;
        hkArray animations;
        hkArray bindings;
        hkArray attachments;
        hkArray skins;

        public hkaAnimationContainer(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            var currArrType = ObjInfo.Type.members[3].type;
            var currArrObj = new HavokTagObject(currArrType, r.ReadBytes(16), 0, 1);
            skeletons = new hkArray(currArrObj);
            skeletons.Read(f);

            currArrType = ObjInfo.Type.members[4].type;
            currArrObj = new HavokTagObject(currArrType, r.ReadBytes(16), 0, 1);
            animations = new hkArray(currArrObj);
            animations.Read(f);

            currArrType = ObjInfo.Type.members[5].type;
            currArrObj = new HavokTagObject(currArrType, r.ReadBytes(16), 0, 1);
            bindings = new hkArray(currArrObj);
            bindings.Read(f);

            currArrType = ObjInfo.Type.members[6].type;
            currArrObj = new HavokTagObject(currArrType, r.ReadBytes(16), 0, 1);
            attachments = new hkArray(currArrObj);
            attachments.Read(f);

            currArrType = ObjInfo.Type.members[7].type;
            currArrObj = new HavokTagObject(currArrType, r.ReadBytes(16), 0, 1);
            skins = new hkArray(currArrObj);
            skins.Read(f);

        }
    }
}
