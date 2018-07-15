using System.Threading;

namespace API.Socket.InternalStructure
{
    public class SyncCount
    {
        private ulong _count = 0;
        private readonly object _sync;
        public SyncCount()
        {
            _sync = new object();
        }
        public ulong CountAdd()
        {
            try
            {
                Monitor.Enter(_sync);
                if (_count + 1 == 0)
                    _count = 1;
                else
                    _count++;
            }
            finally
            {
                Monitor.Exit(_sync);
            }
            return _count;
        }
        public ulong ReadCount()
        {
            return _count;
        }
    }
}
