using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace text2img
{
    /// <summary>
    /// Text parser for different encodings
    /// </summary>
    public static class TextParser
    {
        private const string CR = "\r";
        private const string LF = "\n";
        private const string CRLF = "\r\n";

        /// <summary>
        /// Checks if an UTF-8 BOM is present
        /// </summary>
        /// <param name="SourceContent">Source stream</param>
        /// <remarks>This will rewind the stream to the position it was before reading if possible</remarks>
        /// <returns>True, if BOM is present</returns>
        public static bool IsUtf8(Stream SourceContent)
        {
            long Pos = SourceContent.Position;
            byte[] Data = new byte[3];
            if (SourceContent.Read(Data, 0, 3) == 3)
            {
                if (SourceContent.CanSeek)
                {
                    SourceContent.Seek(-3, SeekOrigin.Current);
                }
                return
                    Data[0] == 0xEF &&
                    Data[1] == 0xBB &&
                    Data[2] == 0xBF;
            }
            if (SourceContent.CanSeek)
            {
                SourceContent.Position = Pos;
            }
            return false;
        }

        /// <summary>
        /// Reads and converts text from a stream
        /// </summary>
        /// <param name="SourceContent">Text content</param>
        /// <param name="SourceEncoding">Encoding the text is in</param>
        /// <param name="ForceEncoding">Force the encoding (skips UTF-8 check if true)</param>
        /// <returns>C# usable string</returns>
        public static string GetText(Stream SourceContent, Encoding SourceEncoding, bool ForceEncoding)
        {
            Encoding E = null;
            if (!ForceEncoding && SourceEncoding.CodePage != Encoding.UTF8.CodePage && IsUtf8(SourceContent))
            {
                E = Encoding.UTF8;
            }
            else
            {
                E = (Encoding)SourceEncoding.Clone();
            }

            return E.GetString(SourceContent.GetBytes());
        }

        /// <summary>
        /// converts text
        /// </summary>
        /// <param name="Data">Text content</param>
        /// <param name="SourceEncoding">Encoding the text is in</param>
        /// <param name="ForceEncoding">Force the encoding (skips UTF-8 check if true)</param>
        /// <returns>C# usable string</returns>
        public static string GetText(byte[] Data, Encoding SourceEncoding, bool ForceEncoding)
        {
            using (MemoryStream MS = new MemoryStream(Data, false))
            {
                return GetText(MS, SourceEncoding, ForceEncoding);
            }
        }

        /// <summary>
        /// Fixes CRLF issues. (Converts Mac and Linux linebreaks to Windows line breaks)
        /// </summary>
        /// <remarks>This can work with files that contain mixed line breaks</remarks>
        /// <param name="Source">Text to normalize</param>
        /// <returns>CRLF-Fixed text</returns>
        public static string FixCrLf(string Source)
        {
            return Source.Replace(CRLF, CR).Replace(CR[0], LF[0]).Replace(LF, CRLF);
        }
    }
}
