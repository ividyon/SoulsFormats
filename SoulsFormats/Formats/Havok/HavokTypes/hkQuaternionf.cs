using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkQuaternionf : hkVector4f
    {
        public hkQuaternionf(HavokTagObject o) : base(o) { }


        public static hkQuaternionf Read(BinaryReaderEx r, HavokFile f)
        {
            var ans = new hkQuaternionf(null);
            var data = r.ReadBytes(16);
            var r2 = new BinaryReaderEx(true, data);
            ans.Read(data, r2, f);
            return ans;
        }
    }
}
