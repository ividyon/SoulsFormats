﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A font layout file used in DeS, DS1, DS2, and DS3; determines the texture used for each different character code.
    /// </summary>
    public class CCM : SoulsFile<CCM>
    {
        /// <summary>
        /// Indicates the game this CCM should be formatted for.
        /// </summary>
        public CCMVer Version { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk08 { get; set; }

        /// <summary>
        /// Width of the font textures.
        /// </summary>
        public short TexWidth { get; set; }

        /// <summary>
        /// Height of the font textures.
        /// </summary>
        public short TexHeight { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk0E { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk1C { get; set; }

        /// <summary>
        /// Number of separate font textures.
        /// </summary>
        public short TexCount { get; set; }

        /// <summary>
        /// Individual characters in this font.
        /// </summary>
        public List<Glyph> Glyphs { get; set; }

        internal override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        internal override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            Version = br.ReadEnum32<CCMVer>();
            if (Version == CCMVer.DemonsSouls)
                br.BigEndian = true;

            int fileSize = br.ReadInt32();
            Unk08 = br.ReadInt16();
            TexWidth = br.ReadInt16();
            TexHeight = br.ReadInt16();

            short codeGroupCount, texRegionCount, glyphCount;
            if (Version == CCMVer.DemonsSouls || Version == CCMVer.DarkSouls1)
            {
                Unk0E = br.ReadInt16();
                codeGroupCount = br.ReadInt16();
                texRegionCount = -1;
                glyphCount = br.ReadInt16();
            }
            else
            {
                Unk0E = 0;
                codeGroupCount = -1;
                texRegionCount = br.ReadInt16();
                glyphCount = br.ReadInt16();
                br.AssertInt16(0);
            }

            br.AssertInt32(0x20);
            int glyphOffset = br.ReadInt32();
            Unk1C = br.ReadInt16();
            TexCount = br.ReadInt16();

            Glyphs = new List<Glyph>(glyphCount);
            if (Version == CCMVer.DemonsSouls || Version == CCMVer.DarkSouls1)
            {
                var codeGroups = new List<CodeGroup>(codeGroupCount);
                for (int i = 0; i < codeGroupCount; i++)
                    codeGroups.Add(new CodeGroup(br));

                for (int i = 0; i < glyphCount; i++)
                {
                    Vector2 uv1 = br.ReadVector2();
                    Vector2 uv2 = br.ReadVector2();
                    short preSpace = br.ReadInt16();
                    short width = br.ReadInt16();
                    short advance = br.ReadInt16();
                    short texIndex = br.ReadInt16();

                    Glyphs.Add(new Glyph(-1, uv1, uv2, preSpace, width, advance, texIndex));
                }

                foreach (CodeGroup group in codeGroups)
                {
                    int codeCount = group.EndCode - group.StartCode + 1;
                    for (int i = 0; i < codeCount; i++)
                        Glyphs[group.GlyphIndex + i].Code = group.StartCode + i;
                }
            }
            else if (Version == CCMVer.DarkSouls2)
            {
                var texRegions = new Dictionary<int, TexRegion>(texRegionCount);
                for (int i = 0; i < texRegionCount; i++)
                    texRegions[(int)br.Position] = new TexRegion(br);

                for (int i = 0; i < glyphCount; i++)
                {
                    int code = br.ReadInt32();
                    int texRegionOffset = br.ReadInt32();
                    short texIndex = br.ReadInt16();
                    short preSpace = br.ReadInt16();
                    short width = br.ReadInt16();
                    short advance = br.ReadInt16();
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    TexRegion texRegion = texRegions[texRegionOffset];
                    Vector2 uv1 = new Vector2(texRegion.X1 / (float)TexWidth, texRegion.Y1 / (float)TexHeight);
                    Vector2 uv2 = new Vector2(texRegion.X2 / (float)TexWidth, texRegion.Y2 / (float)TexHeight);
                    Glyphs.Add(new Glyph(code, uv1, uv2, preSpace, width, advance, texIndex));
                }
            }
        }

        internal override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteUInt32((uint)Version);
            bw.BigEndian = Version == CCMVer.DemonsSouls;

            bw.ReserveInt32("FileSize");
            bw.WriteInt16(Unk08);
            bw.WriteInt16(TexWidth);
            bw.WriteInt16(TexHeight);

            if (Version == CCMVer.DemonsSouls || Version == CCMVer.DarkSouls1)
            {
                bw.WriteInt16(Unk0E);
                bw.ReserveInt16("CodeGroupCount");
                bw.WriteInt16((short)Glyphs.Count);
            }
            else if (Version == CCMVer.DarkSouls2)
            {
                bw.ReserveInt16("TexRegionCount");
                bw.WriteInt16((short)Glyphs.Count);
                bw.WriteInt16(0);
            }

            bw.WriteInt32(0x20);
            bw.ReserveInt32("GlyphOffset");
            bw.WriteInt16(Unk1C);
            bw.WriteInt16(TexCount);

            Glyphs.Sort();
            if (Version == CCMVer.DemonsSouls || Version == CCMVer.DarkSouls1)
            {
                var codeGroups = new List<CodeGroup>();
                for (int i = 0; i < Glyphs.Count;)
                {
                    int startCode = Glyphs[i].Code;
                    int glyphIndex = i;
                    for (i++; i < Glyphs.Count && Glyphs[i].Code == Glyphs[i - 1].Code + 1; i++) ;
                    int endCode = Glyphs[i - 1].Code;
                    codeGroups.Add(new CodeGroup(startCode, endCode, glyphIndex));
                }

                bw.FillInt16("CodeGroupCount", (short)codeGroups.Count);
                foreach (CodeGroup group in codeGroups)
                    group.Write(bw);

                bw.FillInt32("GlyphOffset", (int)bw.Position);
                foreach (Glyph glyph in Glyphs)
                {
                    bw.WriteVector2(glyph.UV1);
                    bw.WriteVector2(glyph.UV2);
                    bw.WriteInt16(glyph.PreSpace);
                    bw.WriteInt16(glyph.Width);
                    bw.WriteInt16(glyph.Advance);
                    bw.WriteInt16(glyph.TexIndex);
                }
            }
            else if (Version == CCMVer.DarkSouls2)
            {
                throw new NotImplementedException();
            }

            bw.FillInt32("FileSize", (int)bw.Position);
            if (Version == CCMVer.DemonsSouls)
                bw.Pad(0x20);
        }

        // This is stupid because it's really two shorts but I am lazy
        /// <summary>
        /// Which game the CCM should be formatted for.
        /// </summary>
        public enum CCMVer : uint
        {
            /// <summary>
            /// Demon's Souls
            /// </summary>
            DemonsSouls = 0x100,

            /// <summary>
            /// Dark Souls 1
            /// </summary>
            DarkSouls1 = 0x10001,

            /// <summary>
            /// Dark Souls 2 and Dark Souls 3
            /// </summary>
            DarkSouls2 = 0x20000,
        }

        /// <summary>
        /// An individual character in the font.
        /// </summary>
        public class Glyph : IComparable<Glyph>
        {
            /// <summary>
            /// The character code.
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// The UV of the top-left corner of the texture.
            /// </summary>
            public Vector2 UV1 { get; set; }

            /// <summary>
            /// The UV of the bottom-right corner of the texture.
            /// </summary>
            public Vector2 UV2 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short PreSpace { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Width { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Advance { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short TexIndex { get; set; }

            /// <summary>
            /// Creates a new Glyph with the given values.
            /// </summary>
            public Glyph(int code, Vector2 uv1, Vector2 uv2, short preSpace, short width, short advance, short texIndex)
            {
                Code = code;
                UV1 = uv1;
                UV2 = uv2;
                PreSpace = preSpace;
                Width = width;
                Advance = advance;
                TexIndex = texIndex;
            }

            /// <summary>
            /// Compares this Glyph's Code to the other's.
            /// </summary>
            public int CompareTo(Glyph other) => Code.CompareTo(other.Code);
        }

        private struct CodeGroup
        {
            public int StartCode, EndCode;
            public int GlyphIndex;

            public CodeGroup(BinaryReaderEx br)
            {
                StartCode = br.ReadInt32();
                EndCode = br.ReadInt32();
                GlyphIndex = br.ReadInt32();
            }

            public CodeGroup(int startCode, int endCode, int glyphIndex)
            {
                StartCode = startCode;
                EndCode = endCode;
                GlyphIndex = glyphIndex;
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(StartCode);
                bw.WriteInt32(EndCode);
                bw.WriteInt32(GlyphIndex);
            }
        }

        private struct TexRegion : IEquatable<TexRegion>
        {
            public short X1, Y1;
            public short X2, Y2;

            public TexRegion(BinaryReaderEx br)
            {
                X1 = br.ReadInt16();
                Y1 = br.ReadInt16();
                X2 = br.ReadInt16();
                Y2 = br.ReadInt16();
            }

            public bool Equals(TexRegion other)
            {
                return X1 == other.X1 && Y1 == other.Y1 && X2 == other.X2 && Y2 == other.Y2;
            }
        }
    }
}
