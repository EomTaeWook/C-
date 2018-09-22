using API.Socket.InternalStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestServer : API.Socket.ServerSocket.ServerSocket<int>
    {
        private int count = 0;
        public TestServer()
        {
            
        }
        public void Init()
        {
            BindCallback<Packet>(2, Callback);
        }
        private void Callback(StateObject stateObject, Packet packet)
        {
            //Send(stateObject, 123, "1234");
            //var str = Encoding.Default.GetString(packet.Data);
            //Console.WriteLine("Recive : " + str);
        }
        public void Send(StateObject stateObject, int protocol, string json)
        {

        }
        public void Start()
        {
            base.Start("192.168.0.3",10000);
        }

        protected override void OnAccepted(StateObject state)
        {
            //count++;
            //var packet = new Packet();
            //packet.GetHeader().Protocol = 1234;

            //packet.Data = Encoding.Default.GetBytes("test");
            //packet.GetHeader().DataSize = 4;

            //state.Send(packet);
            //Console.WriteLine("COUNT : " + count  + " : " + state.Handle);
        }

        protected override void OnDisconnected(ulong handerKey)
        {
            count--;
            Console.WriteLine("DIS COUNT : " + count);
        }

        protected override void OnRecieved(StateObject state)
        {
            uint count = (uint)state.ReceiveBuffer.Count();
            var receive = state.ReceiveBuffer.Read(count);
            Console.WriteLine($"[ {DateTime.Now} ] Recieved : " + System.Text.Encoding.UTF8.GetString(receive));
            state.Send(new Packet(receive));
        }

        private void Work(object state)
        {
            
        }
    }
}
