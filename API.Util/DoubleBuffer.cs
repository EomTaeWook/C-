using System.Threading;

namespace API.Util
{
    public class DoubleBuffer<T>
    {
        BufferQueue<T>[] _buffer;
        private short _idx;
        private readonly object _mutex;

        public DoubleBuffer()
        {
            _mutex = new object();
            _buffer = new BufferQueue<T>[2] { new BufferQueue<T>(), new BufferQueue<T>() };
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
        public BufferQueue<T> ReadBuffer { get => _buffer[_idx ^ 1]; }
        public BufferQueue<T> WriteBuffer { get => _buffer[_idx]; }


    }
}
