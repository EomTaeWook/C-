using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Util.Collections
{
    public interface ICollection<T> : IDisposable
    {
        int Count { get; }
        ICollection<T> Push(T item);
        ICollection<T> Push(T[] items);
        T Pop();
        T Peek();
        void Clear();
    }
}
