using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace API.Util.Collections
{
    public class SyncQueue<T> : IDisposable
    {
        private Queue<T> _queue;
        private bool _disposed;
        private readonly object _append, _read;
        public SyncQueue()
        {
            _append = new object();
            _read = new object();
            _queue = new Queue<T>();
        }
        public SyncQueue<T> Append(T item)
        {
            try
            {
                Monitor.Enter(_append);
                _queue.Enqueue(item);
            }
            finally
            {
                Monitor.Exit(_append);
            }
            return this;
        }
        public SyncQueue<T> Append(T[] items)
        {
            try
            {
                Monitor.Enter(_append);
                for (int i = 0; i < items.Length; i++)
                    _queue.Enqueue(items[i]);
            }
            finally
            {
                Monitor.Exit(_append);
            }
            return this;
        }
        public T Peek()
        {
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count < 1)
                    throw new IndexOutOfRangeException();
                return _queue.Peek();
            }
            finally
            {
                Monitor.Exit(_read);
            }
        }
        public T[] Peek(int offset, int length)
        {
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count < offset + length)
                    throw new IndexOutOfRangeException();
                return _queue.Skip(offset).Take(length).ToArray();
            }
            finally
            {
                Monitor.Exit(_read);
            }
        }
        public T Read()
        {
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count == 0)
                    throw new IndexOutOfRangeException();
                return _queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(_read);
            }
        }
        public T[] Read(uint size)
        {
            T[] items = null;
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count < size)
                    throw new IndexOutOfRangeException();
                items = new T[size];
                for (int i = 0; i < size; i++)
                    items[i] = _queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(_read);
            }
            return items;
        }
        public bool Contain(T item)
        {
            return _queue.Contains(item);
        }
        public int Count()
        {
            return _queue.Count;
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
