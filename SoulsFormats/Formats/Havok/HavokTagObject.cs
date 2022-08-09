using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok
{
    public class HavokTagObject
    {
        public HavokTagType Type { get; set; }
        public byte[] Data { get; set; }
        public uint flags;
        public int unk;
        public HavokTagObject(HavokTagType type, byte[] data, uint flags, int unk)
        {
            Type = type;
            Data = data;
            this.flags = flags;
            this.unk = unk;
        }
    }
}
