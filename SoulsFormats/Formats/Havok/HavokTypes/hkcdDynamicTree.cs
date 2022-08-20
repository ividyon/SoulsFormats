using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkcdDynamicTree
    {
        public class DynamicStorage<Codec> : TreeStorage
        {
            public Codec[] nodes;
            public ushort firstFree;
            public void Read(BinaryReaderEx r, HavokFile f)
            {
                nodes = HavokTypeBase.ReadArr<Codec>(r, f);
                firstFree = r.ReadUInt16();
                r.Skip(6);
            }
        }

        public class DynamicStorage16 : DynamicStorage<hkAabb>
        {

        }

        public interface TreeStorage
        {
            public void Read(BinaryReaderEx r, HavokFile f);
        }

        public class Tree<Storage> where Storage : TreeStorage, new()
        {
            public Storage storage;
            public uint numLeaves;
            public uint path;
            public ushort root;
            public Tree(BinaryReaderEx r, HavokFile f)
            {
                storage = new Storage();
                storage.Read(r, f);
                numLeaves = r.ReadUInt32();
                path = r.ReadUInt32();
                root = r.AssertUInt16();
                r.Skip(6);
            }
        }

        public class DefaultTree32 : Tree<DynamicStorage16>
        {
            public DefaultTree32(BinaryReaderEx r, HavokFile f) : base(r, f)
            {

            }
        }
    }
}
