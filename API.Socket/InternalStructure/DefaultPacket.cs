using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.InternalStructure
{
    public class DefaultPacket : IPacket
    {
        private byte[] _buff;
        private bool _disposed;
        public DefaultPacket()
        {
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            Dispose(true);
            _disposed = true;
        }
        protected virtual void Dispose(bool isDispose)
        {
        }
        public virtual byte[] GetBytes()
        {
            return _buff;
        }
        public virtual byte[] Data
        {
            get
            {
                if (_buff == null)
                    _buff = new byte[0];
                return _buff;
            }
            set => _buff = value;
        }
    }
}
