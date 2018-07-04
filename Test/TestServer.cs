using API.Socket.Data;
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
            Send(stateObject, 123, "1234");
            var str = Encoding.Default.GetString(packet.Data);
            Console.WriteLine("Recive : " + str);
        }
        public void Send(StateObject stateObject, int protocol, string json)
        {

        }
        public void Start()
        {
            base.Start(10000);
        }

        protected override void Accepted(StateObject state)
        {
            count++;
            Console.WriteLine("COUNT : " + count  + " : " + state.Handle);
        }

        protected override void Disconnected(ulong handerKey)
        {
            count--;
            Console.WriteLine("DIS COUNT : " + count);
        }

        protected override void Recieved(StateObject state)
        {
            if (state.ReceiveBuffer.Count() >= 16)
            {
                var packet = new Packet(state.ReceiveBuffer.Peek(0, 16));
                if(packet.GetHeader().DataSize <= state.ReceiveBuffer.Count())
                {
                    packet.Data = state.ReceiveBuffer.Read(packet.GetHeader().DataSize).Skip(16).ToArray();
                    if (state.ReceivePacketBuffer.Count() <= 0)
                    {
                        state.ReceivePacketBuffer.Append(packet);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(Work), state);
                    }
                    else
                    {
                        state.ReceivePacketBuffer.Append(packet);
                    }
                }
            }
        }

        private void Work(object state)
        {
            
        }
    }
}
