using API.Socket.InternalStructure;
using API.Util;
using API.Util.Logger;
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
            FileLogger.Instance().Init(LoggerPeriod.Hour);

            PriorityQueue<long> test = new PriorityQueue<long>(Order.Ascending);

            test.Append(1);
            test.Append(3);
            test.Append(7);
            test.Append(6);
            test.Append(13);
            test.Append(9);
            test.Append(15);
            test.Append(20);

            int cycle = 1;
            while (true)
            {
                Parallel.For(0, 10000, r =>
                {
                    //test.Append(r);
                    FileLogger.Instance().Write($"한글 테스트 : { r }");
                });

                //for (int r=0; r<50; r++)
                //{
                //    FileLogger.Instance().Write($"한글 테스트 : { r }");
                //}
                //foreach (var item in test)
                //{
                //    Console.Write(item + " ");
                //}
                //Console.WriteLine();
                //while (test.Count > 0)
                //{
                //    var t = test.Read();
                //    Console.WriteLine($"Read : {t}");
                //    Trace.WriteLine(t);
                //}
                Thread.Sleep(10);
                if (cycle > 2)
                    break;
                
            }
            

            while(test.Count> 0)
            {
                foreach (var item in test)
                {
                    Console.Write(item + " ");
                }
                var i = test.Read();
                Console.WriteLine($"Read : {i}");
                

            }


            //Trace.WriteLine("test1234", "test");
            //float balance = 5;
            //float amount = 4;
            ////Trace.Assert(amount >= balance);
            ////Trace.Assert(amount >= balance, "temp");
            ////_server = new TestServer();
            ////_server.Init();
            ////_server.Start();


            ////List<TestClient> list = new List<TestClient>();
            ////for (int i = 0; i < 5000; i++)
            ////{
            ////    list.Add(new TestClient());
            ////    list[i].Connect("192.169.0.2", 10000);
            ////    //list[i].Close();
            ////}
            ////List<TestClient> dislist = new List<TestClient>();
            ////for (int i = 0; i < 1000; i++)
            ////{
            ////    dislist.Add(new TestClient());
            ////    dislist[i].Connect("192.169.0.2", 10000);
            ////    //list[i].Close();
            ////}
            //_client = new TestClient();
            //_client.Connect("127.0.0.1", 10000);
            //_client.Send(new Packet(
            //            Encoding.Default.GetBytes($"<When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.>")));
            //ulong temp = 0;
            //_client.Connect("127.0.0.1", 10000);
            //_client.Send(new Packet(
            //            Encoding.Default.GetBytes($"<When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.>")));

            long count = 0;
            while (true)
            {
                FileLogger.Instance().Write($"한글 테스트 : { count++}");
                Thread.Sleep(10);
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
