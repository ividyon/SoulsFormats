using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    internal class HavokTypeRegistry
    {
        public static Dictionary<string, Func<HavokTagObject, HavokFile, HavokTypeBase>> registry = new() {
            { "hkArray", (a, _) => new hkArray(a) },
            { "hkRootLevelContainer::NamedVariant", (a, _) => new hkNamedVariant(a) },
            { "hkaAnimationContainer", (a, _) => new hkaAnimationContainer(a) },
            { "hknpPhysicsSceneData", (a, _) => new hknpPhysicsSceneData(a) },
            { "hknpPhysicsSystemData", (a, _) => new hknpPhysicsSystemData(a) },
            { "hkaSkeleton", (a, _) => new hkaSkeleton(a) },
            { "hkRefPtr", (a, _) => new hkRefPtr(a) },
            { "hkaBone", (a, _) => new hkaBone(a) },
            { "hkQsTransform", (a, _) => new hkQsTransform(a) },

            { "hkVector4f", (a, _) => new hkVector4f(a) },
            { "hkVector4", (a, _) => new hkVector4f(a) },
            { "hkQuaternionf", (a, _) => new hkQuaternionf(a) },
            { "hkQuaternion", (a, _) => new hkQuaternionf(a) },

            { "hknpRagdollData", (a, _) => new hknpRagdollData(a) },
            { "hknpMaterial", (a, _) => new hknpMaterial(a) },
            { "hknpMotionProperties", (a, _) => new hknpMotionProperties(a) },
            { "hkRefCountedProperties", (a, _) => new hkRefCountedProperties(a) },
            { "hkRefCountedProperties::Entry", (a, _) => new hkRefCountedProperties.Entry(a) },
            { "hknpShape", (a, _) => new hknpShape(a) },
            { "hknpRefMassDistribution", (a, _) => new hknpRefMassDistribution(a) },
            { "hknpRefDragProperties", (a, _) => new hknpRefDragProperties(a) },
            { "hkLocalFrame", (a, _) => new hkLocalFrame(a) },
            { "hknpBodyCinfo", (a, _) => new hknpBodyCinfo(a) },
            { "hknpPhysicsSystemData::bodyCinfoWithAttachment", (a, _) => new bodyCinfoWithAttachment(a) },
            { "hknpConvexPolytopeShape::Face", (a, _) => new hknpConvexPolytopeShape.Face(a) },
            { "hknpConvexPolytopeShape::Connectivity", (a, _) => new hknpConvexPolytopeShape.Connectivity(a) },
            { "hknpConvexPolytopeShape::Connectivity::Edge", (a, f) => { var ans = new hknpConvexPolytopeShape.Connectivity.Edge(a); ans.Read(f); return ans; } },
            { "hknpCapsuleShape", (a, _) => new hknpCapsuleShape(a) },
            { "hknpCompoundShape::VelocityInfo", (a, _) => new hknpCompoundShape(a) },
            { "hknpCompoundShape", (a, _) => new hknpCompoundShape(a) },
            { "hknpShapeMassProperties", (a, _) => new hknpShapeMassProperties(a) },
            { "hkaSkeletonMapper", (a, _) => new hkaSkeletonMapper(a) },
            { "hkaSkeletonMapperData::SimpleMapping", (a, _) => new hkaSkeletonMapperData.SimpleMapping(a) },

            { "hkcdSimdTree::Node", (a, _) => new hkcdSimdTree.Node(a) },

            { "fsnpCustomMeshParameter::TriangleData", (a, _) => new fsnpCustomMeshParameter.TriangleData(a) },
            { "fsnpCustomMeshParameter::PrimitiveData", (a, _) => new fsnpCustomMeshParameter.PrimitiveData(a) },
            { "fsnpCustomMeshParameter", (a, _) => new fsnpCustomMeshParameter(a) },
            { "fsnpCustomParamCompressedMeshShape", (a, _) => new fsnpCustomParamCompressedMeshShape(a) },
            { "hknpCompressedMeshShapeData", (a, _) => new hknpCompressedMeshShapeData(a) },
            { "hknpConstraintCinfo", (a, _) => new hknpConstraintCinfo(a) },
            { "hkpConstraintData", (a, _) => new hkpConstraintData(a) },
            { "hkpRagdollConstraintData", (a, _) => new hkpRagdollConstraintData(a) },
            { "hkpLimitedHingeConstraintData", (a, _) => new hkpLimitedHingeConstraintData(a) },
            { "hkcdStaticMeshTree::Section", (a, _) => new hkcdStaticMeshTree.Section(a) },
            { "hkcdStaticMeshTree::Primitive", (a, _) => new hkcdStaticMeshTree.Primitive(a) },
            { "hkcdDefaultStaticMeshTree::PrimitiveDataRun", (a, _) => new hkcdDefaultStaticMeshTree.PrimitiveDataRun(a) },
            { "hkcdCompressedAabbCodecs::Aabb5BytesCodec", (a, _) => new hkcdCompressedAabbCodecs.Aabb5BytesCodec(a) },
            { "hkcdCompressedAabbCodecs::Aabb4BytesCodec", (a, _) => new hkcdCompressedAabbCodecs.Aabb4BytesCodec(a) },
            { "hknpCompressedMeshShapeTree", (a, _) => new hkcdDefaultStaticMeshTree(a) },


            { "hkUint8", (a, f) => { var ans = new hkUint8(a); ans.Read(f); return ans; } },
            { "hkInt16", (a, f) => { var ans = new hkInt16(a); ans.Read(f); return ans; } },
            { "hkUint16", (a, f) => { var ans = new hkUint16(a); ans.Read(f); return ans; } },
            { "int", (a, f) => { var ans = new hkInt(a); ans.Read(f); return ans; } },
            { "hkUint32", (a, f) => { var ans = new hkUint32(a); ans.Read(f); return ans; } },
            { "unsigned int", (a, f) => { var ans = new hkUint32(a); ans.Read(f); return ans; } },
            { "hkUint64", (a, f) => { var ans = new hkUint64(a); ans.Read(f); return ans; } },
            { "unsigned long long", (a, f) => { var ans = new hkUint64(a); ans.Read(f); return ans; } },
        };

        public static HashSet<HavokTagType> unkTypes = new();

        /// <summary>
        /// Tries to instantiate the object described by the parameter, returning the parameter
        /// in the case of a failure.
        /// </summary>
        public static object? TryInstantiateObject(HavokTagObject o, HavokFile f)
        {
            if (registry.TryGetValue(o.Type.name, out var cons)) {
                return cons(o, f);
            }
            if (o.Type.name != "T*") unkTypes.Add(o.Type);
            return o;
        }
    }
}
