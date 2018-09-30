using System;
using System.Collections;
using System.Collections.Generic;

namespace API.Util.Collections
{
    public class PriorityQueue<T> : IEnumerable<T>, ICollection<T> where T : IComparable<T>
    {
        private List<T> _list;
        private readonly Order _order;
        private bool _disposed;
        public int Count => _list.Count;
        public PriorityQueue() : this(Order.Ascending)
        {
        }
        public PriorityQueue(Order order)
        {
            _list = new List<T>();
            _order = order;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public void Clear()
        {
            _list.Clear();
        }
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }
        public T Pop()
        {
            if (_list.Count == 0)
                throw new IndexOutOfRangeException();
            T item = _list[0];
            _list[0] = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);
            int index = 0, childIndex = 0;
            while (true)
            {
                //Left Node Right Node
                if (_list.Count > index * 2 + 1 && _list.Count > index * 2 + 2)
                {
                    var compare = _list[index * 2 + 1].CompareTo(_list[index * 2 + 2]);
                    if (compare > 0 && _order == Order.Ascending)
                        childIndex = index * 2 + 1;
                    else if (compare < 0 && _order == Order.Ascending)
                        childIndex = index * 2 + 2;
                    else if (compare < 0 && _order == Order.Descending)
                        childIndex = index * 2 + 1;
                    else if (compare > 0 && _order == Order.Descending)
                        childIndex = index * 2 + 2;
                    else if(compare == 0)
                        childIndex = index * 2 + 1;

                    if (_list[index].CompareTo(_list[childIndex]) < 0 && _order == Order.Ascending)
                    {
                        Swap(index, childIndex);
                        index = childIndex;
                    }
                    else if (_list[index].CompareTo(_list[childIndex]) > 0 && _order == Order.Descending)
                    {
                        Swap(index, childIndex);
                        index = childIndex;
                    }
                    else
                        break;
                }
                //Left Node
                else if (_list.Count > index * 2 + 1)
                {
                    childIndex = index * 2 + 1;
                    if (_list[index].CompareTo(_list[childIndex]) < 0 && _order == Order.Ascending)
                    {
                        Swap(index, childIndex);
                        index = childIndex;
                    }
                    else if (_list[index].CompareTo(_list[childIndex]) > 0 && _order == Order.Descending)
                    {
                        Swap(index, childIndex);
                        index = childIndex;
                    }
                    else
                        break;
                }
                else
                    break;
            }
            return item;
        }
        public ICollection<T> Push(T item)
        {
            _list.Add(item);
            int index = _list.Count - 1;
            int parent = 0;
            while (true)
            {
                if (index <= 0)
                    break;
                parent = (index - 1) / 2;
                var compare = _list[index].CompareTo(_list[parent]);
                if (_order == Order.Ascending && compare > 0)
                {
                    Swap(index, parent);
                    index = parent;
                }
                else if (_order == Order.Descending && compare < 0)
                {
                    Swap(index, parent);
                    index = parent;
                }
                else
                    break;
            }
            return this;
        }
        public ICollection<T> Push(T[] items)
        {
            foreach(var item in items)
            {
                Push(item);
            }
            return this;
        }
        public T Peek()
        {
            if (_list.Count == 0)
                throw new IndexOutOfRangeException();
            return _list[0];
        }
        public T this[int index]
        {
            get=> _list[index];
        }
        public T[] ToArray()
        {
            return _list.ToArray();
        }
        private void Swap(int child, int parent)
        {
            var temp = _list[child];
            _list[child] = _list[parent];
            _list[parent] = temp;
        }
        private void Dispose(bool disposed)
        {
            _list.Clear();
            _list = null;
            _disposed = disposed;
        }
        public void Dispose()
        {
            if(!_disposed)
                Dispose(true);
        }
    }
}
