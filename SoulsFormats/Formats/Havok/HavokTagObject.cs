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
        public int dataStart;
        public int dataEnd;
        public int containingArrayLength;
        public Dictionary<string, HavokTagObject> Members { get; } = new();
        public HavokTagObject(HavokTagType type, byte[] data, uint flags, int containingArrayLength)
        {
            Type = type;
            Data = data;
            this.flags = flags;
            this.containingArrayLength = containingArrayLength;
        }
        public void Init(List<HavokTagObject> objects)
        {
            foreach (var m in Type.members) {
                
            }
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
