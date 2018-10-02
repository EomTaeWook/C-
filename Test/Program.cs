using API.Socket.InternalStructure;
using API.Util;
using API.Util.Collections;
using API.Util.Logger;
using log4net.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Test
{
    class Program
    {
        static TestServer _server;
        static TestClient _client;
        static void Main(string[] args)
        {
            //List<int> List = new List<int>();

            //DoublePriorityQueue<int> doubleBuffer = new DoublePriorityQueue<int>();
            //doubleBuffer.AppendQueue.Push(10);
            //long count = 0;
            //PriorityQueue<long> queue = new PriorityQueue<long>(Order.Descending);
            //var start = DateTimeOffset.Now;

            //queue.Push(10);
            //queue.Push(9);
            //queue.Push(11);
            //queue.Push(9);
            //queue.Push(11);
            //queue.Push(9);
            //queue.Push(9);
            //queue.Push(11);

            //while(queue.Count > 0)
            //{
            //    var read = queue.Pop();
            //    foreach(var item in queue)
            //    {
            //        Console.Write(item + " ");
            //    }
            //    Console.WriteLine($"read {read}");
            //}
            FileLogger.Instance().Init(LoggerPeriod.Hour);
            XmlConfigurator.Configure(new FileInfo("App.config"));
            var _logger = log4net.LogManager.GetLogger("Program");
            int count = 0;
            var start = DateTimeOffset.Now;

            while (count++ < 5)
            {
                //Parallel.For(0, 5000, r =>
                //{
                //    queue.Append(r);
                //});
                //Random r = new Random((int)(DateTime.Now.Ticks & 0x0000FFFF));

                //for (int i=0; i<1000; i++)
                //{
                //    queue.Append(r.Next(0, 50));
                //}
                //while(queue.Count > 0)
                //{
                //    Trace.Write($"read {queue.Read()}\n");
                //}
                //Trace.Write("============================\n");
                Parallel.For(0, 50000, r =>
                {
                    var time = DateTimeOffset.Now;
                    _logger.Debug($"한글 테스트 : <{time.ToString("HH:mm:ss.fff")}>");
                    //FileLogger.Instance().Write(new LogMessage() { Message = $"한글 테스트 : <{time.ToString("HH:mm:ss.fff")}>", TimeStamp = time });
                });
                //for (int i = 0; i < 100000; i++)
                //{
                //    //_logger.Debug($"한글 테스트 : <{time.ToString("HH:mm:ss.fff")}>");
                //    var time = DateTimeOffset.Now;
                //    FileLogger.Instance().Write(new LogMessage() { Message = $"한글 테스트 : <{time.ToString("HH:mm:ss.fff")}>", TimeStamp = time });
                //}
                //FileLogger.Instance().Write($"한글 테스트 : { count++}");
                //Thread.Sleep(10);
            }
            FileLogger.Instance().Close();
            var span = DateTimeOffset.Now - start;
            Console.WriteLine(span);
            Console.ReadKey();
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


            //while (true)
            //{
            //    FileLogger.Instance().Write($"한글 테스트 : { count++}");
            //    Thread.Sleep(10);
            //    //Parallel.ForEach(list,
            //    //    r => r.Send(new Packet(
            //    //        Encoding.Default.GetBytes($"<[ {temp++} ] When one thinks of the labors which the the English have devoted to digging the tunnel under the Thames, the tremendous expenditure of energy involved, and then how a little accident may for a long time obstruct the entire enterprise, one will be able to form a fitting conception of this critical undertaking as a whole.>")))
            //    //    );
            //    //Parallel.ForEach(dislist,
            //    //    r => r.DisConnect()
            //    //    );
            //    //Thread.Sleep(5000);
            //    //GC.Collect();
            //    //Parallel.ForEach(dislist,
            //    //    r => r.Connect("192.169.0.2", 10000)
            //    //    );
            //}
        }
    }
}
