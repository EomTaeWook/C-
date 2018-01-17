using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.Socket.Data
{
    public class MemoryPool<T> : IDisposable where T : IDisposable, new()
    {
        protected int count;
        protected Queue<T> pool;
        protected readonly object append, read;
        private Func<T> createT;
        public MemoryPool(int count = 100, bool autoCreate = true, Func<T> func = null)
        {
            append = new object();
            read = new object();
            pool = new Queue<T>(count);
            this.count = count;
            if (func == null) createT = () => new T();
            else createT = func;
            if (autoCreate)
            {
                for (int i = 0; i < this.count; i++)
                {
                    pool.Enqueue(createT());
                }
            }
        }
        public void Init(int count, Func<T> func)
        {
            this.count = count;
            this.createT = func;
            if (func == null) createT = () => new T();
            else createT = func;
            for (int i = 0; i < this.count; i++)
            {
                pool.Enqueue(createT());
            }
        }
        public T Pop()
        {
            try
            {
                Monitor.Enter(read);
                if (pool.Count < 1)
                {
                    return createT();
                }
                return pool.Dequeue();
            }
            finally
            {
                Monitor.Exit(read);
            }
        }
        public void Push(T data)
        {
            try
            {
                Monitor.Enter(append);
                if (pool.Count < count)
                {
                    pool.Enqueue(data);
                }
                else
                {
                    data.Dispose();
                    data = default(T);
                }
            }
            finally
            {
                Monitor.Exit(append);
            }
        }
        public void Dispose()
        {
            try
            {
                Monitor.Enter(this);
                for(int i=0; i<pool.Count; i++)
                {
                    var data = pool.Dequeue();
                    data.Dispose();
                    data = default(T);
                }
                pool.Clear();
                pool = null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        public int CurrentCount { get => pool.Count; }
        public int Count { get => count; }
    }
}
