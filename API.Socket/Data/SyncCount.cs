using System.Threading;

namespace API.Socket.Data
{
    public class SyncCount
    {
        private ulong _count = 0;
        private readonly object _mutex;
        public SyncCount()
        {
            _mutex = new object();
        }
        public ulong CountAdd()
        {
            try
            {
                Monitor.Enter(_mutex);
                if (_count + 1 == 0)
                {
                    _count = 1;
                }
                else
                {
                    _count++;
                }
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
            return _count;
        }
        public ulong ReadCount()
        {
            return _count;
        }
    }
}
