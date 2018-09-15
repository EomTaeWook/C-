using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using API.Socket.InternalStructure;

namespace Test
{
    public class TestClient : API.Socket.ClientSocket.ClientSocket<int>
    {
        public void DisConnect()
        {
            base.ClosePeer();
        }
        protected override void OnConnected(StateObject state)
        {
            
        }

        protected override void OnDisconnected()
        {
            
            //while(!IsConnect())
            //{
            //    try
            //    {
            //        Connect("127.0.0.1", 10000);
            //    }
            //    finally
            //    {
            //        Thread.Sleep(4000);
            //    }
            //}
        }

        protected override void OnRecieved(StateObject state)
        {
            uint count = (uint)state.ReceiveBuffer.Count();
            var receive = state.ReceiveBuffer.Read(count);
            //state.ReceiveBuffer.Clear();
            //Console.WriteLine($"[ {DateTime.Now} ] Recieved : " + count.ToString());//System.Text.Encoding.UTF8.GetString(receive));
            //state.Send(new Packet(receive));
        }
    }
}
