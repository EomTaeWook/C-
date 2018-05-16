using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace API.Util
{
    public class SyncQueue<T> : IDisposable
    {
        private Queue<T> _queue;
        private readonly object _append, _read;
        public SyncQueue()
        {
            _append = new object();
            _read = new object();
            _queue = new Queue<T>();
            IsDispose = false;
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
                {
                    _queue.Enqueue(items[i]);
                }
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
                {
                    throw new IndexOutOfRangeException();
                }
                var b = _queue.Peek();
                Monitor.Exit(_read);
                return b;
            }
            catch (Exception ex)
            {
                Monitor.Exit(_read);
                throw new Exception(ex.Message, ex);
            }
        }
        public T[] Peek(int offset, int length)
        {
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count < offset + length)
                {
                    throw new IndexOutOfRangeException();
                }
                var b = _queue.Skip(offset).Take(length).ToArray();
                Monitor.Exit(_read);
                return b;
            }
            catch (Exception ex)
            {
                Monitor.Exit(_read);
                throw new Exception(ex.Message, ex);
            }
        }
        public T Read()
        {
            T item;
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                item = _queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(_read);
            }
            return item;
        }
        public T[] Read(uint size)
        {
            T[] items = null;
            try
            {
                Monitor.Enter(_read);
                if (_queue.Count < size)
                {
                    throw new IndexOutOfRangeException();
                }
                items = new T[size];
                for (int i = 0; i < size; i++)
                {
                    items[i] = _queue.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(_read);
            }
            return items;
        }
        public int Count()
        {
            return _queue.Count;
        }
        public void Clear()
        {
            try
            {
                Monitor.Enter(this);
                _queue.Clear();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        public void Dispose()
        {
            if (IsDispose) return;
            try
            {
                Monitor.Enter(this);
                _queue.Clear();
                _queue = null;
                IsDispose = true;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        public bool IsDispose { get; private set; }
    }
}
