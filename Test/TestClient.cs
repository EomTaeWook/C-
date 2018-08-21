using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Socket.InternalStructure;

namespace Test
{
    public class TestClient : API.Socket.ClientSocket.ClientSocket<int>
    {
        protected override void OnConnected(StateObject state)
        {
            
        }

        protected override void OnDisconnected()
        {
            throw new NotImplementedException();
        }

        protected override void OnRecieved(StateObject state)
        {
            uint count = (uint)state.ReceiveBuffer.Count();
            var receive = state.ReceiveBuffer.Read(count);
            Console.WriteLine($"[ {DateTime.Now} ] Recieved : " + System.Text.Encoding.UTF8.GetString(receive));
            //state.Send(new Packet(receive));
        }
    }
}
