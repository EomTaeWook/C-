using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
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
    }

    public class Packet : API.Socket.Data.Packet
    {
        public const UInt16 HeaderSize = 16;
        private SHeader header;
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
        public override byte[] GetBytes()
        {
            byte[] b = new byte[HeaderSize + Data.Length];

            IntPtr buff = Marshal.AllocHGlobal(HeaderSize);
            Marshal.StructureToPtr(header, buff, true);
            Marshal.Copy(buff, b, 0, HeaderSize);
            Marshal.FreeHGlobal(buff);
            Data.CopyTo(b, HeaderSize);
            return b;
        }
    }
}


