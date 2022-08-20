using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpShape : hkReferencedObject
    {
        //enum
        public ushort flags;
        //enum
        public byte type;
        public byte numShapeKeyBits;
        //enum
        public byte dispatchType;
        public float convexRadius;
        public ulong userData;
        public hkRefCountedProperties properties;

        public hknpShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            flags = r.ReadUInt16();
            type = r.ReadByte();
            numShapeKeyBits = r.ReadByte();
            dispatchType = r.ReadByte();
            r.Skip(3);
            convexRadius = r.ReadSingle();
            r.Skip(4);
            userData = r.ReadUInt64();
            ReadPtr(out properties, r, f);
        }
    }

    public class hknpShapeInstance : HavokTypeBase
    {
        public hkTransformf transform;
        public hkVector4f scale;
        //ptr
        public hknpShape shape;
        public ushort shapeTag;
        public ushort destructionTag;
        public byte isEmpty;
        public uint nextEmptyElement;
        public short instanceId;
        //ptr
        public hknpCompoundShape parentShape;
        public hknpShapeInstance(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            transform = new hkTransformf(r, f);
            scale = hkVector4f.Read(r, f);
            ReadPtr(out shape, r, f);
            shapeTag = r.ReadUInt16();
            destructionTag = r.ReadUInt16();
            isEmpty = r.ReadByte();
            r.Skip(3);
            nextEmptyElement = r.ReadUInt32();
            instanceId = r.ReadInt16();
            r.Skip(2);
            ReadPtr(out parentShape, r, f);
        }
    }

    public class hknpShapeSignals
    {
        public object shapeMutated;
        public object shapeDestroyed;
        public hknpShapeSignals(BinaryReaderEx r, HavokFile f)
        {
            HavokTypeBase.ReadPtr(out shapeMutated, r, f);
            HavokTypeBase.ReadPtr(out shapeDestroyed, r, f);
        }
    }

    public class hknpShapeMassProperties : hkReferencedObject
    {
        public hkCompressedMassProperties compressedMassProperties;

        public hknpShapeMassProperties(HavokTagObject o) : base(o) { }
        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            compressedMassProperties = new hkCompressedMassProperties(r, f);
        }
    }

    public struct hkCompressedMassProperties
    {
        public hkVector4f centerOfMass;
        public hkVector4f inertia;
        public (short, short, short, short) majorAxisSpace;
        public float mass;
        public float volume;
        public hkCompressedMassProperties(BinaryReaderEx r, HavokFile f)
        {
            centerOfMass = hkVector4f.ReadFromPackedVector3(r, f);
            inertia = hkVector4f.ReadFromPackedVector3(r, f);
            majorAxisSpace = (r.ReadInt16(), r.ReadInt16(), r.ReadInt16(), r.ReadInt16());
            mass = r.ReadSingle();
            volume = r.ReadSingle();
        }
    }

    public class hknpConvexShape : hknpShape
    {
        public Half maxAllowedPenetration;
        public hkVector4f[] vertices;
        public hknpConvexShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            maxAllowedPenetration = r.ReadHalf();
            var verticesArr = new hkRelArray<hkVector4f>(r, f);
            vertices = verticesArr.arr;
        }
    }

    public class hknpConvexPolytopeShape : hknpConvexShape
    {
        public hkVector4f[] planes;
        public Face[] faces;
        public byte[] indices;
        //ptr
        public Connectivity connectivity;
        public hknpConvexPolytopeShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            r.Skip(2);
            var planesArr = new hkRelArray<hkVector4f>(r, f);
            planes = planesArr.arr;
            var facesArr = new hkRelArray<Face>(r, f);
            faces = facesArr.arr;
            var indicesArr = new hkRelArray<hkUint8>(r, f);
            indices = indicesArr.arr.Select(v => v.value).ToArray();
            r.Skip(4);
            ReadPtr(out connectivity, r, f);
        }

        public class Face : HavokTypeBase
        {
            public ushort firstIndex;
            public byte numIndices;
            public byte minHalfAngle;
            public Face(HavokTagObject o) : base(o) { }
            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                firstIndex = r.ReadUInt16();
                numIndices = r.ReadByte();
                minHalfAngle = r.ReadByte();
            }
        }

        public class Connectivity : hkReferencedObject
        {
            public Edge[] vertexEdges;
            public Edge[] faceLinks;

            public Connectivity(HavokTagObject o) : base(o) { }

            public class Edge : HavokTypeBase
            {
                public Edge(HavokTagObject o) : base(o) { }
                public ushort faceIndex;
                public byte edgeIndex;
                public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
                {
                    faceIndex = r.ReadUInt16();
                    edgeIndex = r.ReadByte();
                    r.Skip(1);
                }
            }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                base.Read(data, r, f);
                ReadArr(out vertexEdges, r, f);
                ReadArr(out faceLinks, r, f);
            }
        }
    }

    public class hknpCapsuleShape : hknpConvexPolytopeShape
    {
        public hkVector4f a;
        public hkVector4f b;

        public hknpCapsuleShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            r.Skip(8);
            a = hkVector4f.Read(r, f);
            b = hkVector4f.Read(r, f);
        }
    }

    public class hknpCompositeShape : hknpShape
    {
        public uint shapeTagCodecInfo;
        //ptr
        public hkReferencedObject materialTable;

        public hknpCompositeShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            shapeTagCodecInfo = r.ReadUInt32();
            r.Skip(4);
            ReadPtr(out materialTable, r, f);
        }

    }

    public class hknpCompoundShape : hknpCompositeShape
    {
        public hkFreeListArray<hknpShapeInstance> instances;
        public VelocityInfo[] instanceVelocities;
        public hkAabb aabb;
        public float boundingRadius;
        public bool isMutable;
        public int estimatedNumShapeKeys;
        public hknpShapeSignals mutationSignals;
        public hknpCompoundShapeData boundingVolumeData;

        public hknpCompoundShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            instances = new hkFreeListArray<hknpShapeInstance>(r, f);
            ReadArr(out instanceVelocities, r, f);
            aabb = new hkAabb(r, f);
            boundingRadius = r.ReadSingle();
            isMutable = r.ReadBoolean();
            r.Skip(3);
            estimatedNumShapeKeys = r.ReadInt32();
            mutationSignals = new hknpShapeSignals(r, f);
            var bvdType = ObjInfo.Type.members[19].type;
            var bvdData = r.ReadBytes(112);
            var o = new HavokTagObject(bvdType, bvdData, 0, 1);
            boundingVolumeData = new hknpCompoundShapeData(o);
            boundingVolumeData.Read(f);
        }
        public class VelocityInfo : HavokTypeBase
        {
            public hkVector4f linearVelocity;
            public hkVector4f angularVelocity;
            public VelocityInfo(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                linearVelocity = hkVector4f.Read(r, f);
                angularVelocity = hkVector4f.Read(r, f);
            }
        }
    }

    public class hknpCompoundShapeData : hkReferencedObject
    {
        public hkcdDynamicTree.DefaultTree32 aabbTree;
        public hknpCompoundShapeSimdTree simdTree;
        //enum
        public byte type;
        public hknpCompoundShapeData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            aabbTree = new hkcdDynamicTree.DefaultTree32(r, f);
            simdTree = new hknpCompoundShapeSimdTree(r, f);
            type = r.ReadByte();
            r.Skip(7);
        }
    }

    public class hknpCompressedMeshShape : hknpCompositeShape
    {
        //ptr
        public hkReferencedObject data;
        public hkBitField trianglesInterior;
        public int numTriangles;
        public int numConvexShapes;
        public hknpShapeInstance[] externShapes;

        public hknpCompressedMeshShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            ReadPtr(out this.data, r, f);
            trianglesInterior = new hkBitField(r, f);
            numTriangles = r.ReadInt32();
            numConvexShapes = r.ReadInt32();
            ReadArr(out externShapes, r, f);
        }
    }

    public class hknpCompressedMeshShapeData : hkReferencedObject
    {
        public hkcdDefaultStaticMeshTree meshTree;
        public hkcdSimdTree simdTree;
        //TODO
        public byte[] connectivity;
        public bool hasSimdTree;

        public hknpCompressedMeshShapeData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            r.Skip(8);
            //meshTree = r.ReadBytes(160);
            var mtData = r.ReadBytes(160);
            var mtType = ObjInfo.Type.members[3].type;
            var mtInfo = new HavokTagObject(mtType, mtData, 0, 1);
            meshTree = new hkcdDefaultStaticMeshTree(mtInfo);
            meshTree.Read(f);
            simdTree = new hkcdSimdTree(r, f);
            connectivity = r.ReadBytes(48);
            hasSimdTree = r.ReadBoolean();
            r.Skip(7);
        }
    }

    public class fsnpCustomParamCompressedMeshShape : hknpCompressedMeshShape
    {
        //ptr
        public fsnpCustomMeshParameter pParam;
        public uint[] triangleIndexToShapeKey;

        public fsnpCustomParamCompressedMeshShape(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            ReadPtr(out pParam, r, f);
            triangleIndexToShapeKey = ReadArr<hkUint32>(r, f).Select(v => v.value).ToArray();
        }
    }

    public class fsnpCustomMeshParameter : hkReferencedObject
    {
        public TriangleData[] triangleDataArray;
        public PrimitiveData[] primitiveDataArray;
        public int vertexDataStride;
        public int triangleDataStride;
        public uint version;

        public fsnpCustomMeshParameter(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);
            ReadArr(out triangleDataArray, r, f);
            ReadArr(out primitiveDataArray, r, f);
            vertexDataStride = r.ReadInt32();
            triangleDataStride = r.ReadInt32();
            version = r.ReadUInt32();
            r.Skip(4);
        }
        public class TriangleData : HavokTypeBase
        {
            public uint primitiveDataIndex;
            public uint triangleDataIndex;
            public uint vertexIndexA;
            public uint vertexIndexB;
            public uint vertexIndexC;

            public TriangleData(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                primitiveDataIndex = r.ReadUInt32();
                triangleDataIndex = r.ReadUInt32();
                vertexIndexA = r.ReadUInt32();
                vertexIndexB = r.ReadUInt32();
                vertexIndexC = r.ReadUInt32();
            }
        }
        public class PrimitiveData : HavokTypeBase
        {
            public byte[] vertexData;
            public byte[] triangleData;
            public byte[] primitiveData;
            public uint materialNameData;

            public PrimitiveData(HavokTagObject o) : base(o) { }

            public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
            {
                vertexData = ReadArr<hkUint8>(r, f).Select(v => v.value).ToArray();
                triangleData = ReadArr<hkUint8>(r, f).Select(v => v.value).ToArray();
                primitiveData = ReadArr<hkUint8>(r, f).Select(v => v.value).ToArray();
                materialNameData = r.ReadUInt32();
                r.Skip(4);
            }
        }
    }
}
