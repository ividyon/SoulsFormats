using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using SoulsFormats.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SoulsFormats
{
    /// <summary>
    /// The header file of the dvdbnd container format used to package all game files with hashed filenames.
    /// </summary>
    public class BHD5
    {
        /// <summary>
        /// Format the file should be written in.
        /// </summary>
        public Game Format { get; set; }

        /// <summary>
        /// Whether the header is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown; possibly whether crypto is allowed? Offsets are present regardless.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// A salt used to calculate SHA hashes for file data.
        /// </summary>
        public string? Salt { get; set; }

        /// <summary>
        /// Collections of files grouped by their hash value for faster lookup.
        /// </summary>
        public List<Bucket> Buckets { get; set; }
        /// <summary>
        /// List of file headers by file name. Only contains headers with known names
        /// </summary>

        public readonly Dictionary<string, FileHeader> fileHeaders = new();

        private readonly FileNameDictionary? fileNameDictionary;

        /// <summary>
        /// Read a dvdbnd header from the given stream, formatted for the given game. Must already be decrypted, if applicable.
        /// </summary>
        public static BHD5 Read(Stream bhdStream, Game game, string archiveName)
        {
            var br = new BinaryReaderEx(false, bhdStream);
            return new BHD5(br, game, archiveName);
        }

        /// <summary>
        /// Read a dvdbnd header from the given file name, formatted for the given game.
        /// </summary>
        public static BHD5 Read(string fileName, Game game)
        {
            var bytes = File.ReadAllBytes(fileName);
            var archiveName = Path.GetFileNameWithoutExtension(fileName);
            string key = ErRsaKeyDictionary[archiveName];
            var decrypted = NativeRsa.Decrypt(bytes, key, Environment.ProcessorCount > 8 ? Environment.ProcessorCount / 2 : 4);
            return Read(new MemoryStream(decrypted), game, archiveName);
        }

        /// <summary>
        /// Write a dvdbnd header to the given stream.
        /// </summary>
        public void Write(Stream bhdStream)
        {
            var bw = new BinaryWriterEx(false, bhdStream);
            Write(bw);
            bw.Finish();
        }

        /// <summary>
        /// Creates an empty BHD5.
        /// </summary>
        public BHD5(Game game)
        {
            Format = game;
            Salt = "";
            Buckets = new List<Bucket>();
        }

        private BHD5(BinaryReaderEx br, Game game, string archiveName)
        {
            if (game == Game.EldenRing) fileNameDictionary = new FileNameDictionary(game);
            Format = game;

            br.AssertASCII("BHD5");
            BigEndian = br.AssertSByte(0, -1) == 0;
            br.BigEndian = BigEndian;
            Unk05 = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertInt32(1);
            br.ReadInt32(); // File size
            int bucketCount = br.ReadInt32();
            int bucketsOffset = br.ReadInt32();

            if (game >= Game.DarkSouls2)
            {
                int saltLength = br.ReadInt32();
                Salt = br.ReadASCII(saltLength);
                // No padding
            }

            br.Position = bucketsOffset;
            Buckets = new List<Bucket>(bucketCount);
            for (int i = 0; i < bucketCount; i++)
                Buckets.Add(new Bucket(br, game, fileNameDictionary, archiveName));
            if (fileNameDictionary != null) {
                foreach (var bucket in Buckets) {
                    foreach (var entry in bucket) {
                        if (entry.FileName != null) fileHeaders[entry.FileName] = entry;
                    }
                }
            }
        }

        private void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteASCII("BHD5");
            bw.WriteSByte((sbyte)(BigEndian ? 0 : -1));
            bw.WriteBoolean(Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(1);
            bw.ReserveInt32("FileSize");
            bw.WriteInt32(Buckets.Count);
            bw.ReserveInt32("BucketsOffset");

            if (Format >= Game.DarkSouls2)
            {
                bw.WriteInt32(Salt?.Length ?? 0);
                bw.WriteASCII(Salt ?? "");
            }

            bw.FillInt32("BucketsOffset", (int)bw.Position);
            for (int i = 0; i < Buckets.Count; i++)
                Buckets[i].Write(bw, i);

            for (int i = 0; i < Buckets.Count; i++)
                Buckets[i].WriteFileHeaders(bw, Format, i);

            for (int i = 0; i < Buckets.Count; i++)
                for (int j = 0; j < Buckets[i].Count; j++)
                    Buckets[i][j].WriteHashAndKey(bw, Format, i, j);

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Indicates the format of a dvdbnd.
        /// </summary>
        public enum Game
        {
            /// <summary>
            /// Dark Souls 1, both PC and console versions.
            /// </summary>
            DarkSouls1,

            /// <summary>
            /// Dark Souls 2 and Scholar of the First Sin on PC.
            /// </summary>
            DarkSouls2,

            /// <summary>
            /// Dark Souls 3 and Sekiro on PC.
            /// </summary>
            DarkSouls3,

            /// <summary>
            /// Elden Ring on PC.
            /// </summary>
            Sekiro,

            /// <summary>
            /// Elden Ring on PC.
            /// </summary>
            EldenRing,
        }

        /// <summary>
        /// A collection of files grouped by their hash.
        /// </summary>
        public class Bucket : List<FileHeader>
        {
            /// <summary>
            /// Creates an empty Bucket.
            /// </summary>
            public Bucket() : base() { }

            internal Bucket(BinaryReaderEx br, Game game, FileNameDictionary? fileNameDictionary, string archiveName) : base()
            {
                int fileHeaderCount = br.ReadInt32();
                int fileHeadersOffset = br.ReadInt32();
                Capacity = fileHeaderCount;

                br.StepIn(fileHeadersOffset);
                {
                    for (int i = 0; i < fileHeaderCount; i++)
                        Add(new FileHeader(br, game, fileNameDictionary, archiveName));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(Count);
                bw.ReserveInt32($"FileHeadersOffset{index}");
            }

            internal void WriteFileHeaders(BinaryWriterEx bw, Game game, int index)
            {
                bw.FillInt32($"FileHeadersOffset{index}", (int)bw.Position);
                for (int i = 0; i < Count; i++)
                    this[i].Write(bw, game, index, i);
            }
        }

        /// <summary>
        /// Information about an individual file in the dvdbnd.
        /// </summary>
        public class FileHeader
        {
            /// <summary>
            /// Hash of the full file path using From's algorithm found in SFUtil.FromPathHash.
            /// </summary>
            public ulong FileNameHash { get; set; }

            /// <summary>
            /// Name and path of the file, if available
            /// </summary>
            public string? FileName { get; set; }

            /// <summary>
            /// Full size of the file data in the BDT.
            /// </summary>
            public int PaddedFileSize { get; set; }

            /// <summary>
            /// File size after decryption; only included in DS3.
            /// </summary>
            public long UnpaddedFileSize { get; set; }

            /// <summary>
            /// Beginning of file data in the BDT.
            /// </summary>
            public long FileOffset { get; set; }

            /// <summary>
            /// Hashing information for this file.
            /// </summary>
            public SHAHash SHAHash { get; set; }

            /// <summary>
            /// Encryption information for this file.
            /// </summary>
            public AESKey AESKey { get; set; }

            /// <summary>
            /// Creates a FileHeader with default values.
            /// </summary>
            public FileHeader() { }

            internal FileHeader(BinaryReaderEx br, Game game, FileNameDictionary? fileNameDictionary, string archiveName)
            {
                long shaHashOffset = 0;
                long aesKeyOffset = 0;
                UnpaddedFileSize = -1;
                if (game >= Game.EldenRing) {
                    FileNameHash = br.ReadUInt64();
                    PaddedFileSize = br.ReadInt32();
                    UnpaddedFileSize = br.ReadInt32();
                    if (UnpaddedFileSize == 0) UnpaddedFileSize = PaddedFileSize;
                    if (fileNameDictionary != null) {
                        FileName = fileNameDictionary.TryGetFilename(FileNameHash, archiveName, "UNKNOWN");
                    }
                } else {
                    FileNameHash = br.ReadUInt32();
                    PaddedFileSize = br.ReadInt32();
                }
                FileOffset = br.ReadInt64();

                if (game >= Game.DarkSouls2)
                {
                    long shaHashOffset = br.ReadInt64();
                    long aesKeyOffset = br.ReadInt64();

                    if (shaHashOffset != 0)
                    {
                        br.StepIn(shaHashOffset);
                        {
                            SHAHash = new SHAHash(br);
                        }
                        br.StepOut();
                    }

                    if (aesKeyOffset != 0)
                    {
                        br.StepIn(aesKeyOffset);
                        {
                            AESKey = new AESKey(br);
                        }
                        br.StepOut();
                    }
                }

                if (game < Game.EldenRing) {
                    UnpaddedFileSize = -1;
                    if (game == Game.DarkSouls3) {
                        UnpaddedFileSize = br.ReadInt64();
                    }
                }
            }

            internal void Write(BinaryWriterEx bw, Game game, int bucketIndex, int fileIndex)
            {
                if (game >= Game.EldenRing) {
                    bw.WriteUInt64(FileNameHash);
                    bw.WriteInt32(PaddedFileSize);
                    if (PaddedFileSize == UnpaddedFileSize) bw.WriteInt32(0);
                    else bw.WriteInt32((int)UnpaddedFileSize);
                } else {
                    bw.WriteUInt32((uint)FileNameHash);
                    bw.WriteInt32(PaddedFileSize);
                }
                bw.WriteInt64(FileOffset);

                    if (game >= Game.DarkSouls2)
                    {
                        bw.ReserveInt64($"SHAHashOffset{bucketIndex}:{fileIndex}");
                        bw.ReserveInt64($"AESKeyOffset{bucketIndex}:{fileIndex}");
                    }

                    if (game >= Game.DarkSouls3)
                    {
                        bw.WriteInt64(UnpaddedFileSize);
                    }
                }
            }

            internal void WriteHashAndKey(BinaryWriterEx bw, Game game, int bucketIndex, int fileIndex)
            {
                if (game >= Game.DarkSouls2)
                {
                    if (SHAHash == null)
                    {
                        bw.FillInt64($"SHAHashOffset{bucketIndex}:{fileIndex}", 0);
                    }
                    else
                    {
                        bw.FillInt64($"SHAHashOffset{bucketIndex}:{fileIndex}", bw.Position);
                        SHAHash.Write(bw);
                    }

                    if (AESKey == null)
                    {
                        bw.FillInt64($"AESKeyOffset{bucketIndex}:{fileIndex}", 0);
                    }
                    else
                    {
                        bw.FillInt64($"AESKeyOffset{bucketIndex}:{fileIndex}", bw.Position);
                        AESKey.Write(bw);
                    }
                }
            }

            /// <summary>
            /// Read and decrypt (if necessary) file data from the BDT.
            /// </summary>
            public byte[] ReadFile(FileStream bdtStream)
            {
                byte[] bytes = new byte[PaddedFileSize];
                bdtStream.Position = FileOffset;
                bdtStream.Read(bytes, 0, PaddedFileSize);
                AESKey?.Decrypt(bytes);
                return bytes;
            }
        }

        /// <summary>
        /// Hash information for a file in the dvdbnd.
        /// </summary>
        public class SHAHash
        {
            /// <summary>
            /// 32-byte salted SHA hash.
            /// </summary>
            public byte[] Hash { get; set; }

            /// <summary>
            /// Hashed sections of the file.
            /// </summary>
            public List<Range> Ranges { get; set; }

            /// <summary>
            /// Creates a SHAHash with default values.
            /// </summary>
            public SHAHash()
            {
                Hash = new byte[32];
                Ranges = new List<Range>();
            }

            internal SHAHash(BinaryReaderEx br)
            {
                Hash = br.ReadBytes(32);
                int rangeCount = br.ReadInt32();
                Ranges = new List<Range>(rangeCount);
                for (int i = 0; i < rangeCount; i++)
                    Ranges.Add(new Range(br));
            }

            internal void Write(BinaryWriterEx bw)
            {
                if (Hash.Length != 32)
                    throw new InvalidDataException("SHA hash must be 32 bytes long.");

                bw.WriteBytes(Hash);
                bw.WriteInt32(Ranges.Count);
                foreach (Range range in Ranges)
                    range.Write(bw);
            }
        }

        /// <summary>
        /// Encryption information for a file in the dvdbnd.
        /// </summary>
        public class AESKey
        {
            private static readonly Aes AES;

            static AESKey()
            {
                AES = Aes.Create();
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;
                AES.KeySize = 128;
            }

            /// <summary>
            /// 16-byte encryption key.
            /// </summary>
            public byte[] Key { get; set; }

            /// <summary>
            /// Encrypted sections of the file.
            /// </summary>
            public List<Range> Ranges { get; set; }

            /// <summary>
            /// Creates an AESKey with default values.
            /// </summary>
            public AESKey()
            {
                Key = new byte[16];
                Ranges = new List<Range>();
            }

            internal AESKey(BinaryReaderEx br)
            {
                Key = br.ReadBytes(16);
                int rangeCount = br.ReadInt32();
                Ranges = new List<Range>(rangeCount);
                for (int i = 0; i < rangeCount; i++)
                    Ranges.Add(new Range(br));
            }

            internal void Write(BinaryWriterEx bw)
            {
                if (Key.Length != 16)
                    throw new InvalidDataException("AES key must be 16 bytes long.");

                bw.WriteBytes(Key);
                bw.WriteInt32(Ranges.Count);
                foreach (Range range in Ranges)
                    range.Write(bw);
            }

            /// <summary>
            /// Decrypt file data in-place.
            /// </summary>
            public void Decrypt(byte[] bytes)
            {
                using (ICryptoTransform decryptor = AES.CreateDecryptor(Key, new byte[16]))
                {
                    foreach (Range range in Ranges.Where(r => r.StartOffset != -1 && r.EndOffset != -1 && r.StartOffset != r.EndOffset))
                    {
                        int start = (int)range.StartOffset;
                        int count = (int)(range.EndOffset - range.StartOffset);
                        decryptor.TransformBlock(bytes, start, count, bytes, start);
                    }
                }
            }
        }

        /// <summary>
        /// Hashes a file name using the 64-bit hash algorithm.
        /// </summary>
        public static ulong HashFileName(string filePath, ulong prime = 133u)
        {
            if (string.IsNullOrEmpty(filePath))
                return 0u;
            return filePath.Replace('\\', '/')
                .ToLowerInvariant()
                .Aggregate(0ul, (i, c) => i * prime + c);
        }


        /// <summary>
        /// Indicates a hashed or encrypted section of a file.
        /// </summary>
        public struct Range
        {
            /// <summary>
            /// The beginning of the range, inclusive.
            /// </summary>
            public long StartOffset { get; set; }

            /// <summary>
            /// The end of the range, exclusive.
            /// </summary>
            public long EndOffset { get; set; }

            /// <summary>
            /// Creates a Range with the given values.
            /// </summary>
            public Range(long startOffset, long endOffset)
            {
                StartOffset = startOffset;
                EndOffset = endOffset;
            }

            internal Range(BinaryReaderEx br)
            {
                StartOffset = br.ReadInt64();
                EndOffset = br.ReadInt64();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt64(StartOffset);
                bw.WriteInt64(EndOffset);
            }
        }

        internal class FileNameDictionary
        {
            private static readonly string[] VirtualRoots = {
                @"N:\GR\data\INTERROOT_win64\",
                @"N:\FDP\data\INTERROOT_win64\",
                @"N:\FDP\data\INTERROOT_win64_havok2018_1\",
                @"N:\GR\data\INTERROOT_win64_havok2018_1\",
                @"N:\GR\data\",
                @"N:\SPRJ\data\",
                @"N:\FDP\data\",
                @"N:\NTC\",
                @"N:\"
            };

            private static readonly string[] PhysicalRoots = {
                "data0",
                "data1",
                "data2",
                "data3",
                "debugdata",
                "dvdroot",
                "hkxbnd",
                "tpfbnd",
                "sd"
            };

            private static readonly Dictionary<string, string> AliasMap = new() {
                { "testdata", "debugdata:/testdata" },
                { "other", "data0:/other" },
                { "mapinfotex", "data0:/other/mapinfotex" },
                { "material", "data0:/material" },
                { "mtdbnd", "data0:/mtd" },
                { "shader", "data0:/shader" },
                { "shadertestdata", "debugdata:/testdata/Shaderbdle" },
                { "debugfont", "dvdroot:/font" },
                { "font", "data0:/font" },
                { "chrbnd", "data3:/chr" },
                { "chranibnd", "data3:/chr" },
                { "chrbehbnd", "data3:/chr" },
                { "chrtexbnd", "data3:/chr" },
                { "chrtpf", "data3:/chr" },
                { "action", "data0:/action" },
                { "actscript", "data0:/action/script" },
                { "obj", "data0:/obj" },
                { "objbnd", "data0:/obj" },
                { "map", "data2:/map" },
                { "debugmap", "debugdata:/map" },
                { "maphkx", "data2:/map" },
                { "maptpf", "data2:/map" },
                { "mapstudio", "data2:/map/mapstudio" },
                { "breakobj", "data2:/map/breakobj" },
                { "breakgeom", "data2:/map/breakgeom" },
                { "entryfilelist", "data2:/map/entryfilelist" },
                { "onav", "data2:/map/onav" },
                { "script", "data0:/script" },
                { "talkscript", "data0:/script/talk" },
                { "aiscript", "data0:/script" },
                { "msg", "data0:/msg" },
                { "param", "data0:/param" },
                { "paramdef", "debugdata:/paramdef" },
                { "gparam", "data0:/param/drawparam" },
                { "event", "data0:/event" },
                { "menu", "data0:/menu" },
                { "menutexture", "data0:/menu" },
                { "parts", "data0:/parts" },
                { "facegen", "data0:/facegen" },
                { "cutscene", "data0:/cutscene" },
                { "cutscenebnd", "data0:/cutscene" },
                { "movie", "data0:/movie" },
                { "wwise_mobnkinfo", "data0:/sound" },
                { "wwise_moaeibnd", "data0:/sound" },
                { "wwise_testdata", "debugdata:/testdata/sound" },
                { "wwise_pck", "data0:/sound/pck" },
                { "wwise", "sd:" },
                { "sfx", "data0:/sfx" },
                { "sfxbnd", "data0:/sfx" },
                { "patch_sfxbnd", "data0:/sfx" },
                { "title", "data0:/adhoc" },
                { "", "data0:/adhoc" },
                { "dbgai", "data0:/script_interroot" },
                { "dbgactscript", "data0:/script_interroot/action" },
                { "menuesd_dlc", "data0:/script_interroot/action" },
                { "luascriptpatch", "data0:/script_interroot/action" },
                { "asset", "data1:/asset" },
                { "expression", "data0:/expression" }
            };
            
            //id -> (root, name)
            private readonly Dictionary<ulong, List<(string, string)>> dict = new();

            public FileNameDictionary(Game game)
            {
                if (game != Game.EldenRing) throw new System.ArgumentException("Dictionary only supported for Elden Ring");
                var lines = File.ReadAllLines(@"BHDDictionaries\DictionaryER.csv");
                foreach (var line in lines) {
                    string root = "";
                    string name = line;
                    while (true) {
                        var ind = name.IndexOf(':');
                        if (ind == -1) break;
                        var alias = name[..ind];
                        name = name[(ind+1)..];
                        if (PhysicalRoots.Contains(alias)) {
                            root = alias;
                            break;
                        }
                        var aliasVal = AliasMap[alias];
                        name = aliasVal + name;
                    }
                    var hash = HashFileName(name);
                    if (!dict.TryGetValue(hash, out List<(string, string)>? entry)) {
                        entry = new List<(string, string)>();
                        dict.Add(hash, entry);
                    }
                    entry.Add((root, name));
                }
            }

            public string? TryGetFilename(ulong hash, string root, string extension)
            {
                if (!dict.TryGetValue(hash, out List<(string, string)>? entry)) return null;
                if (entry.Count == 1) return entry[0].Item2;
                string? filename = null;
                bool needExtension = false;
                foreach (var (eRoot, eName) in entry) {
                    if (eRoot != root) continue;
                    if (needExtension && !eName.EndsWith(extension)) continue;
                    if (filename != null) {
                        bool a = filename.EndsWith(extension);
                        bool b = eName.EndsWith(extension);
                        if (a && b) throw new InvalidDataException($"Ambiguous filename for hash {hash}");
                        needExtension = true;
                        if (b) filename = eName;
                        else if (!a) filename = null;
                    } 
                    else filename = eName;
                }
                return filename;
            }
        }

        /// <summary>
        /// RSA public keys in PKCS1 PEM format for Elden Ring bhd files
        /// </summary>
        public static Dictionary<string, string> ErRsaKeyDictionary = new(System.StringComparer.InvariantCultureIgnoreCase)
        {
            { "Data0", @"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEA9Rju2whruXDVQZpfylVEPeNxm7XgMHcDyaaRUIpXQE0qEo+6Y36L
P0xpFvL0H0kKxHwpuISsdgrnMHJ/yj4S61MWzhO8y4BQbw/zJehhDSRCecFJmFBz
3I2JC5FCjoK+82xd9xM5XXdfsdBzRiSghuIHL4qk2WZ/0f/nK5VygeWXn/oLeYBL
jX1S8wSSASza64JXjt0bP/i6mpV2SLZqKRxo7x2bIQrR1yHNekSF2jBhZIgcbtMB
xjCywn+7p954wjcfjxB5VWaZ4hGbKhi1bhYPccht4XnGhcUTWO3NmJWslwccjQ4k
sutLq3uRjLMM0IeTkQO6Pv8/R7UNFtdCWwIERzH8IQ==
-----END RSA PUBLIC KEY-----" 
            },
            { "Data1", @"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAxaBCHQJrtLJiJNdG9nq3deA9sY4YCZ4dbTOHO+v+YgWRMcE6iK6o
ZIJq+nBMUNBbGPmbRrEjkkH9M7LAypAFOPKC6wMHzqIMBsUMuYffulBuOqtEBD11
CAwfx37rjwJ+/1tnEqtJjYkrK9yyrIN6Y+jy4ftymQtjk83+L89pvMMmkNeZaPON
4O9q5M9PnFoKvK8eY45ZV/Jyk+Pe+xc6+e4h4cx8ML5U2kMM3VDAJush4z/05hS3
/bC4B6K9+7dPwgqZgKx1J7DBtLdHSAgwRPpijPeOjKcAa2BDaNp9Cfon70oC+ZCB
+HkQ7FjJcF7KaHsH5oHvuI7EZAl2XTsLEQIENa/2JQ==
-----END RSA PUBLIC KEY-----" 
            },
            { "Data2", @"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEA0iDVVQ230RgrkIHJNDgxE7I/2AaH6Li1Eu9mtpfrrfhfoK2e7y4O
WU+lj7AGI4GIgkWpPw8JHaV970Cr6+sTG4Tr5eMQPxrCIH7BJAPCloypxcs2BNfT
GXzm6veUfrGzLIDp7wy24lIA8r9ZwUvpKlN28kxBDGeCbGCkYeSVNuF+R9rN4OAM
RYh0r1Q950xc2qSNloNsjpDoSKoYN0T7u5rnMn/4mtclnWPVRWU940zr1rymv4Jc
3umNf6cT1XqrS1gSaK1JWZfsSeD6Dwk3uvquvfY6YlGRygIlVEMAvKrDRMHylsLt
qqhYkZNXMdy0NXopf1rEHKy9poaHEmJldwIFAP////8=
-----END RSA PUBLIC KEY-----" 
            },
            { "Data3", @"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAvRRNBnVq3WknCNHrJRelcEA2v/OzKlQkxZw1yKll0Y2Kn6G9ts94
SfgZYbdFCnIXy5NEuyHRKrxXz5vurjhrcuoYAI2ZUhXPXZJdgHywac/i3S/IY0V/
eDbqepyJWHpP6I565ySqlol1p/BScVjbEsVyvZGtWIXLPDbx4EYFKA5B52uK6Gdz
4qcyVFtVEhNoMvg+EoWnyLD7EUzuB2Khl46CuNictyWrLlIHgpKJr1QD8a0ld0PD
PHDZn03q6QDvZd23UW2d9J+/HeBt52j08+qoBXPwhndZsmPMWngQDaik6FM7EVRQ
etKPi6h5uprVmMAS5wR/jQIVTMpTj/zJdwIEXszeQw==
-----END RSA PUBLIC KEY-----" 
            },
            { "sd", @"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAmYJ/5GJU4boJSvZ81BFOHYTGdBWPHnWYly3yWo01BYjGRnz8NTkz
DHUxsbjIgtG5XqsQfZstZILQ97hgSI5AaAoCGrT8sn0PeXg2i0mKwL21gRjRUdvP
Dp1Y+7hgrGwuTkjycqqsQ/qILm4NvJHvGRd7xLOJ9rs2zwYhceRVrq9XU2AXbdY4
pdCQ3+HuoaFiJ0dW0ly5qdEXjbSv2QEYe36nWCtsd6hEY9LjbBX8D1fK3D2c6C0g
NdHJGH2iEONUN6DMK9t0v2JBnwCOZQ7W+Gt7SpNNrkx8xKEM8gH9na10g9ne11Mi
O1FnLm8i4zOxVdPHQBKICkKcGS1o3C2dfwIEXw/f3w==
-----END RSA PUBLIC KEY-----" 
            },
        };
    }
}
