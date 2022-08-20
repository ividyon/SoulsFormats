using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpPhysicsSystemData : hkReferencedObject
    {
        public hknpMaterial[] materials;
        public hknpMotionProperties[] motionProperties;
        public bodyCinfoWithAttachment[] bodyCinfos;
        public string name;
        public byte microStepMultiplier;

        public hknpPhysicsSystemData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            ReadArr(out materials, r, f);
            ReadArr(out motionProperties, r, f);
            ReadArr(out bodyCinfos, r, f);
            name = ReadStr(r, f);
            microStepMultiplier = r.ReadByte();
            r.Skip(7);
        }
    }
}
