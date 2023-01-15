using System;
using System.IO;
using System.Text;


namespace SatorImaging.TarArchiver
{
    public class TarHeader
    {
        const int BLOCK_SIZE = 512;
        static DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        enum EEntryType : byte
        {
            File = 0,
            OldFile = (byte)'0',
            HardLink = (byte)'1',
            SymLink = (byte)'2',
            CharDevice = (byte)'3',
            BlockDevice = (byte)'4',
            Directory = (byte)'5',
            Fifo = (byte)'6',
            LongLink = (byte)'K',
            LongName = (byte)'L',
            SparseFile = (byte)'S',
            VolumeHeader = (byte)'V',
            GlobalExtendedHeader = (byte)'g'
        }

        EEntryType EntryType;
        Encoding ArchiveEncoding = Encoding.UTF8;


        internal string Name { get; set; }
        internal long Size { get; set; }
        internal DateTime LastModifiedTime { get; set; }


        internal void Write(Stream output)
        {
            byte[] buffer = new byte[BLOCK_SIZE];

            WriteOctalBytes(511, buffer, 100, 8); // file mode
            WriteOctalBytes(0, buffer, 108, 8); // owner ID
            WriteOctalBytes(0, buffer, 116, 8); // group ID

            //ArchiveEncoding.UTF8.GetBytes("magic").CopyTo(buffer, 257);
            var nameByteCount = ArchiveEncoding.GetByteCount(Name);//.GetEncoding().GetByteCount(Name);
            if (nameByteCount > 100)
            {
                // Set mock filename and filetype to indicate the next block is the actual name of the file
                WriteStringBytes("././@LongLink", buffer, 0, 100);
                buffer[156] = (byte)EEntryType.LongName;
                WriteOctalBytes(nameByteCount + 1, buffer, 124, 12);
            }
            else
            {
                WriteStringBytes(ArchiveEncoding.GetBytes(Name), buffer, 100);
                WriteOctalBytes(Size, buffer, 124, 12);
                var time = (long)(LastModifiedTime.ToUniversalTime() - EPOCH).TotalSeconds;
                WriteOctalBytes(time, buffer, 136, 12);
                buffer[156] = (byte)EntryType;

                if (Size >= 0x1FFFFFFFF)
                {
                    Span<byte> bytes12 = stackalloc byte[12];
                    BinaryPrimitives_WriteInt64BigEndian(bytes12.Slice(4), Size);
                    bytes12[0] |= 0x80;
                    bytes12.CopyTo(buffer.AsSpan(124));
                }
            }

            int crc = RecalculateChecksum(buffer);
            WriteOctalBytes(crc, buffer, 148, 8);

            output.Write(buffer, 0, buffer.Length);

            if (nameByteCount > 100)
            {
                WriteLongFilenameHeader(output);
                // update to short name lower than 100 - [max bytes of one character].
                // subtracting bytes is needed because preventing infinite loop(example code is here).
                //
                // var bytes = Encoding.UTF8.GetBytes(new string(0x3042, 100));
                // var truncated = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(bytes, 0, 100));
                //
                // and then infinite recursion is occured in WriteLongFilenameHeader because truncated.Length is 102.
                Name = ArchiveEncoding.GetString(ArchiveEncoding.GetBytes(Name), 0, 100 - ArchiveEncoding.GetMaxByteCount(1));
                Write(output);
            }
        }



        #region ////////  Utility  ////////


        static void BinaryPrimitives_WriteInt64BigEndian(Span<byte> destination, long value)
        {
            var bytes = BitConverter.GetBytes(value).AsSpan<byte>();
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse<byte>();
            }
            bytes.CopyTo(destination);
        }


        static void WriteOctalBytes(long value, byte[] buffer, int offset, int length)
        {
            var val = Convert.ToString(value, 8);
            var shift = length - val.Length - 1;
            for (var i = 0; i < shift; i++)
            {
                buffer[offset + i] = (byte)' ';
            }
            for (var i = 0; i < val.Length; i++)
            {
                buffer[offset + i + shift] = (byte)val[i];
            }
        }


        static void WriteStringBytes(ReadOnlySpan<byte> name, Span<byte> buffer, int length)
        {
            name.CopyTo(buffer);
            var i = Math.Min(length, name.Length);
            buffer.Slice(i, length - i).Clear();
        }

        static void WriteStringBytes(string name, byte[] buffer, int offset, int length)
        {
            int i;

            for (i = 0; i < length && i < name.Length; ++i)
            {
                buffer[offset + i] = (byte)name[i];
            }

            for (; i < length; ++i)
            {
                buffer[offset + i] = 0;
            }
        }


        void WriteLongFilenameHeader(Stream output)
        {
            var nameBytes = ArchiveEncoding.GetBytes(Name);//.Encode(Name);
            output.Write(nameBytes, 0, nameBytes.Length);

            // pad to multiple of BlockSize bytes, and make sure a terminating null is added
            var numPaddingBytes = BLOCK_SIZE - (nameBytes.Length % BLOCK_SIZE);
            if (numPaddingBytes == 0)
            {
                numPaddingBytes = BLOCK_SIZE;
            }
            output.Write(new byte[numPaddingBytes]);
        }


        static readonly byte[] eightSpaces =
        {
                (byte)' ',
                (byte)' ',
                (byte)' ',
                (byte)' ',
                (byte)' ',
                (byte)' ',
                (byte)' ',
                (byte)' ',
            };

        static int RecalculateChecksum(byte[] buf)
        {
            // Set default value for checksum. That is 8 spaces.
            eightSpaces.CopyTo(buf, 148);

            // Calculate checksum
            var headerChecksum = 0;
            foreach (var b in buf)
            {
                headerChecksum += b;
            }
            return headerChecksum;
        }


        #endregion



    }
}
