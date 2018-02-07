using System;
using System.Runtime.InteropServices;
using System.Linq;

namespace API.Socket.Data.Packet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SHeader
    {
        private byte _tag;
        private ushort _mark;
        private byte _vertify;
        private uint _dataSize;
        private ushort _protocol;
        private ushort _errorCode;
        private ushort _reserved_0;
        private ushort _reserved_1;

        public byte Tag { get { return _tag; } set { _tag = value; } }
        public ushort Mark { get { return _mark; } set { _mark = value; } }
        public byte Vertify { get { return _vertify; } set { _vertify = value; } }
        public uint DataSize { get { return _dataSize; } set { _dataSize = Packet.HeaderSize + value; } }
        public ushort Protocol { get { return _protocol; } set { _protocol = value; } }
        public ushort ErrorCode { get { return _errorCode; } set { _errorCode = value; } }
        public ushort Reserved_0 { get { return _reserved_0; } set { _reserved_0 = value; } }
        public ushort Reserved_1 { get { return _reserved_1; } set { _reserved_1 = value; } }
    };
    public struct SPacket
    {
        private SHeader _header;
        private byte[] _data;
        public SPacket(byte[] data)
        {
            _header = new SHeader();
            _header.Tag = 0x7E;
            _header.Mark = 0x4134;
            _header.Vertify = 1;
            _header.ErrorCode = 0;
            _data = data;
        }
        public byte[] Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new byte[0];
                }
                return _data;
            }
            set => _data = value;
        }
        public void Insert(byte[] b)
        {
            _data = _data.Concat(b).ToArray();
        }
        public byte[] GetBytes()
        {
            byte[] b = new byte[Marshal.SizeOf(_header) + Data.Length];
            IntPtr buff = Marshal.AllocHGlobal(Marshal.SizeOf(_header));
            Marshal.StructureToPtr(_header, buff, true);
            Marshal.Copy(buff, b, 0, Marshal.SizeOf(_header));
            Marshal.FreeHGlobal(buff);
            _data.CopyTo(b, Marshal.SizeOf(_header));
            return b;
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CHeader
    {
        private byte _tag = 0x7E;
        private ushort _mark = 0x4134;
        private byte _vertify;
        private uint _dataSize;
        private ushort _protocol;
        private ushort _errorCode;
        private ushort _reserved_0;
        private ushort _reserved_1;

        public byte Tag { get { return _tag; } set { _tag = value; } }
        public ushort Mark { get { return _mark; } set { _mark = value; } }
        public byte Vertify { get { return _vertify; } set { _vertify = value; } }
        public uint DataSize { get { return _dataSize; } set { _dataSize = Packet.HeaderSize + value; } }
        public ushort Protocol { get { return _protocol; } set { _protocol = value; } }
        public ushort ErrorCode { get { return _errorCode; } set { _errorCode = value; } }
        public ushort Reserved_0 { get { return _reserved_0; } set { _reserved_0 = value; } }
        public ushort Reserved_1 { get { return _reserved_1; } set { _reserved_1 = value; } }
    }

    public class Packet : IDisposable
    {
        public const UInt16 HeaderSize = 16;

        private SHeader _header;
        private byte[] _data;
        public Packet()
        {
            _header.Tag = 0x7E;
            _header.Mark = 0x4134;
            _header.Vertify = 1;
            _header.ErrorCode = 0;
        }
        public Packet(byte[] b)
        {
            _header.Tag = 0x7E;
            _header.Mark = 0x4134;
            _header.Vertify = 1;
            _header.ErrorCode = 0;

            IntPtr buffer = Marshal.AllocHGlobal(HeaderSize);
            Marshal.Copy(b, 0, buffer, HeaderSize);
            _header = (SHeader)Marshal.PtrToStructure(buffer, typeof(SHeader));
            Marshal.FreeHGlobal(buffer);
        }
        public void SetHeader(SHeader s)
        {
            _header = s;
        }
        public ref SHeader GetHeader()
        {
            return ref _header;
        }

        public byte[] Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new byte[0];
                }
                return _data;
            }
            set => _data = value;
        }
        public void Insert(byte[] b)
        {
            _data = _data.Concat(b).ToArray();
        }
        public byte[] GetBytes()
        {
            byte[] b = new byte[HeaderSize + Data.Length];

            IntPtr buff = Marshal.AllocHGlobal(HeaderSize);
            Marshal.StructureToPtr(_header, buff, true);
            Marshal.Copy(buff, b, 0, HeaderSize);
            Marshal.FreeHGlobal(buff);
            _data.CopyTo(b, HeaderSize);
            return b;
        }
        public void Dispose()
        {
            _data = null;
        }
    }
}
