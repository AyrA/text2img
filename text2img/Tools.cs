using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace text2img
{
    public static class Tools
    {
        private const int DEF_BUFFER = 1024 * 1024;

        public static long CopyTo(this Stream Source, Stream Destination)
        {
            return CopyTo(Source, Destination, DEF_BUFFER);
        }

        public static long CopyTo(this Stream Source, Stream Destination, int BufferSize)
        {
            long total=0;
            int i = 0;
            byte[] Data = new byte[BufferSize];
            do
            {
                Destination.Write(Data, 0, i = Source.Read(Data, 0, BufferSize));
                total += i;
            } while (i > 0);
            return total;
        }

        public static byte[] GetBytes(this Stream Source)
        {
            return GetBytes(Source, DEF_BUFFER);
        }

        public static byte[] GetBytes(this Stream Source, int BufferSize)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                Source.CopyTo(MS, BufferSize);
                return MS.ToArray();
            }
        }
    }
}
