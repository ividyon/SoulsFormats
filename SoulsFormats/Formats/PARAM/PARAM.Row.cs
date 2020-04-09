﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class PARAM
    {
        /// <summary>
        /// One row in a param file.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// The ID number of this row.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// A name given to this row; no functional significance, may be null.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
            /// </summary>
            public IReadOnlyList<Cell> Cells { get; private set; }

            internal long DataOffset;

            /// <summary>
            /// Creates a new row based on the given paramdef with default values.
            /// </summary>
            public Row(long id, string name, PARAMDEF paramdef)
            {
                ID = id;
                Name = name;

                var cells = new Cell[paramdef.Fields.Count];
                for (int i = 0; i < paramdef.Fields.Count; i++)
                {
                    PARAMDEF.Field field = paramdef.Fields[i];
                    object value = ParamUtil.CastDefaultValue(field);
                    cells[i] = new Cell(field, value);
                }
                Cells = cells;
            }

            internal Row(BinaryReaderEx br, byte format2D, byte format2E)
            {
                long nameOffset;
                if ((format2D & 0x7F) < 4)
                {
                    ID = br.ReadUInt32();
                    DataOffset = br.ReadUInt32();
                    nameOffset = br.ReadUInt32();
                }
                else
                {
                    ID = br.ReadInt64();
                    DataOffset = br.ReadInt64();
                    nameOffset = br.ReadInt64();
                }

                if (nameOffset != 0)
                {
                    if (format2E < 7)
                        Name = br.GetShiftJIS(nameOffset);
                    else
                        Name = br.GetUTF16(nameOffset);
                }
            }

            internal void ReadCells(BinaryReaderEx br, PARAMDEF paramdef)
            {
                // In case someone decides to add new rows before applying the paramdef (please don't do that)
                if (DataOffset == 0)
                    return;

                br.Position = DataOffset;
                var cells = new Cell[paramdef.Fields.Count];

                int bitOffset = -1;
                PARAMDEF.DefType bitType = PARAMDEF.DefType.u8;
                uint bitValue = 0;

                for (int i = 0; i < paramdef.Fields.Count; i++)
                {
                    PARAMDEF.Field field = paramdef.Fields[i];
                    object value = null;
                    PARAMDEF.DefType type = field.DisplayType;

                    if (type == PARAMDEF.DefType.s8)
                        value = br.ReadSByte();
                    else if (type == PARAMDEF.DefType.s16)
                        value = br.ReadInt16();
                    else if (type == PARAMDEF.DefType.s32)
                        value = br.ReadInt32();
                    else if (type == PARAMDEF.DefType.f32)
                        value = br.ReadSingle();
                    else if (type == PARAMDEF.DefType.fixstr)
                        value = br.ReadFixStr(field.ArrayLength);
                    else if (type == PARAMDEF.DefType.fixstrW)
                        value = br.ReadFixStrW(field.ArrayLength * 2);
                    else if (ParamUtil.IsBitType(type))
                    {
                        if (field.BitSize == -1)
                        {
                            if (type == PARAMDEF.DefType.u8)
                                value = br.ReadByte();
                            else if (type == PARAMDEF.DefType.u16)
                                value = br.ReadUInt16();
                            else if (type == PARAMDEF.DefType.u32)
                                value = br.ReadUInt32();
                            else if (type == PARAMDEF.DefType.dummy8)
                                value = br.ReadBytes(field.ArrayLength);
                        }
                    }
                    else
                        throw new NotImplementedException($"Unsupported field type: {type}");

                    if (value != null)
                    {
                        bitOffset = -1;
                    }
                    else
                    {
                        PARAMDEF.DefType newBitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                        int bitLimit = ParamUtil.GetBitLimit(newBitType);

                        if (field.BitSize == 0)
                            throw new NotImplementedException($"Bit size 0 is not supported.");
                        if (field.BitSize > bitLimit)
                            throw new InvalidDataException($"Bit size {field.BitSize} is too large to fit in type {newBitType}.");

                        if (bitOffset == -1 || newBitType != bitType || bitOffset + field.BitSize > bitLimit)
                        {
                            bitOffset = 0;
                            bitType = newBitType;
                            if (bitType == PARAMDEF.DefType.u8)
                                bitValue = br.ReadByte();
                            else if (bitType == PARAMDEF.DefType.u16)
                                bitValue = br.ReadUInt16();
                            else if (bitType == PARAMDEF.DefType.u32)
                                bitValue = br.ReadUInt32();
                        }

                        uint shifted = bitValue << (32 - field.BitSize - bitOffset) >> (32 - field.BitSize);
                        bitOffset += field.BitSize;
                        if (bitType == PARAMDEF.DefType.u8)
                            value = (byte)shifted;
                        else if (bitType == PARAMDEF.DefType.u16)
                            value = (ushort)shifted;
                        else if (bitType == PARAMDEF.DefType.u32)
                            value = shifted;
                    }

                    cells[i] = new Cell(field, value);
                }
                Cells = cells;
            }

            internal void WriteHeader(BinaryWriterEx bw, byte format2D, int i)
            {
                if ((format2D & 0x7F) < 4)
                {
                    bw.WriteUInt32((uint)ID);
                    bw.ReserveUInt32($"RowOffset{i}");
                    bw.ReserveUInt32($"NameOffset{i}");
                }
                else
                {
                    bw.WriteInt64(ID);
                    bw.ReserveInt64($"RowOffset{i}");
                    bw.ReserveInt64($"NameOffset{i}");
                }
            }

            internal void WriteCells(BinaryWriterEx bw, byte format2D, int index)
            {
                if ((format2D & 0x7F) < 4)
                    bw.FillUInt32($"RowOffset{index}", (uint)bw.Position);
                else
                    bw.FillInt64($"RowOffset{index}", bw.Position);

                int bitOffset = -1;
                PARAMDEF.DefType bitType = PARAMDEF.DefType.u8;
                uint bitValue = 0;

                for (int i = 0; i < Cells.Count; i++)
                {
                    Cell cell = Cells[i];
                    object value = cell.Value;
                    PARAMDEF.Field field = cell.Def;
                    PARAMDEF.DefType type = field.DisplayType;

                    if (type == PARAMDEF.DefType.s8)
                        bw.WriteSByte((sbyte)value);
                    else if (type == PARAMDEF.DefType.s16)
                        bw.WriteInt16((short)value);
                    else if (type == PARAMDEF.DefType.s32)
                        bw.WriteInt32((int)value);
                    else if (type == PARAMDEF.DefType.f32)
                        bw.WriteSingle((float)value);
                    else if (type == PARAMDEF.DefType.fixstr)
                        bw.WriteFixStr((string)value, field.ArrayLength);
                    else if (type == PARAMDEF.DefType.fixstrW)
                        bw.WriteFixStrW((string)value, field.ArrayLength * 2);
                    else if (ParamUtil.IsBitType(type))
                    {
                        if (field.BitSize == -1)
                        {
                            if (type == PARAMDEF.DefType.u8)
                                bw.WriteByte((byte)value);
                            else if (type == PARAMDEF.DefType.u16)
                                bw.WriteUInt16((ushort)value);
                            else if (type == PARAMDEF.DefType.u32)
                                bw.WriteUInt32((uint)value);
                            else if (type == PARAMDEF.DefType.dummy8)
                                bw.WriteBytes((byte[])value);
                        }
                        else
                        {
                            if (bitOffset == -1)
                            {
                                bitOffset = 0;
                                bitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                                bitValue = 0;
                            }

                            uint shifted = 0;
                            if (bitType == PARAMDEF.DefType.u8)
                                shifted = (byte)value;
                            else if (bitType == PARAMDEF.DefType.u16)
                                shifted = (ushort)value;
                            else if (bitType == PARAMDEF.DefType.u32)
                                shifted = (uint)value;
                            // Shift left first to clear any out-of-range bits
                            shifted = shifted << (32 - field.BitSize) >> (32 - field.BitSize - bitOffset);
                            bitValue |= shifted;
                            bitOffset += field.BitSize;

                            bool write = false;
                            if (i == Cells.Count - 1)
                            {
                                write = true;
                            }
                            else
                            {
                                PARAMDEF.Field nextField = Cells[i + 1].Def;
                                PARAMDEF.DefType nextType = nextField.DisplayType;
                                int bitLimit = ParamUtil.GetBitLimit(bitType);
                                if (!ParamUtil.IsBitType(nextType) || nextField.BitSize == -1 || bitOffset + nextField.BitSize > bitLimit
                                    || (nextType == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : nextType) != bitType)
                                {
                                    write = true;
                                }
                            }

                            if (write)
                            {
                                bitOffset = -1;
                                if (bitType == PARAMDEF.DefType.u8)
                                    bw.WriteByte((byte)bitValue);
                                else if (bitType == PARAMDEF.DefType.u16)
                                    bw.WriteUInt16((ushort)bitValue);
                                else if (bitType == PARAMDEF.DefType.u32)
                                    bw.WriteUInt32(bitValue);
                            }
                        }
                    }
                    else
                        throw new NotImplementedException($"Unsupported field type: {type}");
                }
            }

            internal void WriteName(BinaryWriterEx bw, byte format2D, byte format2E, int i)
            {
                long nameOffset = 0;
                if (Name != null)
                {
                    nameOffset = bw.Position;
                    if (format2E < 7)
                        bw.WriteShiftJIS(Name, true);
                    else
                        bw.WriteUTF16(Name, true);
                }

                if ((format2D & 0x7F) < 4)
                    bw.FillUInt32($"NameOffset{i}", (uint)nameOffset);
                else
                    bw.FillInt64($"NameOffset{i}", nameOffset);
            }

            /// <summary>
            /// Returns a string representation of the row.
            /// </summary>
            public override string ToString()
            {
                return $"{ID} {Name}";
            }

            /// <summary>
            /// Returns the first cell in the row with the given internal name.
            /// </summary>
            public Cell this[string name] => Cells.First(cell => cell.Def.InternalName == name);
        }
    }
}
