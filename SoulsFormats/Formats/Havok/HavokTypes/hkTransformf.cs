using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkTransformf
    {
        public byte[] rotation;
        public hkVector4f translation;
        public hkTransformf(BinaryReaderEx r, HavokFile f)
        {
            rotation = r.ReadBytes(48);
            translation = hkVector4f.Read(r, f);
        }
    }
}
