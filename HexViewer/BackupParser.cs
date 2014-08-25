using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HexViewer
{
    public class BackupParser
    {
        private readonly byte[] _data;

        public int Offset { get; private set; }

        public int Length
        {
            get { return _data.Length; }
        }

        public BackupParser(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            _data = File.ReadAllBytes(file);
        }

        private bool IsValidRequest(int size)
        {
            return Offset + size <= Length;
        }

        public bool HasReachedEof()
        {
            return Offset >= Length;
        }

        public ushort ReadInt8()
        {
            if (!IsValidRequest(1))
                throw new BytesNotAvailableException("ReadInt8", 1);

            var bytes = ReadBytes(1);
            return bytes[0];
        }

        public ushort ReadInt16()
        {
            if (!IsValidRequest(2))
                throw new BytesNotAvailableException("ReadInt16", 2);

            var bytes = ReadBytes(2);
            var result = BitConverter.ToUInt16(bytes, 0);

            var b1 = (result >> 0) & 0xff;
            var b2 = (result >> 8) & 0xff;
            result = (ushort)(b1 << 8 | b2 << 0);

            return result;
        }

        public uint ReadInt32()
        {
            if (!IsValidRequest(4))
                throw new BytesNotAvailableException("ReadInt32", 4);

            var bytes = ReadBytes(4);
            var result = BitConverter.ToUInt32(bytes, 0);

            var b1 = (result >> 0) & 0xff;
            var b2 = (result >> 8) & 0xff;
            var b3 = (result >> 16) & 0xff;
            var b4 = (result >> 24) & 0xff;
            result = (b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0);

            return result;
        }

        public ulong ReadInt64()
        {
            if (!IsValidRequest(8))
                throw new BytesNotAvailableException("ReadInt64", 4);

            var bytes = ReadBytes(8);
            var result = BitConverter.ToUInt64(bytes, 0);

            var b1 = (result >> 0) & 0xff;
            var b2 = (result >> 8) & 0xff;
            var b3 = (result >> 16) & 0xff;
            var b4 = (result >> 24) & 0xff;
            var b5 = (result >> 32) & 0xff;
            var b6 = (result >> 40) & 0xff;
            var b7 = (result >> 48) & 0xff;
            var b8 = (result >> 56) & 0xff;
            result = (b1 << 56 | b2 << 48 | b3 << 40 | b4 << 32 | b5 << 24 | b6 << 16 | b7 << 8 | b8 << 0);

            return result;
        }

        public byte[] ReadBytes(int size)
        {
            if (!IsValidRequest(size))
                throw new BytesNotAvailableException("ReadBytes", size);

            var result = new byte[size];

            for (var i = 0; i < size; i++)
            {
                result[i] = _data[Offset + i];
            }

            Offset += size;

            return result;
        }

        public string ReadString()
        {
            byte[] bytes;

            try
            {
                bytes = ReadStringAsBytes();
            }
            catch (Exception ex)
            {
                throw new BytesNotAvailableException("ReadString", -1, ex);
            }

            return (bytes.Length != 0) ? Encoding.ASCII.GetString(bytes) : "";
        }

        public string ReadString(int size)
        {
            byte[] bytes;

            try
            {
                bytes = ReadBytes(size);
            }
            catch (Exception ex)
            {
                throw new BytesNotAvailableException("ReadString", size, ex);
            }

            var result = Encoding.ASCII.GetString(bytes);
            return result;
        }

        public byte[] ReadStringAsBytes()
        {
            var stringSize = -1;

            try
            {
                stringSize = ReadInt16();
            }
            catch (Exception ex)
            {
                throw new BytesNotAvailableException("ReadStringAsBytes", stringSize, ex);
            }

            return ReadStringAsBytes(stringSize);
        }

        public byte[] ReadStringAsBytes(int size)
        {
            if (size <= 0 || size == UInt16.MaxValue)
                return new byte[0];

            if (!IsValidRequest(size))
                throw new BytesNotAvailableException("ReadStringAsBytes", size);

            byte[] bytes;

            try
            {
                bytes = ReadBytes(size);
            }
            catch (Exception ex)
            {
                throw new BytesNotAvailableException("ReadString", size, ex);
            }

            return bytes;
        }

        public FileType ReadFileType()
        {
            var fileType = FileType.Unknown;

            try
            {
                var typeInt = ReadInt16();

                switch (typeInt)
                {
                    case (ushort)FileType.Directory:
                        fileType = FileType.Directory;
                        break;
                    case (ushort)FileType.File:
                        fileType = FileType.File;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new BytesNotAvailableException("ReadFileType", 2, ex);
            }

            return fileType;
        }
    }
}
