using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Linq;

namespace SoulsFormats
{
    public partial class MSBE
    {
        internal enum PartType : uint
        {
            MapPiece = 0,
            Object = 13,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            /// <summary>
            /// All of the fixed visual geometry of the map.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Dynamic props and interactive things.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// All non-player characters.
            /// </summary>
            public List<Part.Enemy> Enemies { get; set; }

            /// <summary>
            /// These have something to do with player spawn points.
            /// </summary>
            public List<Part.Player> Players { get; set; }

            /// <summary>
            /// Invisible physical geometry of the map.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// Objects that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyObject> DummyObjects { get; set; }

            /// <summary>
            /// Enemies that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Dummy parts that reference an actual collision and cause it to load another map.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Creates an empty PartsParam with the default version.
            /// </summary>
            public PartsParam() : base(35, "PARTS_PARAM_ST")
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Enemies = new List<Part.Enemy>();
                Players = new List<Part.Player>();
                Collisions = new List<Part.Collision>();
                DummyObjects = new List<Part.DummyObject>();
                DummyEnemies = new List<Part.DummyEnemy>();
                ConnectCollisions = new List<Part.ConnectCollision>();
            }

            /// <summary>
            /// Adds a part to the appropriate list for its type; returns the part.
            /// </summary>
            public Part Add(Part part)
            {
                switch (part)
                {
                    case Part.MapPiece p: MapPieces.Add(p); break;
                    case Part.Object p: Objects.Add(p); break;
                    case Part.Enemy p: Enemies.Add(p); break;
                    case Part.Player p: Players.Add(p); break;
                    case Part.Collision p: Collisions.Add(p); break;
                    case Part.DummyObject p: DummyObjects.Add(p); break;
                    case Part.DummyEnemy p: DummyEnemies.Add(p); break;
                    case Part.ConnectCollision p: ConnectCollisions.Add(p); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {part.GetType()}.", nameof(part));
                }
                return part;
            }
            IMsbPart IMsbParam<IMsbPart>.Add(IMsbPart item) => Add((Part)item);

            /// <summary>
            /// Returns every Part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries()
            {
                return SFUtil.ConcatAll<Part>(
                    MapPieces, Objects, Enemies, Players, Collisions,
                    DummyObjects, DummyEnemies, ConnectCollisions);
            }
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 12);
                return type switch {
                    PartType.MapPiece => MapPieces.EchoAdd(new Part.MapPiece(br)),
                    PartType.Object => Objects.EchoAdd(new Part.Object(br)),
                    PartType.Enemy => Enemies.EchoAdd(new Part.Enemy(br)),
                    PartType.Player => Players.EchoAdd(new Part.Player(br)),
                    PartType.Collision => Collisions.EchoAdd(new Part.Collision(br)),
                    PartType.DummyObject => DummyObjects.EchoAdd(new Part.DummyObject(br)),
                    PartType.DummyEnemy => DummyEnemies.EchoAdd(new Part.DummyEnemy(br)),
                    PartType.ConnectCollision => ConnectCollisions.EchoAdd(new Part.ConnectCollision(br)),
                    _ => throw new NotImplementedException($"Unimplemented part type: {type}"),
                };
            }
        }

        /// <summary>
        /// Common data for all types of part.
        /// </summary>
        public abstract class Part : Entry, IMsbPart
        {
            private protected abstract PartType Type { get; }
            private protected abstract bool HasUnk1 { get; }
            private protected abstract bool HasUnk2 { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }
            private protected abstract bool HasUnk7 { get; }
            private protected abstract bool HasUnk8 { get; }
            private protected abstract bool HasUnk9 { get; }
            private protected abstract bool HasUnk10 { get; }
            private protected abstract bool HasUnk11 { get; }

            /// <summary>
            /// Unknown. Typically appears at the end of the part's name.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The model used by this part; requires an entry in ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            private int ModelIndex;

            /// <summary>
            /// A path to a .sib file, presumably some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part; only works for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk44 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk48 { get; set; }

            /// <summary>
            /// Identifies the part in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE05 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE06 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LanternID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LodParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE09 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsPointLightShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0B { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsStaticShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsCascade3ShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte EnableOnAboveShadow { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE17 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE18 { get; set; }

            /// <summary>
            /// Allows multiple parts to be identified by the same entity ID.
            /// </summary>
            public int[] EntityGroupIDs { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE3C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE40 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] UnkE44 { get; set; }

            private protected Part(string name)
            {
                Name = name;
                ModelName = "";
                SibPath = "";
                Scale = Vector3.One;
                EntityID = -1;
                EntityGroupIDs = new int[8];
                for (int i = 0; i < 8; i++)
                    EntityGroupIDs[i] = -1;
                UnkE44 = new byte[0x10];
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.EntityGroupIDs = (int[])EntityGroupIDs.Clone();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

#pragma warning disable CS8618
            private protected Part(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                Id = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                ModelIndex = br.ReadInt32();
                int modelIndex2 = br.ReadInt32();
                if (modelIndex2 != ModelIndex) ModelIndex = modelIndex2;
                long sibOffset = br.ReadInt64();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                Unk44 = br.ReadInt32();
                Unk48 = br.ReadInt32();
                br.AssertInt32(0);
                long unkOffset1 = br.ReadInt64();
                long unkOffset2 = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGparamOffset = br.ReadInt64();
                long unkOffset7 = br.ReadInt64();
                long unkOffset8 = br.ReadInt64();
                long unkOffset9 = br.ReadInt64();
                long unkOffset10 = br.ReadInt64();
                long unkOffset11 = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (HasUnk1 ^ unkOffset1 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset1)} 0x{unkOffset1:X} in type {GetType()}.");
                if (HasUnk2 ^ unkOffset2 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset2)} 0x{unkOffset2:X} in type {GetType()}.");
                if (entityDataOffset == 0)
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {GetType()}.");
                if (typeDataOffset == 0)
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {GetType()}.");
                if (HasGparamConfig ^ gparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(gparamOffset)} 0x{gparamOffset:X} in type {GetType()}.");
                if (HasSceneGparamConfig ^ sceneGparamOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(sceneGparamOffset)} 0x{sceneGparamOffset:X} in type {GetType()}.");
                if (HasUnk7 ^ unkOffset7 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset7)} 0x{unkOffset7:X} in type {GetType()}.");
                if (HasUnk8 ^ unkOffset8 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset8)} 0x{unkOffset8:X} in type {GetType()}.");
                if (HasUnk9 ^ unkOffset9 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset9)} 0x{unkOffset9:X} in type {GetType()}.");
                if (HasUnk10 ^ unkOffset10 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset10)} 0x{unkOffset10:X} in type {GetType()}.");
                if (HasUnk11 ^ unkOffset11 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset11)} 0x{unkOffset11:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();

                if (HasUnk1)
                {
                    br.Position = start + unkOffset1;
                    ReadUnk1(br);
                }

                if (HasUnk2)
                {
                    br.Position = start + unkOffset2;
                    ReadUnk2(br);
                }

                br.Position = start + entityDataOffset;
                ReadEntityData(br);

                br.Position = start + typeDataOffset;
                ReadTypeData(br);

                if (HasGparamConfig)
                {
                    br.Position = start + gparamOffset;
                    ReadGparamConfig(br);
                }

                if (HasSceneGparamConfig)
                {
                    br.Position = start + sceneGparamOffset;
                    ReadSceneGparamConfig(br);
                }

                if (HasUnk7)
                {
                    br.Position = start + unkOffset7;
                    ReadUnk7(br);
                }

                if (HasUnk8) {
                    br.Position = start + unkOffset8;
                    ReadUnk8(br);
                }

                if (HasUnk9) {
                    br.Position = start + unkOffset9;
                    ReadUnk9(br);
                }

                if (HasUnk10) {
                    br.Position = start + unkOffset10;
                    ReadUnk10(br);
                }

                if (HasUnk11) {
                    br.Position = start + unkOffset11;
                    ReadUnk11(br);
                }
            }
#pragma warning restore CS8618

            private void ReadEntityData(BinaryReaderEx br)
            {
                EntityID = br.ReadInt32();
                UnkE04 = br.ReadByte();
                UnkE05 = br.ReadByte();
                UnkE06 = br.ReadByte();
                LanternID = br.ReadByte();
                LodParamID = br.ReadByte();
                UnkE09 = br.ReadByte();
                IsPointLightShadowSrc = br.ReadByte();
                UnkE0B = br.ReadByte();
                IsShadowSrc = br.ReadBoolean();
                IsStaticShadowSrc = br.ReadByte();
                IsCascade3ShadowSrc = br.ReadByte();
                UnkE0F = br.ReadByte();
                UnkE10 = br.ReadByte();
                IsShadowDest = br.ReadBoolean();
                IsShadowOnly = br.ReadBoolean();
                DrawByReflectCam = br.ReadBoolean();
                DrawOnlyReflectCam = br.ReadBoolean();
                EnableOnAboveShadow = br.ReadByte();
                DisablePointLightEffect = br.ReadBoolean();
                UnkE17 = br.ReadByte();
                UnkE18 = br.ReadInt32();
                EntityGroupIDs = br.ReadInt32s(8);
                UnkE3C = br.ReadInt32();
                UnkE40 = br.ReadInt32();
                UnkE44 = br.ReadBytes(0x10);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadUnk1(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk1)}.");

            private protected virtual void ReadUnk2(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk2)}.");

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            private protected virtual void ReadUnk7(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk7)}.");

            private protected virtual void ReadUnk8(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk8)}.");

            private protected virtual void ReadUnk9(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk9)}.");

            private protected virtual void ReadUnk10(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk10)}.");

            private protected virtual void ReadUnk11(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnk11)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(id);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(ModelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32(Unk44);
                bw.WriteInt32(Unk48);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffset1");
                bw.ReserveInt64("UnkOffset2");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");
                bw.ReserveInt64("UnkOffset7");
                bw.ReserveInt64("UnkOffset8");
                bw.ReserveInt64("UnkOffset9");
                bw.ReserveInt64("UnkOffset10");
                bw.ReserveInt64("UnkOffset11");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                bw.Pad(8);

                if (HasUnk1)
                {
                    bw.FillInt64("UnkOffset1", bw.Position - start);
                    WriteUnk1(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset1", 0);
                }

                if (HasUnk2)
                {
                    bw.FillInt64("UnkOffset2", bw.Position - start);
                    WriteUnk2(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset2", 0);
                }

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                WriteEntityData(bw);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                WriteTypeData(bw);

                if (HasGparamConfig)
                {
                    bw.FillInt64("GparamOffset", bw.Position - start);
                    WriteGparamConfig(bw);
                }
                else
                {
                    bw.FillInt64("GparamOffset", 0);
                }

                if (HasSceneGparamConfig)
                {
                    bw.FillInt64("SceneGparamOffset", bw.Position - start);
                    WriteSceneGparamConfig(bw);
                }
                else
                {
                    bw.FillInt64("SceneGparamOffset", 0);
                }

                if (HasUnk7)
                {
                    bw.FillInt64("UnkOffset7", bw.Position - start);
                    WriteUnk7(bw);
                }
                else
                {
                    bw.FillInt64("UnkOffset7", 0);
                }

                if (HasUnk8) {
                    bw.FillInt64("UnkOffset8", bw.Position - start);
                    WriteUnk8(bw);
                } else {
                    bw.FillInt64("UnkOffset8", 0);
                }

                if (HasUnk9) {
                    bw.FillInt64("UnkOffset9", bw.Position - start);
                    WriteUnk9(bw);
                } else {
                    bw.FillInt64("UnkOffset9", 0);
                }

                if (HasUnk10) {
                    bw.FillInt64("UnkOffset10", bw.Position - start);
                    WriteUnk10(bw);
                } else {
                    bw.FillInt64("UnkOffset10", 0);
                }

                if (HasUnk11) {
                    bw.FillInt64("UnkOffset11", bw.Position - start);
                    WriteUnk11(bw);
                } else {
                    bw.FillInt64("UnkOffset11", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw)
            {
                bw.WriteInt32(EntityID);
                bw.WriteByte(UnkE04);
                bw.WriteByte(UnkE05);
                bw.WriteByte(UnkE06);
                bw.WriteByte(LanternID);
                bw.WriteByte(LodParamID);
                bw.WriteByte(UnkE09);
                bw.WriteByte(IsPointLightShadowSrc);
                bw.WriteByte(UnkE0B);
                bw.WriteBoolean(IsShadowSrc);
                bw.WriteByte(IsStaticShadowSrc);
                bw.WriteByte(IsCascade3ShadowSrc);
                bw.WriteByte(UnkE0F);
                bw.WriteByte(UnkE10);
                bw.WriteBoolean(IsShadowDest);
                bw.WriteBoolean(IsShadowOnly);
                bw.WriteBoolean(DrawByReflectCam);
                bw.WriteBoolean(DrawOnlyReflectCam);
                bw.WriteByte(EnableOnAboveShadow);
                bw.WriteBoolean(DisablePointLightEffect);
                bw.WriteByte(UnkE17);
                bw.WriteInt32(UnkE18);
                bw.WriteInt32s(EntityGroupIDs);
                bw.WriteInt32(UnkE3C);
                bw.WriteInt32(UnkE40);
                bw.WriteBytes(UnkE44);
                bw.Pad(8);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteUnk1(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk1)}.");

            private protected virtual void WriteUnk2(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk2)}.");

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            private protected virtual void WriteUnk7(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk7)}.");

            private protected virtual void WriteUnk8(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk8)}.");

            private protected virtual void WriteUnk9(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk9)}.");

            private protected virtual void WriteUnk10(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk10)}.");

            private protected virtual void WriteUnk11(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnk11)}.");

            internal virtual void GetNames(MSBE msb, Entries entries)
            {
                ModelName = MSB.FindName(entries.Models, ModelIndex);
            }

            internal virtual void GetIndices(MSBE msb, Entries entries)
            {
                ModelIndex = MSB.FindIndex(entries.Models, ModelName);
            }

            /// <summary>
            /// Returns the type and name of the part as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct1
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] CollisionMask { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Condition1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Condition2 { get; set; }

                /// <summary>
                /// Creates an UnkStruct1 with default values.
                /// </summary>
                public UnkStruct1()
                {
                    CollisionMask = new uint[48];
                    Condition1 = 0;
                    Condition2 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct1 DeepCopy()
                {
                    var unk1 = (UnkStruct1)MemberwiseClone();
                    unk1.CollisionMask = (uint[])CollisionMask.Clone();
                    return unk1;
                }

                internal UnkStruct1(BinaryReaderEx br)
                {
                    CollisionMask = br.ReadUInt32s(48);
                    Condition1 = br.ReadByte();
                    Condition2 = br.ReadByte();
                    br.ReadInt16();
                    //br.AssertInt16(0);
                    for (int i = 0; i < 0xC0; i++) br.ReadByte();
                    //br.AssertPattern(0xC0, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteUInt32s(CollisionMask);
                    bw.WriteByte(Condition1);
                    bw.WriteByte(Condition2);
                    bw.WriteInt16(0);
                    bw.WritePattern(0xC0, 0x00);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    string cmask = CollisionMask.Aggregate("", (a, b) => a +", "+ b)[2..];
                    return $"{Condition1}; {Condition2}; [{cmask}]";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct2
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Condition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int[] DispGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk26 { get; set; }

                /// <summary>
                /// Creates an UnkStruct2 with default values.
                /// </summary>
                public UnkStruct2()
                {
                    DispGroups = new int[8];
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct2 DeepCopy()
                {
                    var unk2 = (UnkStruct2)MemberwiseClone();
                    unk2.DispGroups = (int[])DispGroups.Clone();
                    return unk2;
                }

                internal UnkStruct2(BinaryReaderEx br)
                {
                    Condition = br.ReadInt32();
                    DispGroups = br.ReadInt32s(8);
                    Unk24 = br.ReadInt16();
                    Unk26 = br.ReadInt16();
                    br.AssertPattern(0x20, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Condition);
                    bw.WriteInt32s(DispGroups);
                    bw.WriteInt16(Unk24);
                    bw.WriteInt16(Unk26);
                    bw.WritePattern(0x20, 0x00);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    string dgroups = DispGroups.Aggregate("", (a, b) => a + ", " + b)[2..];
                    return $"{Condition}; {Unk24}; {Unk26}; [{DispGroups}]";
                }
            }

            /// <summary>
            /// Gparam value IDs for various part types.
            /// </summary>
            public class GparamConfig
            {

                /// <summary>
                /// ID of the value set from LightSet ParamEditor to use.
                /// </summary>
                public int LightSetID { get; set; }

                /// <summary>
                /// ID of the value set from FogParamEditor to use.
                /// </summary>
                public int FogParamID { get; set; }

                /// <summary>
                /// ID of the value set from LightScattering : ParamEditor to use.
                /// </summary>
                public int LightScatteringID { get; set; }

                /// <summary>
                /// ID of the value set from Env Map:Editor to use.
                /// </summary>
                public int EnvMapID { get; set; }

                /// <summary>
                /// Creates a GparamConfig with default values.
                /// </summary>
                public GparamConfig() { }

                /// <summary>
                /// Creates a deep copy of the gparam config.
                /// </summary>
                public GparamConfig DeepCopy()
                {
                    return (GparamConfig)MemberwiseClone();
                }

                internal GparamConfig(BinaryReaderEx br)
                {
                    LightSetID = br.ReadInt32();
                    FogParamID = br.ReadInt32();
                    LightScatteringID = br.ReadInt32();
                    EnvMapID = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(LightSetID);
                    bw.WriteInt32(FogParamID);
                    bw.WriteInt32(LightScatteringID);
                    bw.WriteInt32(EnvMapID);
                    bw.WritePattern(0x10, 0x00);
                }

                /// <summary>
                /// Returns the four gparam values as a string.
                /// </summary>
                public override string ToString()
                {
                    return $"{LightSetID}, {FogParamID}, {LightScatteringID}, {EnvMapID}";
                }
            }

            /// <summary>
            /// Unknown; sceneGParam Struct according to Pav.
            /// </summary>
            public class SceneGparamConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte[] EventIDs { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk40 { get; set; }

                /// <summary>
                /// Creates a SceneGparamConfig with default values.
                /// </summary>
                public SceneGparamConfig()
                {
                    Unk00 = new byte[0x3C];
                    EventIDs = new sbyte[4];
                }

                /// <summary>
                /// Creates a deep copy of the scene gparam config.
                /// </summary>
                public SceneGparamConfig DeepCopy()
                {
                    var config = (SceneGparamConfig)MemberwiseClone();
                    config.EventIDs = (sbyte[])EventIDs.Clone();
                    return config;
                }

                internal SceneGparamConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadBytes(0x3c);
                    EventIDs = br.ReadSBytes(4);
                    Unk40 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Unk00);
                    bw.WriteSBytes(EventIDs);
                    bw.WriteSingle(Unk40);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    string unk00 = Unk00.Aggregate("", (a, b) => a + ", " + b)[2..];
                    return $"EventID[{EventIDs[0],2}][{EventIDs[1],2}][{EventIDs[2],2}][{EventIDs[3],2}] {Unk40:0.0} [{unk00}]";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct7
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// ID in GrassTypeParam determining properties of dynamic grass on a map piece.
                /// </summary>
                public int GrassTypeParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct7() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct7 DeepCopy()
                {
                    return (UnkStruct7)MemberwiseClone();
                }

                internal UnkStruct7(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    GrassTypeParamID = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(GrassTypeParamID);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(0);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00} {Unk04} {GrassTypeParamID} {Unk0C} {Unk10} {Unk14} {Unk18}";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct8
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates an UnkStruct8 with default values.
                /// </summary>
                public UnkStruct8() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct8 DeepCopy()
                {
                    return (UnkStruct8)MemberwiseClone();
                }

                internal UnkStruct8(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00} {Unk04} {Unk08} {Unk0C} {Unk10} {Unk14} {Unk18} {Unk1C}";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct9
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates an UnkStruct9 with default values.
                /// </summary>
                public UnkStruct9() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct9 DeepCopy()
                {
                    return (UnkStruct9)MemberwiseClone();
                }

                internal UnkStruct9(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00} {Unk04} {Unk08} {Unk0C} {Unk10} {Unk14} {Unk18} {Unk1C}";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct10
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates an UnkStruct10 with default values.
                /// </summary>
                public UnkStruct10() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct10 DeepCopy()
                {
                    return (UnkStruct10)MemberwiseClone();
                }

                internal UnkStruct10(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00} {Unk04} {Unk08} {Unk0C} {Unk10} {Unk14} {Unk18} {Unk1C}";
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct11
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Creates an UnkStruct11 with default values.
                /// </summary>
                public UnkStruct11() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct11 DeepCopy()
                {
                    return (UnkStruct11)MemberwiseClone();
                }

                internal UnkStruct11(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString()
                {
                    return $"{Unk00} {Unk04} {Unk08} {Unk0C} {Unk10} {Unk14} {Unk18} {Unk1C}";
                }
            }

            /// <summary>
            /// Fixed visual geometry.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => true;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => true;
                private protected override bool HasUnk10 => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct7 Unk7 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct9 Unk9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct10 Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int T00 { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                public int T04 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    Gparam = new GparamConfig();
                    Unk7 = new UnkStruct7();
                    Unk8 = new UnkStruct8();
                    Unk9 = new UnkStruct9();
                    Unk10 = new UnkStruct10();
                    Unk11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.Unk1 = Unk1.DeepCopy();
                    piece.Unk2 = Unk2.DeepCopy();
                    piece.Gparam = Gparam.DeepCopy();
                    piece.Unk7 = Unk7.DeepCopy();
                    piece.Unk8 = Unk8.DeepCopy();
                    piece.Unk9 = Unk9.DeepCopy();
                    piece.Unk10 = Unk10.DeepCopy();
                    piece.Unk11 = Unk11.DeepCopy();
                }

#pragma warning disable CS8618
                internal MapPiece(BinaryReaderEx br) : base(br) { }
#pragma warning restore CS8618

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    T00 = br.ReadInt32();
                    T04 = br.ReadInt32();
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadUnk7(BinaryReaderEx br) => Unk7 = new UnkStruct7(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => Unk9 = new UnkStruct9(br);
                private protected override void ReadUnk10(BinaryReaderEx br) => Unk10 = new UnkStruct10(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(T00);
                    bw.WriteInt32(T04);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteUnk7(BinaryWriterEx bw) => Unk7.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => Unk9.Write(bw);
                private protected override void WriteUnk10(BinaryWriterEx bw) => Unk10.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);
            }

            /// <summary>
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part
            {
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasUnk10 => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct10 Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }
                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Reference to a map piece or collision; believed to determine when the object is loaded.
                /// </summary>
                public string ObjPartName1 { get; set; }
                private int ObjPartIndex1;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte BreakTerm { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte NetSyncType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte SetMainObjStructureBooleans { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short AnimID { get; set; }


                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT1A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Reference to a collision; believed to be involved with loading when grappling to the object.
                /// </summary>
                public string ObjPartName2 { get; set; }
                private int ObjPartIndex2;

                /// <summary>
                /// Reference to a collision; believed to be involved with loading when grappling to the object.
                /// </summary>
                public string ObjPartName3 { get; set; }
                private int ObjPartIndex3;

                private protected ObjectBase() : base("oXXXXXX_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Gparam = new GparamConfig();
                    Unk8 = new UnkStruct8();
                    Unk10 = new UnkStruct10();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var obj = (ObjectBase)part;
                    obj.Unk1 = Unk1.DeepCopy();
                    obj.Gparam = Gparam.DeepCopy();
                    obj.Unk8 = Unk8.DeepCopy();
                    obj.Unk10 = Unk10.DeepCopy();
                }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    ObjPartIndex1 = br.ReadInt32();
                    BreakTerm = br.ReadByte();
                    NetSyncType = br.ReadByte();
                    UnkT0E = br.ReadByte();
                    SetMainObjStructureBooleans = br.ReadByte();
                    AnimID = br.ReadInt16();
                    UnkT12 = br.ReadInt16();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt16();
                    UnkT1A = br.ReadInt16();
                    UnkT1C = br.ReadInt32();
                    ObjPartIndex2 = br.ReadInt32();
                    ObjPartIndex3 = br.ReadInt32();
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);

                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk10(BinaryReaderEx br) => Unk10 = new UnkStruct10(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(ObjPartIndex1);
                    bw.WriteByte(BreakTerm);
                    bw.WriteByte(NetSyncType);
                    bw.WriteByte(UnkT0E);
                    bw.WriteByte(SetMainObjStructureBooleans);
                    bw.WriteInt16(AnimID);
                    bw.WriteInt16(UnkT12);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt16(UnkT18);
                    bw.WriteInt16(UnkT1A);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(ObjPartIndex2);
                    bw.WriteInt32(ObjPartIndex3);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk10(BinaryWriterEx bw) => Unk10.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    ObjPartName1 = MSB.FindName(entries.Parts, ObjPartIndex1);
                    ObjPartName2 = MSB.FindName(entries.Parts, ObjPartIndex2);
                    ObjPartName3 = MSB.FindName(entries.Parts, ObjPartIndex3);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    ObjPartIndex1 = MSB.FindIndex(entries.Parts, ObjPartName1);
                    ObjPartIndex2 = MSB.FindIndex(entries.Parts, ObjPartName2);
                    ObjPartIndex3 = MSB.FindIndex(entries.Parts, ObjPartName3);
                }
            }

            /// <summary>
            /// A dynamic or interactible element in the map.
            /// </summary>
            public class Object : ObjectBase
            {
                private protected override PartType Type => PartType.Object;
                private protected override bool HasUnk2 => true;
                private protected override bool HasUnk7 => true;
                private protected override bool HasUnk9 => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }


                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct7 Unk7 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct9 Unk9 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base()
                {
                    Unk2 = new UnkStruct2();
                    Unk7 = new UnkStruct7();
                    Unk9 = new UnkStruct9();
                    Unk11 = new UnkStruct11();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                    var obj = (Object)part;
                    obj.Unk2 = Unk2.DeepCopy();
                    obj.Unk7 = Unk7.DeepCopy();
                    obj.Unk9 = Unk9.DeepCopy();
                    obj.Unk11 = Unk11.DeepCopy();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadUnk7(BinaryReaderEx br) => Unk7 = new UnkStruct7(br);
                private protected override void ReadUnk9(BinaryReaderEx br) => Unk9 = new UnkStruct9(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteUnk2(BinaryWriterEx bw) => Unk2.Write(bw);
                private protected override void WriteUnk7(BinaryWriterEx bw) => Unk7.Write(bw);
                private protected override void WriteUnk9(BinaryWriterEx bw) => Unk9.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part
            {
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasUnk10 => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct10 Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Unknown; previously talk ID, now always 0 or 1 except for the Memorial Mob in Senpou.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short PlatoonID { get; set; }

                /// <summary>
                /// An ID in CharaInitParam that determines a human's inventory and stats.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Should reference the collision the enemy starts on.
                /// </summary>
                public string CollisionPartName { get; set; }
                private int CollisionPartIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT22 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagCompareState { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT54 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT58 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT5C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] UnkT60 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT78 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT7C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT80 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT84 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public (int, short, short)[] UnkT88 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] UnkTB0 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX")
                {
                    Gparam = new GparamConfig();
                    Unk1 = new UnkStruct1();
                    Unk8 = new UnkStruct8();
                    Unk10 = new UnkStruct10();
                    ThinkParamID = -1;
                    NPCParamID = -1;
                    UnkT10 = -1;
                    CharaInitID = -1;
                    BackupEventAnimID = -1;
                    EventFlagID = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (EnemyBase)part;
                    enemy.Unk1 = Unk1.DeepCopy();
                    enemy.Gparam = Gparam.DeepCopy();
                    enemy.Unk8 = Unk8.DeepCopy();
                    enemy.Unk10 = Unk10.DeepCopy();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    ThinkParamID = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt16();
                    PlatoonID = br.ReadInt16();
                    CharaInitID = br.ReadInt32();
                    CollisionPartIndex = br.ReadInt32();
                    UnkT20 = br.ReadInt16();
                    UnkT22 = br.ReadInt16();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadBytes(0x10);
                    BackupEventAnimID = br.ReadInt32();
                    UnkT3C = br.ReadInt32();
                    EventFlagID = br.ReadInt32();
                    EventFlagCompareState = br.ReadInt32();
                    UnkT48 = br.ReadInt32();
                    UnkT4C = br.ReadInt32();
                    UnkT50 = br.ReadInt32();
                    UnkT54 = br.ReadInt32();
                    UnkT58 = br.ReadInt32();
                    UnkT5C = br.ReadInt32();
                    UnkT60 = br.ReadBytes(0x18);
                    UnkT78 = br.ReadInt32();
                    UnkT7C = br.ReadInt32();
                    UnkT80 = br.ReadInt32();
                    UnkT84 = br.ReadSingle();
                    UnkT88 = new (int, short, short)[5];
                    for (int i = 0; i < 5; i++)
                    {
                        UnkT88[i] = (br.ReadInt32(), br.ReadInt16(), br.ReadInt16());
                    }
                    UnkTB0 = br.ReadBytes(0x10);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk10(BinaryReaderEx br) => Unk10 = new UnkStruct10(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(ThinkParamID);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt16(UnkT14);
                    bw.WriteInt16(PlatoonID);
                    bw.WriteInt32(CharaInitID);
                    bw.WriteInt32(CollisionPartIndex);
                    bw.WriteInt16(UnkT20);
                    bw.WriteInt16(UnkT22);
                    bw.WriteInt32(UnkT24);
                    bw.WriteBytes(UnkT28);
                    bw.WriteInt32(BackupEventAnimID);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32(EventFlagID);
                    bw.WriteInt32(EventFlagCompareState);
                    bw.WriteInt32(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteInt32(UnkT50);
                    bw.WriteInt32(UnkT54);
                    bw.WriteInt32(UnkT58);
                    bw.WriteInt32(UnkT5C);
                    bw.WriteBytes(UnkT60);
                    bw.WriteInt32(UnkT78);
                    bw.WriteInt32(UnkT7C);
                    bw.WriteInt32(UnkT80);
                    bw.WriteSingle(UnkT84);
                    foreach (var (a, b, c) in UnkT88) {
                        bw.WriteInt32(a);
                        bw.WriteInt16(b);
                        bw.WriteInt16(c);
                    }
                    bw.WriteBytes(UnkTB0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk10(BinaryWriterEx bw) => Unk10.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionPartName = MSB.FindName(entries.Parts, CollisionPartIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionPartIndex = MSB.FindIndex(entries.Parts, CollisionPartName);
                }
            }

            /// <summary>
            /// Any non-player character.
            /// </summary>
            public class Enemy : EnemyBase
            {
                private protected override PartType Type => PartType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base() {}

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

            }

            /// <summary>
            /// A spawn point for the player, or something.
            /// </summary>
            public class Player : Part
            {
                private protected override PartType Type => PartType.Player;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasUnk10 => true;
                private protected override bool HasUnk11 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct10 Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] UnkT00 { get; set; }

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX") {
                    Unk1 = new UnkStruct1();
                    Unk8 = new UnkStruct8();
                    Unk10 = new UnkStruct10();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                    var enemy = (Player)part;
                    enemy.Unk1 = Unk1.DeepCopy();
                    enemy.Unk8 = Unk8.DeepCopy();
                    enemy.Unk10 = Unk10.DeepCopy();
                }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk10(BinaryReaderEx br) => Unk10 = new UnkStruct10(br);

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadBytes(0x10);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(UnkT00);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk10(BinaryWriterEx bw) => Unk10.Write(bw);
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part
            {
                private protected override PartType Type => PartType.Collision;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;
                private protected override bool HasUnk7 => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasUnk10 => true;
                private protected override bool HasUnk11 => true;


                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfig SceneGparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct10 Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID { get; set; }

                /// <summary>
                /// Adds reverb to sounds while on this collision to simulate echoes.
                /// </summary>
                public byte SoundSpaceType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Determines the text to display for map popups and save files.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte DisableStart { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT17 { get; set; }

                /// <summary>
                /// If not -1, the bonfire with this ID will be disabled when enemies are on this collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT25 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT26 { get; set; }

                /// <summary>
                /// Should alter visibility while on this collision, but doesn't seem to do much.
                /// </summary>
                public byte MapVisibility { get; set; }

                /// <summary>
                /// Used to determine invasion eligibility.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Alters camera properties while on this collision.
                /// </summary>
                public short LockCamParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT2E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT54 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT58 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT5C { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX")
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    Gparam = new GparamConfig();
                    SceneGparam = new SceneGparamConfig();
                    Unk8 = new UnkStruct8();
                    Unk10 = new UnkStruct10();
                    Unk11 = new UnkStruct11();
                    DisableBonfireEntityID = -1;
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var collision = (Collision)part;
                    collision.Unk1 = Unk1.DeepCopy();
                    collision.Unk2 = Unk2.DeepCopy();
                    collision.Gparam = Gparam.DeepCopy();
                    collision.SceneGparam = SceneGparam.DeepCopy();
                    collision.Unk8 = Unk8.DeepCopy();
                    collision.Unk10 = Unk10.DeepCopy();
                    collision.Unk11 = Unk11.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    HitFilterID = br.ReadByte(); // Pav says Type, did it change?
                    SoundSpaceType = br.ReadByte();
                    UnkT02 = br.ReadInt16();
                    ReflectPlaneHeight = br.ReadSingle();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    MapNameID = br.ReadInt16();
                    DisableStart = br.ReadByte();
                    UnkT17 = br.ReadByte();
                    DisableBonfireEntityID = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadByte();
                    UnkT25 = br.ReadByte();
                    UnkT26 = br.ReadByte();
                    MapVisibility = br.ReadByte();
                    PlayRegionID = br.ReadInt32();
                    LockCamParamID = br.ReadInt16();
                    UnkT2E = br.ReadInt16();
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt32();
                    UnkT38 = br.ReadInt32();
                    UnkT3C = br.ReadInt32();
                    UnkT40 = br.ReadInt32();
                    UnkT44 = br.ReadSingle();
                    UnkT48 = br.ReadSingle();
                    UnkT4C = br.ReadInt32();
                    UnkT50 = br.ReadSingle();
                    UnkT54 = br.ReadSingle();
                    UnkT58 = br.ReadInt32();
                    UnkT5C = br.ReadInt32();
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => Gparam = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => SceneGparam = new SceneGparamConfig(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk10(BinaryReaderEx br) => Unk10 = new UnkStruct10(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(HitFilterID);
                    bw.WriteByte(SoundSpaceType);
                    bw.WriteInt16(UnkT02);
                    bw.WriteSingle(ReflectPlaneHeight);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt16(MapNameID);
                    bw.WriteByte(DisableStart);
                    bw.WriteByte(UnkT17);
                    bw.WriteInt32(DisableBonfireEntityID);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(UnkT20);
                    bw.WriteByte(UnkT24);
                    bw.WriteByte(UnkT25);
                    bw.WriteByte(UnkT26);
                    bw.WriteByte(MapVisibility);
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(LockCamParamID);
                    bw.WriteInt16(UnkT2E);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(UnkT38);
                    bw.WriteInt32(UnkT3C);
                    bw.WriteInt32(UnkT40);
                    bw.WriteSingle(UnkT44);
                    bw.WriteSingle(UnkT48);
                    bw.WriteInt32(UnkT4C);
                    bw.WriteSingle(UnkT50);
                    bw.WriteSingle(UnkT54);
                    bw.WriteInt32(UnkT58);
                    bw.WriteInt32(UnkT5C);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => Unk2.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => Gparam.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => SceneGparam.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk10(BinaryWriterEx bw) => Unk10.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);
            }

            /// <summary>
            /// An object that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase
            {
                private protected override PartType Type => PartType.DummyObject;

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                }



                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() {
                }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase
            {
                private protected override PartType Type => PartType.DummyEnemy;

                private protected override void DeepCopyTo(Part part)
                {
                    base.DeepCopyTo(part);
                }

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base()
                {
                }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// References an actual collision and causes another map to be loaded while on it.
            /// </summary>
            public class ConnectCollision : Part
            {
                private protected override PartType Type => PartType.ConnectCollision;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;
                private protected override bool HasUnk8 => true;
                private protected override bool HasUnk9 => false;
                private protected override bool HasUnk10 => true;
                private protected override bool HasUnk11 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct8 Unk8 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct10 Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct11 Unk11 { get; set; }

                /// <summary>
                /// The collision part to attach to.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX")
                {
                    Unk1 = new UnkStruct1();
                    Unk2 = new UnkStruct2();
                    Unk8 = new UnkStruct8();
                    Unk10 = new UnkStruct10();
                    Unk11 = new UnkStruct11();
                    MapID = new byte[4];
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var connect = (ConnectCollision)part;
                    connect.Unk1 = Unk1.DeepCopy();
                    connect.Unk2 = Unk2.DeepCopy();
                    connect.Unk8 = Unk8.DeepCopy();
                    connect.Unk10 = Unk10.DeepCopy();
                    connect.Unk11 = Unk11.DeepCopy();
                    connect.MapID = (byte[])MapID.Clone();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CollisionIndex = br.ReadInt32();
                    MapID = br.ReadBytes(4);
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => Unk2 = new UnkStruct2(br);
                private protected override void ReadUnk8(BinaryReaderEx br) => Unk8 = new UnkStruct8(br);
                private protected override void ReadUnk10(BinaryReaderEx br) => Unk10 = new UnkStruct10(br);
                private protected override void ReadUnk11(BinaryReaderEx br) => Unk11 = new UnkStruct11(br);

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CollisionIndex);
                    bw.WriteBytes(MapID);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => Unk1.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => Unk2.Write(bw);
                private protected override void WriteUnk8(BinaryWriterEx bw) => Unk8.Write(bw);
                private protected override void WriteUnk10(BinaryWriterEx bw) => Unk10.Write(bw);
                private protected override void WriteUnk11(BinaryWriterEx bw) => Unk11.Write(bw);

                internal override void GetNames(MSBE msb, Entries entries)
                {
                    base.GetNames(msb, entries);
                    CollisionName = MSB.FindName(msb.Parts.Collisions, CollisionIndex);
                }

                internal override void GetIndices(MSBE msb, Entries entries)
                {
                    base.GetIndices(msb, entries);
                    CollisionIndex = MSB.FindIndex(msb.Parts.Collisions, CollisionName);
                }
            }

        }
    }
}
