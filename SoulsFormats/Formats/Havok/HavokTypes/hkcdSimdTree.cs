using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkcdSimdTree
    {
        public Node[] nodes;
        public bool isCompact;

        public hkcdSimdTree(BinaryReaderEx r, HavokFile f)
        {
            HavokTypeBase.ReadArr(out nodes, r, f);
            isCompact = r.ReadBoolean();
            r.Skip(7);
        }

        public class Node : hkcdFourAabb
        {
            public (uint, uint, uint, uint) data;
            public bool isLeaf;
            public bool isActive;
            public Node(HavokTagObject o) : base(o) { }
            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                base.Read(data, r, f);
                this.data = (r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32());
                isLeaf = r.ReadBoolean();
                isActive = r.ReadBoolean();
                r.Skip(15);
            }
        }
    }
    public class hknpCompoundShapeSimdTree : hkcdSimdTree
    {
        public hkVector4f[] points;

        public hknpCompoundShapeSimdTree(BinaryReaderEx r, HavokFile f) : base(r, f)
        {
            HavokTypeBase.ReadArr(out points, r, f);
        }
    }
}
