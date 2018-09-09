using System;
using System.Collections.Concurrent;
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
        static TestClient _client;
        static void Main(string[] args)
        {
            //_server = new TestServer();
            //_server.Init();

            //_server.Start();

            List<TestClient> list = new List<TestClient>();
            for(int i=0; i<10000; i++)
            {
                list.Add(new TestClient());
                list[i].Connect("127.0.0.1", 10000);
                //list[i].Close();
            }
            //_client = new TestClient();
            //_client.Connect("127.0.0.1", 10000);
            list[0].Send(new Packet(
                        Encoding.Default.GetBytes($"When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.")));
            ulong temp = 0;
            while (true)
            {
                Parallel.ForEach(list,
                    r => r.Send(new Packet(
                        Encoding.Default.GetBytes($"[ {temp++} ] When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.")))
                    );
                //_client.Send(new Packet(Encoding.Default.GetBytes($"When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.")));
                Thread.Sleep(5000);
                GC.Collect();
            }
        }
    }
}
