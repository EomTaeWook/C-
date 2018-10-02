using System;
using System.Threading;

namespace API.Util.Collections
{
    public class DoublePriorityQueue<T> : IDisposable where T : IComparable<T> 
    {
        private readonly ICollection<T>[] _queue;
        private byte _idx;
        private readonly Order _order;
        private readonly object _sync;
        private bool _disposed;
        public DoublePriorityQueue() : this(Order.Ascending)
        {
        }
        public DoublePriorityQueue(Order order)
        {
            _disposed = false;
            _idx = 0;
            _order = order;
            _sync = new object();
            _queue = new ICollection<T>[] { new PriorityQueue<T>(order), new PriorityQueue<T>(order) };
        }
        public void Swap()
        {
            try
            {
                Monitor.Enter(_sync);
                if (ReadQueue.Count == 0)
                    _idx ^= 1;
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }
        public ICollection<T> Push(T item)
        {
            try
            {
                Monitor.Enter(_sync);
                AppendQueue.Push(item);
            }
            finally
            {
                Monitor.Exit(_sync);
            }
            return AppendQueue;
        }
        public T Peek()
        {
            return ReadQueue.Peek();
        }

        public ICollection<T> Push(T[] items)
        {
            try
            {
                Monitor.Enter(_sync);
                foreach(var item in items)
                    AppendQueue.Push(item);
            }
            finally
            {
                Monitor.Exit(_sync);
            }
            return AppendQueue;
        }

        public T Pop()
        {
            return ReadQueue.Pop();
        }
        private void Dispose(bool dispose)
        {
            ReadQueue.Clear();
            AppendQueue.Clear();
            ReadQueue.Dispose();
            AppendQueue.Dispose();
            _disposed = true;
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            Dispose(true);
        }

        public int AppendCount => AppendQueue.Count;
        public int ReadCount => ReadQueue.Count;

        private ICollection<T> ReadQueue
        {
            get => _queue[_idx ^ 1];
        }
        private ICollection<T> AppendQueue
        {
            get => _queue[_idx];
        }
    }
}
