using System;
using System.Collections.Generic;
using System.Threading;

namespace API.Util
{
    public sealed class MemoryPool<T> : IDisposable where T : IDisposable, new()
    {
        private Queue<T> _pool;
        private readonly object _append, _read;
        private Func<T> _createCallback;
        private bool _disposed;
        public int CurrentCount { get => _pool.Count; }
        public int Count { get; private set; }

        public MemoryPool()
        {
            _append = new object();
            _read = new object();
            _pool = new Queue<T>();
        }
        public void Init(int count, Func<T> func)
        {
            Count = count;
            _createCallback = func;
            if (func == null)
                _createCallback = () => new T();
            else
                _createCallback = func;
            for (int i = 0; i < Count; i++)
                _pool.Enqueue(_createCallback());
        }
        public T Pop()
        {
            try
            {
                Monitor.Enter(_read);
                if (_pool.Count < 1)
                    return _createCallback();
                return _pool.Dequeue();
            }
            finally
            {
                Monitor.Exit(_read);
            }
        }
        public void Push(T data)
        {
            try
            {
                Monitor.Enter(_append);
                if (_pool.Count < Count)
                    _pool.Enqueue(data);
                else
                    data.Dispose();
            }
            finally
            {
                Monitor.Exit(_append);
            }
        }
        private void Dispose(bool isDispose)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                var data = _pool.Dequeue();
                data.Dispose();
            }
            _pool.Clear();
            _pool = null;
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
