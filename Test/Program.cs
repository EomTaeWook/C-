using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static TestServer _server;
        static void Main(string[] args)
        {
            _server = new TestServer();
            _server.Init();
            _server.Start();
            while(true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
