using SoulsFormats.Formats.Havok.HavokTypes.hkcdCompressedAabbCodecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hkcdDefaultStaticMeshTree : hkcdStaticMeshTree.Base
    {
        public uint[] packedVertices;
        public ulong[] sharedVertices;
        public PrimitiveDataRun[] primitiveDataRuns;

        public hkcdDefaultStaticMeshTree(HavokTagObject o) : base(o)
        {
        }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            packedVertices = ReadArr<hkUint32>(r, f).Select(v => v.value).ToArray();
            sharedVertices = ReadArr<hkUint64>(r, f).Select(v => v.value).ToArray();
            ReadArr(out primitiveDataRuns, r, f);
        }

        public void UnpackVertices()
        {

        }

        public class PrimitiveDataRun : HavokTypeBase
        {
            public ushort value;
            public byte index;
            public byte count;
            public PrimitiveDataRun(HavokTagObject o) : base(o) { }
            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                value = r.ReadUInt16();
                index = r.ReadByte();
                count = r.ReadByte();
            }
        }
    }

    public class hkcdStaticMeshTree
    {
        public abstract class AabbTreeBase : HavokTypeBase
        {
            public AabbTreeBase(HavokTagObject o) : base(o) { }
        }

        public class AabbTree<Node> : AabbTreeBase
        {
            public Node[] nodes;
            public hkAabb domain;

            public AabbTree(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                ReadArr(out nodes, r, f);
                domain = new hkAabb(r, f);
            }
        }
        public class Aabb5BytesTree : AabbTree<Aabb5BytesCodec>
        {
            public Aabb5BytesTree(HavokTagObject o) : base(o) { }
        }

        public class Base : Aabb5BytesTree
        {
            public int numPrimitiveKeys;
            public int bitsPerKey;
            public uint maxKeyValue;
            public byte primitiveStoreIsFlatConvex;
            public Section[] sections;
            public Primitive[] primitives;
            public ushort[] sharedVerticesIndex;

            public Base(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                base.Read(data, r, f);
                numPrimitiveKeys = r.ReadInt32();
                bitsPerKey = r.ReadInt32();
                maxKeyValue = r.ReadUInt32();
                primitiveStoreIsFlatConvex = r.ReadByte();
                r.Skip(3);
                ReadArr(out sections, r, f);
                ReadArr(out primitives, r, f);
                sharedVerticesIndex = ReadArr<hkUint16>(r, f).Select(v => v.value).ToArray();
            }
        }

        public class Aabb4BytesTree : AabbTree<Aabb4BytesCodec>
        {
            public Aabb4BytesTree(HavokTagObject o) : base(o) { }
        }
        public class Section : Aabb4BytesTree
        {
            public (float, float, float, float, float, float) codecParams;
            public uint firstPackedVertexIndex;
            public uint firstSharedVertexIndex;
            public uint firstPrimitiveIndex;
            public uint firstDataRunIndex;
            public byte numPackedVertices;
            public byte numPrimitives;
            public byte numDataRuns;
            public byte page;
            public ushort leafIndex;
            public byte layerData;
            public byte flags;

            public Section(HavokTagObject o) : base(o) { }
            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                base.Read(data, r, f);
                codecParams = (r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                firstPackedVertexIndex = r.ReadUInt32();
                firstSharedVertexIndex = r.ReadUInt32();
                firstPrimitiveIndex = r.ReadUInt32();
                firstDataRunIndex = r.ReadUInt32();
                numPackedVertices = r.ReadByte();
                numPrimitives = r.ReadByte();
                numDataRuns = r.ReadByte();
                page = r.ReadByte();
                leafIndex = r.ReadUInt16();
                layerData = r.ReadByte();
                flags = r.ReadByte();
            }

            public (hkVector4f, hkVector4f, hkVector4f, hkVector4f)[] UnpackPrimitives(hkcdDefaultStaticMeshTree mesh)
            {
                var ans = new List<(hkVector4f, hkVector4f, hkVector4f, hkVector4f)>(numPrimitives);
                for (int primInd = 0; primInd < numPrimitives; primInd++) {
                    var prim = mesh.primitives[primInd + firstPrimitiveIndex];
                    var a = unpackVector(mesh, prim.indices.Item1);
                    var b = unpackVector(mesh, prim.indices.Item2);
                    var c = unpackVector(mesh, prim.indices.Item3);
                    var d = unpackVector(mesh, prim.indices.Item4);
                    ans.Add((a, b, c, d));
                    if (prim.indices.Item3 != prim.indices.Item4) {
                        ans.Add((b, c, d, a));
                        ans.Add((a, c, d, b));
                        ans.Add((a, b, d, c));
                    }
                }
                return ans.ToArray();
            }
            private hkVector4f unpackVector(hkcdDefaultStaticMeshTree mesh, int ind)
            {
                if (ind < numPackedVertices) {
                    uint packed = mesh.packedVertices[ind + firstPackedVertexIndex];
                    //the indices into codecParams may be wrong, especially for y and w
                    float x = (float)(packed & 0x7ff) * codecParams.Item4 + codecParams.Item1;
                    float y = (float)((packed >> 0xb) & 0x7ff) * codecParams.Item5 + codecParams.Item2;
                    float z = (float)(packed >> 0x16) * codecParams.Item6 + codecParams.Item3;
                    float w = codecParams.Item4;
                    return hkVector4f.FromFloats(x, y, z, w);
                } else {
                    //4.768374e-07, 4.768374e-07, 2.384186e-07, 1.0
                    var d = mesh.domain;
                    var xmul = (d.max.x - d.min.x) / 0x1fffff;
                    var ymul = (d.max.y - d.min.y) / 0x1fffff;
                    var zmul = (d.max.z - d.min.z) / (0x3fffff);
                    var xadd = d.min.x;
                    var yadd = d.min.y;
                    var zadd = d.min.z;
                    var wadd = d.min.w;
                    var ind2 = mesh.sharedVerticesIndex[(int)firstSharedVertexIndex - (int)numPackedVertices + ind];
                    var packed = mesh.sharedVertices[page * 0x10000 + ind2];
                    var x = (float)((packed & 0x1fffff) * xmul + xadd);
                    var y = (float)((packed >> 0x15) & 0x1fffff) * ymul + yadd;
                    var z = (float)((packed >> 0x2a) & 0x3fffff) * zmul + zadd;
                    var w = wadd;
                    return hkVector4f.FromFloats((float)x, (float)y, (float)z, w);
                }
            }
        }
        public class Primitive : HavokTypeBase
        {
            public (byte, byte, byte, byte) indices;
            public Primitive(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                indices = (r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
            }
        }
    }
}
