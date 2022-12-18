using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpMotionProperties : HavokTypeBase
    {
        public uint isExclusive;
        public uint flags;
        public float gravityFactor;
        public float timeFactor;
        public float maxLinearSpeed;
        public float maxAngularSpeed;
        public float linearDamping;
        public float angularDamping;
        public float solverStabilizationSpeedThreshold;
        public float solverStabilizationSpeedReduction;
        public DeactivationSettings deactivationSettings;
        public FullCastSettings fullCastSettings;

        public hknpMotionProperties(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            isExclusive = r.ReadUInt32();
            flags = r.ReadUInt32();
            gravityFactor = r.ReadSingle();
            timeFactor = r.ReadSingle();
            maxLinearSpeed = r.ReadSingle();
            maxAngularSpeed = r.ReadSingle();
            linearDamping = r.ReadSingle();
            angularDamping = r.ReadSingle();
            solverStabilizationSpeedThreshold = r.ReadSingle();
            solverStabilizationSpeedReduction = r.ReadSingle();
            deactivationSettings = new DeactivationSettings(r);
            fullCastSettings = new FullCastSettings(r);
        }
        public struct DeactivationSettings
        {
            public float maxDistSqrd;
            public float maxRotSqrd;
            public float invBlockSize;
            public short pathingUpperThreshold;
            public short pathingLowerThreshold;
            public byte numDeactivationFrequencyPasses;
            public byte deactivationVelocityScaleSquare;
            public byte minimumPathingVelocityScaleSquare;
            public byte spikingVelocityScaleThresholdSquared;
            public byte minimumSpikingVelocityScaleSquared;
            public DeactivationSettings(BinaryReaderEx r)
            {
                maxDistSqrd = r.ReadSingle();
                maxRotSqrd = r.ReadSingle();
                invBlockSize = r.ReadSingle();
                pathingUpperThreshold = r.ReadInt16();
                pathingLowerThreshold = r.ReadInt16();
                numDeactivationFrequencyPasses = r.ReadByte();
                deactivationVelocityScaleSquare = r.ReadByte();
                minimumPathingVelocityScaleSquare = r.ReadByte();
                spikingVelocityScaleThresholdSquared = r.ReadByte();
                minimumSpikingVelocityScaleSquared = r.ReadByte();
                r.Skip(3);
            }
        }
        public struct FullCastSettings
        {
            public float minSeparation;
            public float minExtraSeparation;
            public float toiSeparation;
            public float toiExtraSeparation;
            public float toiAccuracy;
            public float relativeSafeDeltaTime;
            public float absoluteSafeDeltaTime;
            public float keepTime;
            public float keepDistance;
            public int maxIterations;
            public FullCastSettings(BinaryReaderEx r)
            {
                minSeparation = r.ReadSingle();
                minExtraSeparation = r.ReadSingle();
                toiSeparation = r.ReadSingle();
                toiExtraSeparation = r.ReadSingle();
                toiAccuracy = r.ReadSingle();
                relativeSafeDeltaTime = r.ReadSingle();
                absoluteSafeDeltaTime = r.ReadSingle();
                keepTime = r.ReadSingle();
                keepDistance = r.ReadSingle();
                maxIterations = r.ReadInt32();
            }
        }
    }
}
