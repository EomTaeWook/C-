using System;
using System.Collections;
using System.Collections.Generic;

namespace API.Util
{
    public enum Order
    {
        Ascending,
        Descending
    }
    public class PriorityQueue<T> : IEnumerable<T> where T : IComparable<T>
    {
        private List<T> _list = new List<T>();
        private Order _order;
        public PriorityQueue(Order order = Order.Ascending)
        {
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
        public int Count
        {
            get
            {
                return _list.Count;
            }
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
        public T Dequeue()
        {
            if (_list.Count == 0)
            {
                throw new InvalidOperationException();
            }
            T item = _list[0];
            _list[0] = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);
            int index = 0;
            int size = _list.Count - 1;
            while (true)
            {
                //Left Node Right Node
                if (size > index * 2 + 1 && size > index * 2)
                {
                    var child = _list[index * 2 + 1].CompareTo(_list[index * 2]);
                    int childIndex = 0;
                    if (child > 0 && _order == Order.Ascending)
                    {
                        childIndex = index * 2 + 1;
                    }
                    else if (child < 0 && _order == Order.Ascending)
                    {
                        childIndex = index * 2;
                    }
                    else if (child < 0 && _order == Order.Descending)
                    {
                        childIndex = index * 2 + 1;
                    }
                    else if (child > 0 && _order == Order.Descending)
                    {
                        childIndex = index * 2;
                    }
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
                    var order = _list[index].CompareTo(_list[index * 2 + 1]);
                    if (order < 0 && _order == Order.Ascending)
                    {
                        Swap(index, index * 2 + 1);
                        index = index * 2 + 1;
                    }
                    else if (order > 0 && _order == Order.Descending)
                    {
                        Swap(index, index * 2 + 1);
                        index = index * 2 + 1;
                    }
                    else
                        break;
                }
                else
                    break;
                
            }

            return item;
        }
        public void Enqueue(T item)
        {
            _list.Add(item);
            int index = _list.Count - 1;
            int parent = 0;
            while (true)
            {
                if (index == 0) break;
                parent = (index - 1) / 2;
                var order = _list[index].CompareTo(_list[parent]);
                if (_order == Order.Ascending && order > 0)
                {
                    Swap(index, parent);
                    index = parent;
                }
                else if (_order == Order.Descending && order < 0)
                {
                    Swap(index, parent);
                    index = parent;
                }
                else
                    break;
            }
        }
        public T Peek()
        {
            if (_list.Count == 0)
            {
                throw new InvalidOperationException();
            }
            return _list[0];
        }
        public T[] ToArray()
        {
            return _list.ToArray();
        }
        private void Swap(int index, int parent)
        {
            var temp = _list[index];
            _list[index] = _list[parent];
            _list[parent] = temp;
        }
    }
}
