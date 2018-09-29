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
        ICollection<T> Append(T item);
        ICollection<T> Append(T[] items);
        T Read();
        T Peek();
    }
}
