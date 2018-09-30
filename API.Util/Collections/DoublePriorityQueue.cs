using System;
using System.Threading;

namespace API.Util.Collections
{
    public class DoublePriorityQueue<T> where T : IComparable<T>
    {
        private readonly ICollection<T>[] _queue;
        private byte _idx;
        private readonly object _sync;
        private readonly Order _order;
        public DoublePriorityQueue() : this(Order.Ascending)
        {
        }
        public DoublePriorityQueue(Order order)
        {
            _idx = 0;
            _sync = new object();
            _order = order;
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
        public ICollection<T> ReadQueue
        {
            get => _queue[_idx ^ 1];
        }
        public ICollection<T> AppendQueue
        {
            get => _queue[_idx];
        }
    }
}
