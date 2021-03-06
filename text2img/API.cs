﻿using System;
using System.Runtime.InteropServices;

namespace text2img
{
    /// <summary>
    /// Provides additional console features
    /// </summary>
    public static class ConsoleEx
    {
        /// <summary>
        /// Returns true, if the output stream is redirected
        /// </summary>
        public static bool IsOutputRedirected
        {
            get { return FileType.Char != GetFileType(GetStdHandle(StdHandle.Stdout)); }
        }
        /// <summary>
        /// Returns true, if the input stream is redirected
        /// </summary>
        public static bool IsInputRedirected
        {
            get { return FileType.Char != GetFileType(GetStdHandle(StdHandle.Stdin)); }
        }
        /// <summary>
        /// Returns true, if the error stream is redirected
        /// </summary>
        public static bool IsErrorRedirected
        {
            get { return FileType.Char != GetFileType(GetStdHandle(StdHandle.Stderr)); }
        }

        // P/Invoke:
        private enum FileType { Unknown, Disk, Char, Pipe };
        private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
        [DllImport("kernel32.dll")]
        private static extern FileType GetFileType(IntPtr hdl);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);
    }
}