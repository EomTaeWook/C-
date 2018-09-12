using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.InternalStructure
{
    public class TcpKeepAlive
    {
        public uint OnOff { get; set; }
        public uint KeepAliveTime { get; set; }
        public uint KeepAliveInterval { get; set; }
        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(OnOff).Concat(BitConverter.GetBytes(KeepAliveTime)).Concat(BitConverter.GetBytes(KeepAliveInterval)).ToArray();
        }
    }
}
