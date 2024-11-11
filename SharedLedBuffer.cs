using System.IO.MemoryMappedFiles;

namespace Ontroller.WinUSB.IO
{
    internal class SharedLedBuffer
    {
        MemoryMappedFile _file;
        MemoryMappedViewAccessor _accessor;

        byte[] _buffer = new byte[12];

        public SharedLedBuffer()
        {
            _file = MemoryMappedFile.CreateOrOpen("ontroller_ipc_led_buffer", 12);
            _accessor = _file.CreateViewAccessor();

            Logging.WriteLine($"Ontroller: Led Shard Buffer initialized");
        }

        public void SetData(byte[] data)
        {
            _accessor.WriteArray(0, data, 0, 12);
        }

        public ReadOnlySpan<byte> GetData()
        {
            _accessor.ReadArray(0, _buffer, 0, 12);
            return _buffer;
        }
    }
}
