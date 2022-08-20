using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public struct hkRelArray<T>
    {
        public ushort size;
        public ushort offset;
        public T[] arr;
        public hkRelArray(BinaryReaderEx r, HavokFile f)
        {
            //type data appears to be wrong about the order here?
            //in the type desc size is first and offset is second
            //maybe an issue with Havok source? e.g. programmer copy-pasted and used wrong names?
            offset = r.ReadUInt16();
            size = r.ReadUInt16();
            HavokTypeBase.ReadArrBase(out arr, offset, f);
        }
    }
}
