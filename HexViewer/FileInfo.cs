using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HexViewer
{
    public enum FileType : ushort
    {
        Unknown = 0x0000,
        File = 0x8180,
        Directory = 0x41ed
    }

    /*
     * ==========================================
     * 4 bytes - String Length
     * Domain
     * 4 bytes - String Length
     * Path or Directory
     * 4 bytes - Separator (ffff)
     * 4 bytes - String Length or (ffff)
     * File Hash
     * 4 bytes - Separator (ffff)
     * 4 bytes - Directory (41ed) or File (8180)
     * 8 bytes - (0000 0000)
     * 4 bytes - ?????
     * 4 bytes - Size?
     * 4 bytes - ?????
     * 12 bytes - (0019 0000 0019)
     * 24 bytes - Date Time Possibly?
     * 12 bytes - (0000 0000 0000)
     * 4 bytes - ?????
     * 4 bytes - (0400 or 0000)
     */
    public class FileInfo
    {
        private static readonly SHA1 Sha1Generator = SHA1.Create();

        public string Domain { get; private set; }
        public string Path { get; private set; }
        public FileType Type { get; private set; }
        
        // 12 bytes Unknown
        public byte[] Unknown { get; private set; }

        public byte[] FileHash { get; private set; }

        // 4 bytes (Size?)
        public ulong Size { get; private set; }

        // 16 bytes Unknown
        public byte[] Unknown2 { get; private set; }

        // 24 bytes (Date Time?)
        public byte[] DateTime { get; private set; }

        // 16 bytes Unknown
        public byte[] Unknown3 { get; private set; }

        // 16 bytes Unknown
        public byte[] Unknown4 { get; private set; }

        public List<FileProperty> Properties { get; private set; }

        // Domain-Path
        public byte[] FileNameHash
        {
            get { return CreateSha1Hash(Domain + "-" + Path); }
        }

        private static byte[] CreateSha1Hash(string valueToHash)
        {
            var hashBytes = Sha1Generator.ComputeHash(Encoding.ASCII.GetBytes(valueToHash));
            return hashBytes;
        }

        public FileInfo(BackupParser parser)
        {
            Properties = new List<FileProperty>();

            Domain = parser.ReadString();
            Path = parser.ReadString();

            Unknown = parser.ReadStringAsBytes();

            FileHash = parser.ReadStringAsBytes();

            Unknown2 = parser.ReadStringAsBytes();

            Unknown3 = parser.ReadBytes(30);

            Size = parser.ReadInt64();

            Unknown4 = parser.ReadBytes(1);

            var propertyLen = parser.ReadInt8();

            if (propertyLen == 0)
                return;

            for (var i = 0; i < propertyLen; i++)
            {
                var property = new FileProperty(parser);
                Properties.Add(property);
            }
        }

        public override string ToString()
        {
            return Domain + "-" + Path + " (" + (Size / 1024d).ToString("0.00") + " KB)";
        }
    }
}
