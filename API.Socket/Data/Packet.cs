using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.Data
{
    public class Packet : IDisposable
    {
        private byte[] _buff;
        private bool _isDispose;
        public Packet()
        {
        }
        public void Dispose()
        {
            Dispose(true);
        }
        private void Dispose(bool isDispose)
        {
            _isDispose = isDispose;
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
