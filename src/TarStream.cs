using System;
using System.IO;
using System.Text;


namespace SatorImaging.TarArchiver
{
    public class TarStream : IDisposable
    {
        Stream m_outputStream;

        public TarStream(Stream output)
        {
            m_outputStream = output;
        }

        public void Dispose()
        {
            /// Indicates if archive should be finalized (by 2 empty blocks) on close.
            m_outputStream.Write(new byte[1024]);
            m_outputStream.Flush();
            // parent stream keep open for further op
            //m_outputStream.Close();
        }


        public void Flush() => m_outputStream.Flush();
        public void Close() => Dispose();


        public void Write(string filename, string source) => Write(filename, source, DateTime.Now);
        public void Write(string filename, string source, DateTime modificationTime, long size = -1)
        {
            Write(filename, Encoding.UTF8.GetBytes(source), modificationTime, size);
        }

        public void Write(string filename, byte[] source) => Write(filename, source, DateTime.Now);
        public void Write(string filename, byte[] source, DateTime modificationTime, long size = -1)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(source);
                stream.Flush();
                stream.Position = 0;
                Write(filename, stream, modificationTime, size);
            }
        }

        public void Write(string filename, Stream source) => Write(filename, source, DateTime.Now);
        public void Write(string filename, Stream source, DateTime modificationTime, long size = -1)
        {
            if (!source.CanSeek && size < 0)
            {
                throw new ArgumentException("Seekable stream is required if no size is given.");
            }

            long realSize = size < 0 ? source.Length : size;

            TarHeader header = new TarHeader();

            header.LastModifiedTime = modificationTime;
            header.Name = NormalizeFilename(filename);
            header.Size = realSize;
            header.Write(m_outputStream);

            size = TransferTo(source, m_outputStream);
            PadTo512(size);
        }



        #region ////////  Utility  ////////


        void PadTo512(long size)
        {
            int zeros = unchecked((int)(((size + 511L) & ~511L) - size));
            m_outputStream.Write(new byte[zeros]);
        }

        string NormalizeFilename(string filename)
        {
            filename = filename.Replace('\\', '/');

            int pos = filename.IndexOf(':');
            if (pos >= 0)
            {
                filename = filename.Remove(0, pos + 1);
            }

            return filename.Trim('/');
        }


        static byte[] GetTransferByteArray() => new byte[81920]; //ArrayPool<byte>.Shared.Rent(81920);

        static bool ReadTransferBlock(Stream source, byte[] array, out int count) =>
            (count = source.Read(array, 0, array.Length)) != 0;

        static long TransferTo(/*this*/ Stream source, Stream destination)
        {
            var array = GetTransferByteArray();
            try
            {
                long total = 0;
                while (ReadTransferBlock(source, array, out var count))
                {
                    total += count;
                    destination.Write(array, 0, count);
                }
                return total;
            }
            catch
            {
                throw;
            }
            finally
            {
                //ArrayPool<byte>.Shared.Return(array);
            }
        }


        #endregion


    }
}
