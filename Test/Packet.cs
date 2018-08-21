using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Packet : API.Socket.IPacket
    {
        private byte[] _datas;
        public Packet(byte[] b)
        {
            _datas = b;
        }
        public byte[] GetBytes()
        {
            return _datas;
        }
        public void Dispose()
        {
            _datas = null;
        }
    }
}


