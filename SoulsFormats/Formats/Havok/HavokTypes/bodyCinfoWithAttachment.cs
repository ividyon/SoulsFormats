using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class bodyCinfoWithAttachment : hknpBodyCinfo
    {
        public int attachedBody;
        public bodyCinfoWithAttachment(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            attachedBody = r.ReadInt32();
        }
    }
}
