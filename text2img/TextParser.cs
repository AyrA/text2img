using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace text2img
{
    public static class TextParser
    {
        private const string CR = "\r";
        private const string LF = "\n";
        private const string CRLF = "\r\n";

        public static bool IsUtf8(Stream SourceContent)
        {
            long Pos = SourceContent.Position;
            byte[] Data = new byte[3];
            if (SourceContent.Read(Data, 0, 3) == 3)
            {
                SourceContent.Seek(-3, SeekOrigin.Current);
                return
                    Data[0] == 0xEF &&
                    Data[1] == 0xBB &&
                    Data[2] == 0xBF;
            }
            SourceContent.Position = Pos;
            return false;
        }

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

        public static string GetText(byte[] Data, Encoding SourceEncoding, bool ForceEncoding)
        {
            using (MemoryStream MS = new MemoryStream(Data, false))
            {
                return GetText(MS, SourceEncoding, ForceEncoding);
            }
        }

        public static string FixCrLf(string Source)
        {
            return Source.Replace(CRLF, CR).Replace(CR[0], LF[0]).Replace(LF, CRLF);
        }
    }
}
