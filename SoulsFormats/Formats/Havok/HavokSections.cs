using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok
{
    /// <summary>
    /// Represents a section in a Havok tagfile
    /// </summary>
    public abstract class HavokSection
    {
        /// <summary>
        /// unknown
        /// </summary>
        public ushort Unk00;
        /// <summary>
        /// Length of this section in bytes
        /// </summary>
        public ushort Length;
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Reads Unk00, Length, and Id
        /// </summary>
        protected void ReadHeader(BinaryReaderEx br)
        {
            br.BigEndian = true;
            Unk00 = br.ReadUInt16();
            Length = br.ReadUInt16();
            br.BigEndian = false;
            br.AssertASCII(new string[] { Id });
        }

        /// <summary>
        /// Reads this section
        /// </summary>
        public abstract void Read(BinaryReaderEx br, HavokCompendium f);
    }
    
    /// <summary>
    /// Represents the TCM0 section, used to start a compendium file
    /// </summary>
    public class HavokSectionTCM0 : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TCM0";

        /// <summary>
        /// The TCID section inside this section
        /// </summary>
        public HavokSectionTCID tcid = new();
        /// <summary>
        /// The TYPE section inside this section
        /// </summary>
        public HavokSectionTYPE type = new();

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            ReadHeader(br);
            tcid.Read(br, f);
            type.Read(br, f);
        }
    }

    /// <summary>
    /// Represents the TCID section
    /// Contains IDs for an unknown purpose
    /// </summary>
    public class HavokSectionTCID : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TCID";

        /// <summary>
        /// Unknown
        /// </summary>
        public Byte[] Unk08;

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            ReadHeader(br);
            Unk08 = br.ReadBytes(Length - 8);
        }

    }

    /// <summary>
    /// Represents the TYPE section
    /// Contains several sections used to define Havok types
    /// </summary>
    public class HavokSectionTYPE : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TYPE";

        /// <summary>
        /// The TPTR section inside this section
        /// </summary>
        HavokSectionTPTR tptr = new();
        /// <summary>
        /// The TSTR section inside this section
        /// </summary>
        HavokSectionTSTR tstr = new();
        /// <summary>
        /// The TNA1 section inside this section
        /// </summary>
        HavokSectionTNA1 tna1 = new();
        /// <summary>
        /// The FSTR section inside this section
        /// </summary>
        HavokSectionFSTR fstr = new();
        /// <summary>
        /// The TBDY section inside this section
        /// </summary>
        HavokSectionTBDY tbdy = new();

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            ReadHeader(br);
            tptr.Read(br, f);
            tstr.Read(br, f);
            tna1.Read(br, f);
            fstr.Read(br, f);
            tbdy.Read(br, f);
        }

    }

    /// <summary>
    /// Represents the TPTR section
    /// Used for an unknown purpose
    /// </summary>
    public class HavokSectionTPTR : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TPTR";

        /// <summary>
        /// Unknown
        /// </summary>
        public Byte[] Unk08;

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            ReadHeader(br);
            Unk08 = br.ReadBytes(Length - 8);
        }

    }

    /// <summary>
    /// Represents the TSTR section
    /// Contains a list of strings used for type names and template params
    /// </summary>
    public class HavokSectionTSTR : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TSTR";

        /// <summary>
        /// The list of strings that makes up this section
        /// </summary>
        public List<string> strings = new();

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            this.strings = f.TagTypeStrings;
            ReadHeader(br);
            var start = br.Position - 8;
            var end = start + Length;
            while (br.Position < end) {
                strings.Add(br.ReadASCII());
            }
        }

    }

    /// <summary>
    /// Represents the TNA1 section
    /// Contains basic definitions for types and their template params
    /// </summary>
    public class HavokSectionTNA1 : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TNA1";

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            var start = br.Position;
            ReadHeader(br);
            int numTypes = (int)br.ReadHavokVarint();
            f.TagTypes.Capacity = numTypes;
            f.TagTypes.Add(new HavokTagType());
            for (int i = 1; i < numTypes + 1; i++) f.TagTypes.Add(new HavokTagType());
            for (int i = 1; i < numTypes + 1; i++) {
                var t = f.TagTypes[i];
                var nameInd = (int)br.ReadHavokVarint();
                t.name = f.TagTypeStrings[nameInd];
                var numTemplates = (int)br.ReadHavokVarint();
                t.templates = new List<HavokTagTemplate>(numTemplates);
                for (int j = 0; j < numTemplates; j++) {
                    var templateName = f.TagTypeStrings[(int)br.ReadHavokVarint()];
                    var templateValueInt = (int)br.ReadHavokVarint();
                    HavokTagTemplate template;
                    if (templateName.StartsWith("t"))
                        template = new HavokTagTemplate(templateName, f.TagTypes[templateValueInt]);
                    else
                        template = new HavokTagTemplate(templateName, templateValueInt);
                    t.templates.Add(template);
                }
            }
            br.Position = start + Length;
        }

    }

    /// <summary>
    /// Represents the FSTR section
    /// Contains a list of strings used for anything TSTR doesn't cover
    /// </summary>
    public class HavokSectionFSTR : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "FSTR";

        /// <summary>
        /// The list of strings that makes up this section
        /// </summary>
        public List<string> strings = new();

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            this.strings = f.TagFieldStrings;
            ReadHeader(br);
            var start = br.Position - 8;
            var end = start + Length;
            while (br.Position < end) {
                strings.Add(br.ReadASCII());
            }
        }

    }

    /// <summary>
    /// Represents the TBDY section
    /// Contains full information to flesh out types, including fields and flags
    /// </summary>
    public class HavokSectionTBDY : HavokSection
    {
        /// <summary>
        /// The id or tag for this section
        /// </summary>
        public override string Id => "TBDY";

        /// <summary>
        /// Reads this section
        /// </summary>
        public override void Read(BinaryReaderEx br, HavokCompendium f)
        {
            var start = br.Position;
            ReadHeader(br);
            var end = start + Length;
            while (br.Position < end) {
                var typeIndex = (int)br.ReadHavokVarint();
                if (typeIndex == 0) continue;
                var type = f.TagTypes[typeIndex];
                var parentInd = (int)br.ReadHavokVarint();
                type.parent = f.TagTypes[parentInd];
                type.flags = (int)br.ReadHavokVarint();
                if ((type.flags & (int)HavokTagFlags.SubType) != 0) {
                    type.subTypeFlags = (int)br.ReadHavokVarint();
                }
                if ((type.flags & (int)HavokTagFlags.Pointer) != 0) {
                    var pointerInd = (int)br.ReadHavokVarint();
                    type.pointer = f.TagTypes[pointerInd];
                }
                if ((type.flags & (int)HavokTagFlags.Version) != 0) {
                    type.version = (int)br.ReadHavokVarint();
                }
                if ((type.flags & (int)HavokTagFlags.ByteSize) != 0) {
                    type.byteSize = (int)br.ReadHavokVarint();
                    type.alignment = (int)br.ReadHavokVarint();
                }
                if ((type.flags & (int)HavokTagFlags.AbstractValue) != 0) {
                    type.abstractValue = (int)br.ReadHavokVarint();
                }
                if ((type.flags & (int)HavokTagFlags.Members) != 0) {
                    var numMembers = (int)br.ReadHavokVarint() & 0xffff;
                    type.members = new List<HavokTagMember>(numMembers);
                    for (int i = 0; i < numMembers; i++) {
                        var member = new HavokTagMember();
                        var nameInd = (int)br.ReadHavokVarint();
                        member.name = f.TagFieldStrings[nameInd];
                        member.flags = (int)br.ReadHavokVarint();
                        member.byteOffset = (int)br.ReadHavokVarint();
                        var typeInd = (int)br.ReadHavokVarint();
                        member.type = f.TagTypes[typeInd];
                        type.members.Add(member);
                    }
                }
                if ((type.flags & (int)HavokTagFlags.Interfaces) != 0) {
                    var numInterfaces = (int)br.ReadHavokVarint();
                    type.interfaces = new List<(HavokTagType, int)>(numInterfaces);
                    for (int i = 0; i < numInterfaces; i++) {
                        var typeInd = (int)br.ReadHavokVarint();
                        type.interfaces.Add((f.TagTypes[typeInd], (int)br.ReadHavokVarint()));
                    }
                }
                if ((type.flags & (uint)HavokTagFlags.Attribute) != 0) {
                    var nameInd = (int)br.ReadHavokVarint();
                    type.attribute = f.TagFieldStrings[nameInd];
                }
            }

        }

    }
}
