using API.Socket.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.Socket.Data
{
    public class DoubleBuffer<T>
    {
        BufferQueue<T>[] buffer;
        private short idx;
        private readonly object mutex;

        public DoubleBuffer()
        {
            mutex = new object();
            buffer = new BufferQueue<T>[2] { new BufferQueue<T>(), new BufferQueue<T>() };
        }

        public void Swap()
        {
            try
            {
                Monitor.Enter(mutex);
                if (ReadBuffer.Count() == 0) idx ^= 1;
            }
            finally
            {
                Monitor.Exit(mutex);
            }
        }
        public BufferQueue<T> ReadBuffer { get => buffer[idx ^ 1]; }
        public BufferQueue<T> WriteBuffer { get => buffer[idx]; }


    }
}
