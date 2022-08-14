using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public abstract class HavokTypeBase
    {
        public HavokTagObject ObjInfo { get; set; }
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

        protected static string ReadStr(long ptr, HavokFile f)
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

    }
}
