using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok
{
    /// <summary>
    /// Data representing a Havok type
    /// Stored in .compendium files and used in other Havok files
    /// </summary>
    public class HavokTagType
    {
        private bool copiedParent = false;
        /// <summary>
        /// The type's name
        /// </summary>
        public string name;
        /// <summary>
        /// A list of template variables (C++ templates)
        /// </summary>
        public List<HavokTagTemplate> templates;
        /// <summary>
        /// The superclass of this type
        /// </summary>
        public HavokTagType parent;
        /// <summary>
        /// Flags telling a few things about this type
        /// Not necessary except for serialization
        /// </summary>
        public int flags;
        /// <summary>
        /// Unknown
        /// </summary>
        public int subTypeFlags;
        /// <summary>
        /// Unknown
        /// </summary>
        public HavokTagType? pointer;
        /// <summary>
        /// The version of this type
        /// Unknown semantics
        /// </summary>
        public int version;
        /// <summary>
        /// Probably the size of this type in bytes
        /// </summary>
        public int byteSize;
        /// <summary>
        /// The alignment of this type in bits
        /// </summary>
        public int alignment;
        /// <summary>
        /// Unknown
        /// </summary>
        public int abstractValue;
        /// <summary>
        /// A list of members/fields
        /// </summary>
        public List<HavokTagMember> members = new();
        /// <summary>
        /// A list of interfaces this type implements
        /// </summary>
        public List<(HavokTagType, int)>? interfaces;
        /// <summary>
        /// Unknown
        /// </summary>
        public string? attribute;

        public override string ToString()
        {
            return $"{name}{{{byteSize}}}";
        }

        internal void EnsureCopiedParent()
        {
            if (copiedParent) return;
            copiedParent = true;
            if (parent == null) return;
            parent.EnsureCopiedParent();
            var tmp = new List<HavokTagMember>(parent.members);
            tmp.AddRange(members);
            members = tmp;
        }
        private static Dictionary<string, string> primitives = new() {
            { "bool", "bool" },
            { "hkBool", "bool" },
            { "hkUint8", "byte" },
            { "hkInt8", "sbyte" },
            { "char", "char" },
            { "signed char", "char" },
            { "unsigned char", "uchar" },
            { "hkUint16", "ushort" },
            { "unsigned short", "ushort" },
            { "hkInt16", "short" },
            { "short", "short" },
            { "hkUint32", "uint" },
            { "unsigned int", "uint" },
            { "hkInt32", "int" },
            { "int", "int" },
            { "hkUint64", "ulonglong" },
            { "unsigned long long", "ulonglong" },
            { "unsigned long", "ulong" },
            { "hkInt64", "longlong" },
            { "long long", "longlong" },
            { "long", "long" },
            { "char*", "char*" },
            { "const char*", "const char*" },
            { "hkUintReal", "hkUintReal" },
            { "void", "void" },
            { "hkReal", "float" },
            { "float", "float" },
            { "hkUFloat8", "ufbyte" },
            { "hkHalf16", "half" },

        };
        public string ToCppRepr()
        {
            var sb = new StringBuilder();
            var parentStr = parent == null ? "" : $" : {parent.name}";
            sb.AppendLine($"public class {name}{parentStr} {{ //{byteSize} bytes");
            int currPosBytes = 0;
            foreach (var m in members) {
                var diffPos = m.byteOffset - currPosBytes;
                if (diffPos != 0) {
                    sb.AppendLine($"    byte[{diffPos}] paddingBytes; // {currPosBytes}-{currPosBytes+diffPos} ({diffPos})");
                    currPosBytes += diffPos;
                }
                if (primitives.TryGetValue(m.type.name, out string typeName)) {
                    sb.Append($"    {typeName} {m.name};");
                } else {
                    if (m.type.name == "T*") {
                        //typeName = m.type.templates[0].valueType.name+"*";
                        sb.Append($"    {m.type.templates[0].valueType.name}* {m.name};");
                    } else {
                        string generics = "";
                        if (m.type.templates.Count != 0) {
                            generics = "<";
                            for (int i = 0; i < m.type.templates.Count; i++) {
                                var t = m.type.templates[i];
                                var val = t.valueType == null ? t.valueInt.ToString() : t.valueType.ToString();
                                generics += $"{t.name} = {val}";
                                if (i < m.type.templates.Count - 1) generics += ",";
                            }
                            generics += ">";
                        }
                        sb.Append($"    {m.type.name}{generics} {m.name};");
                    }
                }
                sb.AppendLine($" //{currPosBytes}-{currPosBytes + m.type.byteSize} ({m.type.byteSize})");
                currPosBytes += m.type.byteSize;
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

    }
    /// <summary>
    /// A template variable for a Havok type
    /// May be either a value variable or a type variable
    /// </summary>
    public class HavokTagTemplate
    {
        /// <summary>
        /// The name of this template variable
        /// Starts with "v" if it's a value variable,
        /// or "t" if it's a type variable
        /// </summary>
        public string name;
        /// <summary>
        /// The value of this variable if it's a value variable
        /// Must have a value if valueType is null
        /// </summary>
        public int? valueInt;
        /// <summary>
        /// The value of this variable if it's a type variable
        /// Must have a value if valueInt is null
        /// </summary>
        public HavokTagType? valueType;
        /// <summary>
        /// Constructor for a value variable
        /// </summary>
        public HavokTagTemplate(string name, int valueInt)
        {
            this.name = name;
            this.valueInt = valueInt;
        }
        /// <summary>
        /// Constructor for a type variable
        /// </summary>
        public HavokTagTemplate(string name, HavokTagType valueType)
        {
            this.name = name;
            this.valueType = valueType;
        }
    }
    /// <summary>
    /// A member (field) in a Havok type
    /// </summary>
    public class HavokTagMember
    {
        /// <summary>
        /// The name of this member
        /// </summary>
        public string name;
        /// <summary>
        /// Unknown
        /// </summary>
        public int flags;
        /// <summary>
        /// The offset of this member from the start of its containing type
        /// </summary>
        public int byteOffset;
        /// <summary>
        /// The type of this member
        /// </summary>
        public HavokTagType type;
        public override string ToString()
        {
            return $"{name}: {type.ToString()}";
        }
    }
    /// <summary>
    /// Values for flags to help deserialization
    /// </summary>
    public enum HavokTagFlags : int
    {
        /// <summary>
        /// This type has a subtype
        /// </summary>
        SubType = 0x1, //0x1
        /// <summary>
        /// This type has a value for its pointer field
        /// </summary>
        Pointer = 0x2, //0x2
        /// <summary>
        /// This type is versioned
        /// </summary>
        Version = 0x4, //0x10
        /// <summary>
        /// This type has a specified byte size and alignment
        /// </summary>
        ByteSize = 0x8, //0x800000
        /// <summary>
        /// This type has a value for its abstractValue field
        /// </summary>
        AbstractValue = 0x10, //0x1000000
        /// <summary>
        /// This type has members
        /// </summary>
        Members = 0x20, //0x4000000
        /// <summary>
        /// This type implements interfaces
        /// </summary>
        Interfaces = 0x40, //0x20000
        /// <summary>
        /// This type has an attribute
        /// </summary>
        Attribute = 0x80, //0x10000000
    }
}
