using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.Util.Common
{
    public class Singleton<T> where T : class, new()
    {
        private readonly static object _mutex = new object();
        private static T _instace;
        public static T Instance()
        {
            try
            {
                Monitor.Enter(_mutex);
                if (_instace == null)
                {
                    _instace = new T();
                }
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
            return _instace;
        }
    }
}
