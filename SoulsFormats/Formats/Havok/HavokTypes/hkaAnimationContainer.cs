using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkaAnimationContainer : hkReferencedObject
    {
        hkaSkeleton[] skeletons;
        object[] animations;
        object[] bindings;
        object[] attachments;
        object[] skins;

        public hkaAnimationContainer(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);

            ReadPtrArr(out skeletons, r, f);
            ReadArr(out animations, r, f);
            ReadArr(out bindings, r, f);
            ReadArr(out attachments, r, f);
            ReadArr(out skins, r, f);
        }
    }
}
