using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkArray : HavokTypeBase
    {
        object[] m_data;
        int m_size;
        int m_capacityAndFlags;

        public hkArray(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            var ptr = r.ReadInt64();
            m_size = r.ReadInt32();
            m_capacityAndFlags = r.ReadInt32();

            var objInd = f.ObjectPtrs[(int)ptr];
            //maybe wrong?
            //if size is recorded as 0, assume we just take the whole array
            if (m_size == 0) m_size = f.ObjectInfos[objInd].containingArrayLength;
            m_data = new object[m_size];
            for (int i = 0; i < m_size; i++) {
                m_data[i] = f.Objects[objInd + i];
            }
        }
    }

    public struct hkFreeListArray<T>
    {
        T[] arr;
        int m_size;
        int m_capacityAndFlags;
        int firstFree;
        public hkFreeListArray(BinaryReaderEx r, HavokFile f)
        {
            m_size = r.GetInt32(8);
            m_capacityAndFlags = r.GetInt32(12);
            arr = HavokTypeBase.ReadArr<T>(r, f);
            firstFree = r.ReadInt32();
            r.Skip(4);
        }
    }
}
