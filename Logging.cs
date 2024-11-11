using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ontroller.WinUSB.IO
{
    internal static partial class Logging
    {
        [LibraryImport("kernel32.dll", EntryPoint = "OutputDebugStringW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial void OutputDebugString(string lpOutputString);

        public static void Write(string str)
        {
            OutputDebugString(str);
        }

        public static void WriteLine(string str)
        {
            OutputDebugString(str + "\n");
        }
    }
}
