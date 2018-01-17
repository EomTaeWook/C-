using System;
using System.Runtime.InteropServices;
using System.Linq;

namespace API.Socket.Data.Packet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SHeader
    {
        private byte tag;
        private ushort mark;
        private byte vertify;
        private uint dataSize;
        private ushort protocol;
        private ushort errorCode;
        private ushort reserved_0;
        private ushort reserved_1;

        public byte Tag { get { return tag; } set { tag = value; } }
        public ushort Mark { get { return mark; } set { mark = value; } }
        public byte Vertify { get { return vertify; } set { vertify = value; } }
        public uint DataSize { get { return dataSize; } set { dataSize = Packet.HeaderSize + value; } }
        public ushort Protocol { get { return protocol; } set { protocol = value; } }
        public ushort ErrorCode { get { return errorCode; } set { errorCode = value; } }
        public ushort Reserved_0 { get { return reserved_0; } set { reserved_0 = value; } }
        public ushort Reserved_1 { get { return reserved_1; } set { reserved_1 = value; } }
    };
    public struct SPacket
    {
        private SHeader header;
        private byte[] data;
        public SPacket(byte[] data)
        {
            header = new SHeader();
            header.Tag = 0x7E;
            header.Mark = 0x4134;
            header.Vertify = 1;
            header.ErrorCode = 0;
            this.data = data;
        }
        public byte[] Data
        {
            get
            {
                if (data == null)
                {
                    data = new byte[0];
                }
                return data;
            }
            set => data = value;
        }
        public void Insert(byte[] b)
        {
            data = data.Concat(b).ToArray();
        }
        public byte[] GetBytes()
        {
            byte[] b = new byte[Marshal.SizeOf(header) + Data.Length];
            IntPtr buff = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, buff, true);
            Marshal.Copy(buff, b, 0, Marshal.SizeOf(header));
            Marshal.FreeHGlobal(buff);
            data.CopyTo(b, Marshal.SizeOf(header));
            return b;
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CHeader
    {
        private byte tag = 0x7E;
        private ushort mark= 0x4134;
        private byte vertify;
        private uint dataSize;
        private ushort protocol;
        private ushort errorCode;
        private ushort reserved_0;
        private ushort reserved_1;

        public byte Tag { get { return tag; } set { tag = value; } }
        public ushort Mark { get { return mark; } set { mark = value; } }
        public byte Vertify { get { return vertify; } set { vertify = value; } }
        public uint DataSize { get { return dataSize; } set { dataSize = Packet.HeaderSize + value; } }
        public ushort Protocol { get { return protocol; } set { protocol = value; } }
        public ushort ErrorCode { get { return errorCode; } set { errorCode = value; } }
        public ushort Reserved_0 { get { return reserved_0; } set { reserved_0 = value; } }
        public ushort Reserved_1 { get { return reserved_1; } set { reserved_1 = value; } }
    }

    public class Packet : IDisposable
    {
        public const UInt16 HeaderSize = 16;
        private SHeader header;
        private byte[] data;
        public Packet()
        {
            header.Tag = 0x7E;
            header.Mark = 0x4134;
            header.Vertify = 1;
            header.ErrorCode = 0;
        }
        public Packet(byte[] b)
        {
            header.Tag = 0x7E;
            header.Mark = 0x4134;
            header.Vertify = 1;
            header.ErrorCode = 0;

            IntPtr buffer = Marshal.AllocHGlobal(HeaderSize);
            Marshal.Copy(b, 0, buffer, HeaderSize);
            header = (SHeader)Marshal.PtrToStructure(buffer, typeof(SHeader));
            Marshal.FreeHGlobal(buffer);
        }
        public void SetHeader(SHeader s)
        {
            header = s;
        }
        public ref SHeader GetHeader()
        {
            return ref header;
        }

        public byte[] Data
        {
            get
            {
                if (data == null)
                {
                    data = new byte[0];
                }
                return data;
            }
            set => data = value;
        }
        public void Insert(byte[] b)
        {
            data = data.Concat(b).ToArray();
        }
        public byte[] GetBytes()
        {
            byte[] b = new byte[HeaderSize + Data.Length];

            IntPtr buff = Marshal.AllocHGlobal(HeaderSize);
            Marshal.StructureToPtr(header, buff, true);
            Marshal.Copy(buff, b, 0, HeaderSize);
            Marshal.FreeHGlobal(buff);
            data.CopyTo(b, HeaderSize);
            return b;
        }

        public void Dispose()
        {
            data = null;
        }
    }
}
