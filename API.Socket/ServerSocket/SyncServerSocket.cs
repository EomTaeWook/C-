using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using API.Socket.Data.Packet;
using System.Diagnostics;

namespace API.Socket
{
    public class SyncServerSocket : ServerSocket
    {
        
        public override void Close()
        {
            
        }

        protected override void Receive()
        {
            
        }

        protected override void Send(System.Net.Sockets.Socket handler, Packet packet)
        {
            
        }

        protected override void StartListening()
        {
            System.Net.Sockets.Socket listener = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(iPEndPoint);
                listener.Listen(100);

                while (isRunning)
                {
                    System.Net.Sockets.Socket handler = listener.Accept();
                    var client = new API.Socket.Data.StateObject();
                    client.WorkSocket = handler;
                    client_list.Add(client);
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
