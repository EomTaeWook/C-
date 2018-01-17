using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.Socket.Data
{
    public class MutexCount
    {
        private ulong count = 0;
        private readonly object mutex;
        public MutexCount()
        {
            mutex = new object();
        }
        public ulong CountAdd()
        {
            try
            {
                Monitor.Enter(mutex);
                if (count + 1 == 0)
                {
                    count = 1;
                }
                else
                {
                    count++;
                }
            }
            finally
            {
                Monitor.Exit(mutex);
            }
            return count;
        }
        public ulong ReadCount()
        {
            return count;
        }
    }
}
