using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace API.Util.Collections
{
    public class SyncQueue<T> : ICollection<T>
    {
        private Queue<T> _queue;
        private bool _disposed;
        private readonly object _push, _pop;
        public int Count => _queue.Count;

        public SyncQueue()
        {
            _push = new object();
            _pop = new object();
            _queue = new Queue<T>();
        }
        public ICollection<T> Push(T item)
        {
            try
            {
                Monitor.Enter(_push);
                _queue.Enqueue(item);
            }
            finally
            {
                Monitor.Exit(_push);
            }
            return this;
        }
        public ICollection<T> Push(T[] items)
        {
            try
            {
                Monitor.Enter(_push);
                for (int i = 0; i < items.Length; i++)
                    _queue.Enqueue(items[i]);
            }
            finally
            {
                Monitor.Exit(_push);
            }
            return this;
        }
        public T Peek()
        {
            try
            {
                Monitor.Enter(_pop);
                if (_queue.Count < 1)
                    throw new IndexOutOfRangeException();
                return _queue.Peek();
            }
            finally
            {
                Monitor.Exit(_pop);
            }
        }
        public T[] Peek(int offset, int length)
        {
            try
            {
                Monitor.Enter(_pop);
                if (_queue.Count < offset + length)
                    throw new IndexOutOfRangeException();
                return _queue.Skip(offset).Take(length).ToArray();
            }
            finally
            {
                Monitor.Exit(_pop);
            }
        }
        public T Pop()
        {
            try
            {
                Monitor.Enter(_pop);
                if (_queue.Count == 0)
                    throw new IndexOutOfRangeException();
                return _queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(_pop);
            }
        }
        public T[] Read(uint size)
        {
            T[] items = null;
            try
            {
                Monitor.Enter(_pop);
                if (_queue.Count < size)
                    throw new IndexOutOfRangeException();
                items = new T[size];
                for (int i = 0; i < size; i++)
                    items[i] = _queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(_pop);
            }
            return items;
        }
        public bool Contain(T item)
        {
            return _queue.Contains(item);
        }
        public void Clear()
        {
            if(Monitor.TryEnter(this))
            {
                try
                {
                    _queue.Clear();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        private void Dispose(bool isDispose)
        {
            Clear();
            _queue = null;
            _disposed = isDispose;
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            Dispose(true);
        }
    }
}
