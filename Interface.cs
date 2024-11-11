using Ontroller.WinUSB.IO.Types;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ontroller.WinUSB.IO
{
    public unsafe static class Interface
    {
        private static Connection? _connection;
        private static SharedLedBuffer? _ledBuffer;

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_api_version", CallConvs = [typeof(CallConvCdecl)])]
        public static ushort GetApiVersion() => 0x0101;

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_init", CallConvs = [typeof(CallConvCdecl)])]
        public static int Init()
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            Logging.WriteLine($"Ontroller: Init from {processName}");

            if (_ledBuffer == null)
                _ledBuffer = new SharedLedBuffer();

            if (!processName.ToLower().Contains("amdaemon"))
                return 0;

            if (_connection == null)
                _connection = new Connection();

            _connection.TryConnect();
            return 0;
        }

        private static int last_retry = 0;

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_poll", CallConvs = [typeof(CallConvCdecl)])]
        public static int Poll()
        {
            if (_connection == null)
                return 1;

            if (!_connection.Connected)
            {
                if (last_retry > 600)
                {
                    _connection.TryConnect();
                    last_retry = 0;
                }

                last_retry++;
            }

            if (_ledBuffer != null)
                _connection.SetSideLeds(_ledBuffer.GetData());

            _connection.Write();
            return 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_opbtns", CallConvs = [typeof(CallConvCdecl)])]
        public static void GetOperationButtons(OperationButton* opbtn)
        {
            if (_connection == null)
            {
                *opbtn = OperationButton.None;
                return;
            }

            *opbtn = _connection.Operation;
        }

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_gamebtns", CallConvs = [typeof(CallConvCdecl)])]
        public static void GetGameButtons(GameButton* left, GameButton* right)
        {
            if (_connection == null)
            {
                *left = GameButton.None;
                *right = GameButton.None;
                return;
            }

            *left = _connection.Left;
            *right = _connection.Right;
        }

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_lever", CallConvs = [typeof(CallConvCdecl)])]
        public static void GetGameButtons(short* pos)
        {
            if (_connection == null)
            {
                *pos = 0;
                return;
            }

            *pos = _connection.Lever;
        }

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_led_init", CallConvs = [typeof(CallConvCdecl)])]
        public static int LedInit()
        {
            Logging.WriteLine($"Ontroller: Led Init");
            // does nothing...
            return 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "mu3_io_led_set_colors", CallConvs = [typeof(CallConvCdecl)])]
        public static void SetLedColors(byte board, byte* rgb)
        {
            if (_ledBuffer == null)
                return;

            // amdaemon
            if (board == 1 && _connection != null)
            {
                ReadOnlySpan<byte> leds = new(rgb, 3 * 6);
                _connection.SetIO4Leds(leds);
                return;
            }

            // game
            if (board == 0)
            {
                ReadOnlySpan<byte> leds = new(rgb, 3 * 61);

                byte[] data = new byte[12];
                leds[..6].CopyTo(new Span<byte>(data, 0, 6));
                leds[177..].CopyTo(new Span<byte>(data, 6, 6));

                _ledBuffer.SetData(data);
                return;
            }
        }
    }
}
