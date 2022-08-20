using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public abstract class HavokTypeBase
    {
        public HavokTagObject? ObjInfo { get; set; }
        public HavokTypeBase(HavokTagObject o)
        {
            this.ObjInfo = o;
        }
        public abstract void Read(byte[] data, BinaryReaderEx r, HavokFile f);
        public void Read(HavokFile f)
        {
            var data = ObjInfo.Data;
            var r = new BinaryReaderEx(false, data);
            Read(data, r, f);
        }

        internal static string ReadStr(long ptr, HavokFile f)
        {
            var objInd = f.ObjectPtrs[(int)ptr];
            var ans = "";
            var len = f.ObjectInfos[objInd].containingArrayLength;
            for (int i = 0; i < len; i++) {
                ans += (char)f.ObjectInfos[objInd + i].Data[0];
            }
            if (ans.EndsWith('\0')) ans = ans[0..^1];
            return ans;
        }

        internal static string ReadStr(BinaryReaderEx r, HavokFile f)
        {
            return ReadStr(r.ReadInt64(), f);
        }

        internal static T ReadPtr<T>(BinaryReaderEx r, HavokFile f)
        {
            ulong ptr = r.ReadUInt64();
            int ind = f.ObjectPtrs[(int)ptr];
            if (ind == 0) return default(T);
            var o = f.Objects[ind];
            return (T)o;
        }

        internal static void ReadPtr<T>(out T val, BinaryReaderEx r, HavokFile f)
        {
            val = ReadPtr<T>(r, f);
        }

        internal static T[] ReadArr<T>(BinaryReaderEx r, HavokFile f)
        {
            var ptr = r.ReadInt64();
            var m_size = r.ReadInt32();
            var m_capacityAndFlags = r.ReadInt32();
            return ReadArrBase<T>(ptr, f, m_size);
        }

        internal static void ReadArr<T>(out T[] arr, BinaryReaderEx r, HavokFile f)
        {
            arr = ReadArr<T>(r, f);
        }

        internal static T[] ReadArrBase<T>(long ptr, HavokFile f, int len)
        {
            var objInd = f.ObjectPtrs[(int)ptr];

            if (len == 0) len = f.ObjectInfos[objInd].containingArrayLength;
            var ans = new T[len];
            for (int i = 0; i < len; i++) {
                var obj = f.Objects[objInd + i];
                ans[i] = (T)obj;
            }
            return ans;
        }

        internal static T[] ReadArrBase<T>(long ptr, HavokFile f) => ReadArrBase<T>(ptr, f, 0);

        internal static void ReadArrBase<T>(out T[] arr, long ptr, HavokFile f)
        {
            arr = ReadArrBase<T>(ptr, f, 0);
        }

        internal static T[] ReadPtrArr<T>(BinaryReaderEx r, HavokFile f)
        {
            var ptr = r.ReadInt64();
            var m_size = r.ReadInt32();
            var m_capacityAndFlags = r.ReadInt32();

            var objInd = f.ObjectPtrs[(int)ptr];

            if (m_size == 0) m_size = f.ObjectInfos[objInd].containingArrayLength;
            var ans = new T[m_size];
            for (int i = 0; i < m_size; i++) {
                hkRefPtr refPtr = (hkRefPtr)f.Objects[objInd + i];
                if (refPtr.val == null) refPtr.Read(f);
                ans[i] = (T)refPtr.val;
            }
            return ans;
        }

        internal static void ReadPtrArr<T>(out T[] arr, BinaryReaderEx r, HavokFile f)
        {
            arr = ReadPtrArr<T>(r, f);
        }
    }
}
