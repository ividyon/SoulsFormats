﻿using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// A texture used by the shader specified in an MTD.
        /// </summary>
        public class Texture
        {
            /// <summary>
            /// The type of texture this is, corresponding to the entries in the MTD.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Network path to the texture file to use.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector2 Scale { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk11 { get; set; }

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
            /// Creates a Texture with default values.
            /// </summary>
            public Texture()
            {
                Type = "";
                Path = "";
                Scale = Vector2.One;
            }

            /// <summary>
            /// Creates a new Texture with the specified values.
            /// </summary>
            public Texture(string type, string path, Vector2 scale, byte unk10, bool unk11, int unk14, int unk18, int unk1C)
            {
                Type = type;
                Path = path;
                Scale = scale;
                Unk10 = unk10;
                Unk11 = unk11;
                Unk14 = unk14;
                Unk18 = unk18;
                Unk1C = unk1C;
            }

            internal Texture(BinaryReaderEx br, FLVERHeader header)
            {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                Scale = br.ReadVector2();

                Unk10 = br.AssertByte(0, 1, 2);
                Unk11 = br.ReadBoolean();
                br.AssertByte(0);
                br.AssertByte(0);

                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                if (header.Unicode)
                {
                    Type = br.GetUTF16(typeOffset);
                    Path = br.GetUTF16(pathOffset);
                }
                else
                {
                    Type = br.GetShiftJIS(typeOffset);
                    Path = br.GetShiftJIS(pathOffset);
                }
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"TexturePath{index}");
                bw.ReserveInt32($"TextureType{index}");
                bw.WriteVector2(Scale);

                bw.WriteByte(Unk10);
                bw.WriteBoolean(Unk11);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            internal void WriteStrings(BinaryWriterEx bw, FLVERHeader header, int index)
            {
                bw.FillInt32($"TexturePath{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(Path, true);
                else
                    bw.WriteShiftJIS(Path, true);

                bw.FillInt32($"TextureType{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(Type, true);
                else
                    bw.WriteShiftJIS(Type, true);
            }

            /// <summary>
            /// Returns this texture's type and path.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} = {Path}";
            }
        }
    }
}
