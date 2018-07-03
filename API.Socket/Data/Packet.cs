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
        private byte[] _data;
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
            return _data;
        }
        public virtual byte[] Data
        {
            get
            {
                if (_data == null)
                    _data = new byte[0];
                return _data;
            }
            set => _data = value;
        }
    }
}
