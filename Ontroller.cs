using Nefarius.Drivers.WinUSB;
using Nefarius.Utilities.DeviceManagement.PnP;
using Ontroller.WinUSB.IO.Types;
using System.IO;

namespace Ontroller.WinUSB.IO
{
    public class OntrollerIOException(string message) : Exception(message);

    public class Connection : IDisposable
    {
        private static readonly Guid USB_DEVICE_GUID = Guid.Parse("{A5DCBF10-6530-11D2-901F-00C04FB951ED}");
        private USBDevice? _device;
        private Thread? _readThread;

        private byte[] _inBuffer = new byte[7];
        private byte[] _outBuffer = new byte[33];

        public bool Connected => _device != null;
        public GameButton Left
        {
            get
            {
                if (!Connected)
                    return GameButton.None;

                GameButton result = GameButton.None;

                if ((_inBuffer[3] & 0x20) != 0)
                    result |= GameButton.A;

                if ((_inBuffer[3] & 0x10) != 0)
                    result |= GameButton.B;

                if ((_inBuffer[3] & 8) != 0)
                    result |= GameButton.C;

                if ((_inBuffer[4] & 0x80) != 0)
                    result |= GameButton.Side;

                if ((_inBuffer[4] & 0x20) != 0)
                    result |= GameButton.Menu;

                return result;
            }
        }

        public GameButton Right
        {
            get
            {
                if (!Connected)
                    return GameButton.None;

                GameButton result = GameButton.None;

                if ((_inBuffer[3] & 4) != 0)
                    result |= GameButton.A;

                if ((_inBuffer[3] & 2) != 0)
                    result |= GameButton.B;

                if ((_inBuffer[3] & 1) != 0)
                    result |= GameButton.C;

                if ((_inBuffer[4] & 0x40) != 0)
                    result |= GameButton.Side;

                if ((_inBuffer[4] & 0x10) != 0)
                    result |= GameButton.Menu;

                return result;
            }
        }

        public OperationButton Operation
        {
            get
            {
                if (!Connected)
                    return OperationButton.None;

                OperationButton result = OperationButton.None;

                if ((_inBuffer[4] & 8) != 0)
                    result |= OperationButton.Test;

                if ((_inBuffer[4] & 4) != 0)
                    result |= OperationButton.Service;

                // no coin button on ontroller

                return result;
            }
        }

        public short Lever
        {
            get
            {
                if (!Connected)
                    return 0;

                var raw = (ushort)(_inBuffer[5] << 8 | _inBuffer[6]);
                return (short)(raw * 80 - short.MaxValue);
            }
        }

        public void SetIO4Leds(ReadOnlySpan<byte> leds)
        {
            if (leds.Length != 3 * 6)
                return;

            leds.CopyTo(new Span<byte>(_outBuffer, 3, 18));
        }

        public void SetSideLeds(ReadOnlySpan<byte> leds)
        {
            if (leds.Length != 3 * 4)
                return;

            leds.CopyTo(new Span<byte>(_outBuffer, 21, 12));
        }

        string? FindOntroller()
        {
            int instance = 0;

            while (Devcon.FindByInterfaceGuid(USB_DEVICE_GUID, out var path, out _, instance++))
            {
                if (path.Contains("VID_0E8F") && path.Contains("PID_1216"))
                {
                    Logging.WriteLine($"Ontroller: Device path is {path}");
                    return path;
                }
            }

            return null;
        }

        public bool TryConnect()
        {
            if (Connected) 
                return true;

            var devicePath = FindOntroller();
            if (devicePath == null)
                return false;

            try
            {
                _device = USBDevice.GetSingleDeviceByPath(devicePath);
            }
            catch (Exception e)
            {
                Logging.WriteLine($"Ontroller: Unable to connect: {e.Message}\n{e.StackTrace}");

                _device = null;
                return false;
            }

            Logging.WriteLine("Ontroller: Connected");

            if (_readThread != null && _readThread.IsAlive)
                _readThread.Join();

            _readThread = new Thread(ReadThread);
            _readThread.Start();

            _outBuffer[0] = 0x44;
            _outBuffer[1] = 0x4C;
            _outBuffer[2] = 1;

            return true;
        }

        public void ReadThread()
        {
            Logging.WriteLine($"Ontroller: Read thread start");

            while (Connected)
            {
                Read();
            }

            Logging.WriteLine($"Ontroller: Read thread stop");
        }

        public void Read()
        {
            if (_device == null)
                return;

            try
            {
                var inPipe = _device.Pipes[0x84];

                inPipe.Read(_inBuffer);
                if (_inBuffer[0] != 0x44 || _inBuffer[1] != 0x44 || _inBuffer[2] != 0x54)
                {
                    Logging.WriteLine("Ontroller: Device output is incorrect, are you trolling?");

                    throw new OntrollerIOException("Invalid input message");
                }
            }
            catch (USBException)
            {
                Logging.WriteLine("Ontroller: Device is disconnected.");

                _device.Dispose();
                _device = null;
            }
        }

        public void Write()
        {
            if (_device == null)
                return;

            try
            {
                var outPipe = _device.Pipes[0x03];
                outPipe.Write(_outBuffer);
            }
            catch (USBException)
            {
                Logging.WriteLine("Ontroller: Device is disconnected.");

                _device.Dispose();
                _device = null;
            }
        }

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }
        }
    }
}
