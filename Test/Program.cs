using API.Socket.InternalStructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static TestServer _server;
        static TestClient _client;
        static void Main(string[] args)
        {
            Trace.WriteLine("test1234", "test");
            float balance = 5;
            float amount = 4;
            //Trace.Assert(amount >= balance);
            //Trace.Assert(amount >= balance, "temp");
            //_server = new TestServer();
            //_server.Init();
            //_server.Start();


            //List<TestClient> list = new List<TestClient>();
            //for (int i = 0; i < 5000; i++)
            //{
            //    list.Add(new TestClient());
            //    list[i].Connect("192.169.0.2", 10000);
            //    //list[i].Close();
            //}
            //List<TestClient> dislist = new List<TestClient>();
            //for (int i = 0; i < 1000; i++)
            //{
            //    dislist.Add(new TestClient());
            //    dislist[i].Connect("192.169.0.2", 10000);
            //    //list[i].Close();
            //}
            _client = new TestClient();
            //_client.Connect("127.0.0.1", 10000);
            //list[0].Send(new Packet(
            //            Encoding.Default.GetBytes($"<When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.>")));
            ulong temp = 0;
            _client.Connect("127.0.0.1", 10000);
            _client.Send(new Packet(
                        Encoding.Default.GetBytes($"<When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.>")));
            while (true)
            {
                //Parallel.ForEach(list,
                //    r => r.Send(new Packet(
                //        Encoding.Default.GetBytes($"<[ {temp++} ] When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.>")))
                //    );
                //Parallel.ForEach(dislist,
                //    r => r.DisConnect()
                //    );
                //Thread.Sleep(5000);
                //GC.Collect();
                //Parallel.ForEach(dislist,
                //    r => r.Connect("192.169.0.2", 10000)
                //    );
            }
        }
    }
}
