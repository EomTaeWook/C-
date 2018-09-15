using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket
{
    public interface IPacket : IDisposable
    {
        byte[] GetBytes();
    }
}
