using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Socket.Data;

namespace Test
{
    public class TestServer : API.Socket.AsyncServerSocket
    {
        private int count = 0;
        public TestServer()
        {
            
        }
        public void Init()
        {
            BindCallback(2, Callback);
        }
        private void Callback(API.Socket.Data.Packet.Packet packet, StateObject stateObject)
        {
            Send(stateObject, 123, "1234");
            var str = Encoding.Default.GetString(packet.Data);
            Console.WriteLine("Recive : " + str);
        }
        public override void Start()
        {
            base.Start(10000);
        }

        protected override void AcceptComplete(StateObject state)
        {
            count++;
            Console.WriteLine("COUNT : " + count  + " : " + state.Handle);
        }

        protected override void DisconnectedComplete(ulong handerKey)
        {
            count--;
            Console.WriteLine("DIS COUNT : " + count);
        }
    }
}
