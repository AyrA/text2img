using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Drawing.Imaging;

namespace text2img
{
    class Program
    {
        /// <summary>
        /// Default codepage to use
        /// </summary>
        public const int DEFAULT_CP = 437;
        /// <summary>
        /// Argument to use console input/output
        /// </summary>
        public const string STDIO = "-";

        /// <summary>
        /// Command line argument structure
        /// </summary>
        private struct Arguments
        {
            /// <summary>
            /// Input file
            /// </summary>
            public string InFile;
            /// <summary>
            /// Output file
            /// </summary>
            public string OutFile;
            /// <summary>
            /// Arguments valid
            /// </summary>
            public bool Valid;
            /// <summary>
            /// Help requested
            /// </summary>
            public bool HelpRequest;
            /// <summary>
            /// Codepage list requested
            /// </summary>
            public bool List;
            /// <summary>
            /// User supplied Codepage
            /// </summary>
            public Encoding Codepage;

            /// <summary>
            /// Initializes defaults
            /// </summary>
            public void Init()
            {
                InFile = OutFile = null;
                Valid = true;
                HelpRequest = List = false;
                Codepage = null;
            }

        }

        /// <summary>
        /// Possible exit codes
        /// </summary>
        private enum EXITCODE : int
        {
            /// <summary>
            /// No errors
            /// </summary>
            SUCCESS = 0,
            /// <summary>
            /// No arguments provided
            /// </summary>
            NO_ARGS = SUCCESS + 1,
            /// <summary>
            /// Invalid/Unsupported argument
            /// </summary>
            INVALID_ARGS = NO_ARGS + 1,
            /// <summary>
            /// Cannot read source file
            /// </summary>
            ERR_READ = INVALID_ARGS + 1,
            /// <summary>
            /// Cannot write destination file
            /// </summary>
            ERR_WRITE = ERR_READ + 1,
            /// <summary>
            /// Invalid codepage specified
            /// </summary>
            ERR_CP = ERR_WRITE + 1,
            /// <summary>
            /// Help requested
            /// </summary>
            HELP_REQUEST = ERR_CP + 1,
            /// <summary>
            /// Codepage list requested
            /// </summary>
            LIST_REQUEST = HELP_REQUEST + 1
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Exit code</returns>
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Help(EXITCODE.NO_ARGS);
            }
            Arguments A = ParseArgs(args);

            if (!A.Valid)
            {
                return (int)EXITCODE.INVALID_ARGS;
            }

            if (A.List)
            {
                List(true);
            }

            if (A.HelpRequest)
            {
                Help(EXITCODE.HELP_REQUEST);
            }

            Console.Error.WriteLine("Converting...");

            byte[] Data;
            try
            {
                if (A.InFile == STDIO)
                {
                    using (var Str = Console.OpenStandardInput())
                    {
                        Data = Str.GetBytes(Console.BufferWidth * Console.BufferHeight - 1);
                    }
                    Console.Error.WriteLine("Read {0} bytes from console input", Data.Length);
                }
                else
                {
                    Data = File.ReadAllBytes(A.InFile);
                }
            }
            catch
            {
                Console.Error.WriteLine("Cannot read source file");
                return (int)EXITCODE.ERR_READ;
            }

            string Text = TextParser.FixCrLf(TextParser.GetText(Data, A.Codepage == null ? Encoding.GetEncoding(DEFAULT_CP) : A.Codepage, A.Codepage == null));

            var B = ImageTools.RenderString(Text);
            if (B != null)
            {
                if (A.OutFile == STDIO)
                {
                    Console.Error.WriteLine("Writing to Console output");
                    using (var Str = Console.OpenStandardOutput())
                    {
                        using (var Temp = new MemoryStream())
                        {
                            B.Save(Temp, ImageFormat.Png);
                            Temp.Position = 0;
                            Temp.CopyTo(Str, Console.BufferWidth * Console.BufferHeight - 1);
                        }
                    }
                }
                else
                {
                    B.Save(A.OutFile);
                }
            }
            else
            {
                Console.Error.WriteLine("Error creating bitmap.");
                return (int)EXITCODE.ERR_WRITE;
            }

#if DEBUG
            if (!ConsoleEx.IsInputRedirected)
            {
                Console.Error.WriteLine("#END");
                Console.ReadKey(true);
            }
#endif
            return (int)EXITCODE.SUCCESS;
        }

        /// <summary>
        /// Parses and validates command line arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Argument structure</returns>
        private static Arguments ParseArgs(string[] args)
        {
            Arguments A = new Arguments();
            A.Init();

            foreach (string arg in args)
            {
                //Just stop parsing once it becomes invalid
                if (A.Valid)
                {
                    string U = arg.ToUpper();
                    if (U == "/?" || U == "--HELP" || U == "-?")
                    {
                        A.Init();
                        A.HelpRequest = true;
                        return A;
                    }
                    if (U == "/L")
                    {
                        A.Init();
                        A.List = true;
                        return A;
                    }


                    if (U.StartsWith("/C:"))
                    {
                        if (A.Valid &= (U.Length > 3))
                        {
                            int cp = 0;
                            //check if codepage is an integer
                            if (int.TryParse(U.Substring(3), out cp))
                            {
                                try
                                {
                                    A.Codepage = Encoding.GetEncoding(cp);
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine("Codepage parse error: {0}", ex.Message);
                                    A.Valid = false;
                                }
                            }
                            else
                            {
                                try
                                {
                                    A.Codepage = Encoding.GetEncoding(U.Substring(3));
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine("Codepage parse error: {0}", ex.Message);
                                    A.Valid = false;
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid codepage argument. Use /? for help");
                        }
                    }
                    else
                    {
                        //first unknown argument is input file, second (optional) is output file
                        if (string.IsNullOrEmpty(A.InFile))
                        {
                            if (!File.Exists(A.InFile = arg) && A.InFile != STDIO)
                            {
                                Console.Error.WriteLine("Input file not found");
                                A.Valid = false;
                            }
                        }
                        else if (string.IsNullOrEmpty(A.OutFile))
                        {
                            if (arg == STDIO)
                            {
                                A.OutFile = arg;
                            }
                            else
                            {
                                switch (U.Split('.').Last())
                                {
                                    case "PNG":
                                    case "JPG":
                                    case "JPEG":
                                    case "BMP":
                                    case "GIF":
                                        A.OutFile = arg;
                                        break;
                                    default:
                                        Console.Error.WriteLine("Only PNG, JPG, BMP and GIF are supported output formats");
                                        A.Valid = false;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Too many arguments. Use /? for help. Note: put your argument insite quotes (\"...\") if it contains special chars or spaces.");
                            A.Valid = false;
                        }
                    }
                }
                else
                {
                    return A;
                }
            }


            //Autogenerate output file if not specified
            if (A.Valid && string.IsNullOrEmpty(A.OutFile))
            {
                if (A.InFile == STDIO)
                {
                    A.OutFile = STDIO;
                }
                else
                {
                    FileInfo FI = new FileInfo(A.InFile);
                    //if file name without extension, we don't need to cut anything.
                    if (string.IsNullOrEmpty(FI.Extension))
                    {
                        A.OutFile = A.InFile + ".png";
                    }
                    else
                    {
                        A.OutFile = A.InFile.Substring(0, A.InFile.Length - FI.Extension.Length) + ".png";
                    }
                }
            }
            return A;
        }

        /// <summary>
        /// Shows Codepage list
        /// </summary>
        /// <param name="Exit">true to exit application with 'LIST_REQUEST' code</param>
        private static void List(bool Exit)
        {
            Console.Error.WriteLine("List of codepages");
            Console.Error.WriteLine("{0,-10} {1,-30} {2}", "ID", "NAME", "DESCRIPTION");
            Console.Error.Write(string.Empty.PadRight(Console.BufferWidth, '-'));
            foreach (var E in Encoding.GetEncodings())
            {
                Console.Error.WriteLine("{0,-10} {1,-30} {2}", E.CodePage, E.Name, E.DisplayName);
            }
            Console.Error.Write(string.Empty.PadRight(Console.BufferWidth, '-'));
            Console.Error.WriteLine("You can use the ID or the NAME for codepages");
            if (Exit)
            {
                Environment.Exit((int)EXITCODE.LIST_REQUEST);
            }
        }

        /// <summary>
        /// Shows Help
        /// </summary>
        private static void Help()
        {
            Console.Error.WriteLine(@"text2img <infile> [/c:codepage] [outfile]

infile   - text file to read. Use '{1}' for console input.
/c       - codepage of the file. If not specified, the application assumes the
           file to be encoded in codepage {0}, unless an UTF-8 BOM is present.
outfile  - Image file to write. If not specified, the file name and location
           of the input file is used (will replace file extension with 'PNG').
           Use '{1}' for console output. If infile is '{1}', outfile will be
           set to '{1}' too if not specified on the command line.

If you are interested in a list of all supported codepages, use /L. This will
ignore all other arguments.
The codepage argument can either be the name or the ID.", DEFAULT_CP, STDIO);
        }

        /// <summary>
        /// Shows help and exits with the given exit code
        /// </summary>
        /// <param name="ExitCode">Exit code</param>
        private static void Help(EXITCODE ExitCode)
        {
            Help();
            Environment.Exit((int)ExitCode);
        }
    }
}
