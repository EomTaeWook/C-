using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace API.Socket.Data
{
    public class BufferQueue<T> : IDisposable
    {
        private Queue<T> queue;
        private readonly object append, read;
        public BufferQueue()
        {
            append = new object();
            read = new object();
            queue = new Queue<T>();
        }
        public void Append(T data)
        {
            try
            {
                Monitor.Enter(append);
                queue.Enqueue(data);
            }
            finally
            {
                Monitor.Exit(append);
            }
        }
        public void Append(T[] data)
        {
            try
            {
                Monitor.Enter(append);
                for (int i = 0; i < data.Length; i++)
                {
                    queue.Enqueue(data[i]);
                }
            }
            finally
            {
                Monitor.Exit(append);
            }
        }
        public T Peek()
        {
            try
            {
                Monitor.Enter(read);
                if (queue.Count < 1)
                {
                    throw new IndexOutOfRangeException();
                }
                var b = queue.Peek();
                Monitor.Exit(read);
                return b;
            }
            catch (System.Exception ex)
            {
                Monitor.Exit(read);
                throw new System.Exception(ex.Message, ex);
            }
        }
        public T[] Peek(int offset, int length)
        {
            try
            {
                Monitor.Enter(read);
                if (queue.Count < offset + length)
                {
                    throw new IndexOutOfRangeException();
                }
                var b = queue.Skip(offset).Take(length).ToArray();
                Monitor.Exit(read);
                return b;
            }
            catch (System.Exception ex)
            {
                Monitor.Exit(read);
                throw new System.Exception(ex.Message, ex);
            }
        }
        public T Read()
        {
            T data;
            try
            {
                Monitor.Enter(read);
                if (queue.Count == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                data = queue.Dequeue();
            }
            finally
            {
                Monitor.Exit(read);
            }
            return data;
        }
        public T[] Read(uint size)
        {
            T[] data = null;
            try
            {
                Monitor.Enter(read);
                if (queue.Count < size)
                {
                    throw new IndexOutOfRangeException();
                }
                data = new T[size];
                for (int i = 0; i < size; i++)
                {
                    data[i] = queue.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(read);
            }
            return data;
        }
        public int Count()
        {
            return queue.Count;
        }
        public void Clear()
        {
            try
            {
                Monitor.Enter(this);
                queue.Clear();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        public void Dispose()
        {
            try
            {
                Monitor.Enter(this);
                queue.Clear();
                queue = null;
                IsDisposable = true;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        public bool IsDisposable { get; private set; }
    }
}
