using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace text2img
{
    /// <summary>
    /// Provides stream Functionality
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Default stream buffer
        /// </summary>
        private const int DEF_BUFFER = 1024 * 1024;

        /// <summary>
        /// Copies a stream onto another
        /// </summary>
        /// <param name="Source">Source stream</param>
        /// <param name="Destination">Destination stream</param>
        /// <remarks>This will not seek back</remarks>
        /// <returns>Number of bytes copied</returns>
        public static long CopyTo(this Stream Source, Stream Destination)
        {
            return CopyTo(Source, Destination, DEF_BUFFER);
        }

        /// <summary>
        /// Copies a stream onto another
        /// </summary>
        /// <param name="Source">Source stream</param>
        /// <param name="Destination">Destination stream</param>
        /// <param name="BufferSize">Size of the copy buffer</param>
        /// <remarks>This will not seek back</remarks>
        /// <returns>Number of bytes copied</returns>
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

        /// <summary>
        /// MemoryStream.ToArray() for all streams
        /// </summary>
        /// <param name="Source">Stream to read</param>
        /// <remarks>Will not seek back after reading</remarks>
        /// <returns>Byte array with stream contents</returns>
        public static byte[] GetBytes(this Stream Source)
        {
            return GetBytes(Source, DEF_BUFFER);
        }

        /// <summary>
        /// MemoryStream.ToArray() for all streams
        /// </summary>
        /// <param name="Source">Stream to read</param>
        /// <param name="BufferSize">The read buffer size</param>
        /// <remarks>Will not seek back after reading</remarks>
        /// <returns>Byte array with stream contents</returns>
        public static byte[] GetBytes(this Stream Source, int BufferSize)
        {
            if (Source is MemoryStream)
            {
                return ((MemoryStream)Source).ToArray();
            }
            using (MemoryStream MS = new MemoryStream())
            {
                Source.CopyTo(MS, BufferSize);
                return MS.ToArray();
            }
        }
    }
}
