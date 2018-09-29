using System.Threading;

namespace API.Util.Collections
{
    public class DoubleBuffer<T>
    {
        SyncQueue<T>[] _buffer;
        private short _idx;
        private readonly object _mutex;

        public DoubleBuffer()
        {
            _mutex = new object();
            _buffer = new SyncQueue<T>[2] { new SyncQueue<T>(), new SyncQueue<T>() };
        }

        public void Swap()
        {
            try
            {
                Monitor.Enter(_mutex);
                if (ReadBuffer.Count() == 0) _idx ^= 1;
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
        }
        public SyncQueue<T> ReadBuffer { get => _buffer[_idx ^ 1]; }
        public SyncQueue<T> WriteBuffer { get => _buffer[_idx]; }
    }
}
