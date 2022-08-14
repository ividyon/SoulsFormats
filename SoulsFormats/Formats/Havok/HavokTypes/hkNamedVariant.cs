using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkNamedVariant : HavokTypeBase
    {
        public string name;
        public string className;
        public object variant;
        public hkNamedVariant(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var namePtr = r.ReadInt64();
            var classNamePtr = r.ReadInt64();
            var variantPtr = r.ReadInt64();
            var variantInd = f.ObjectPtrs[(int)variantPtr];

            name = ReadStr(namePtr, f);
            className = ReadStr(classNamePtr, f);
            variant = f.Objects[variantInd];
        }
    }
}
