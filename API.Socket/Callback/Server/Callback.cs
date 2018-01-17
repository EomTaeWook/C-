using API.Socket.Data;
using System;

namespace API.Socket.Callback.Server
{
    public abstract class Callback<T> where T : class
    {
        private ServerBase server;
        public Callback(ServerBase server) 
        {
            this.server = server;
            InitCallback();
        }

        protected abstract void InitCallback();

        protected void BindCallback(int protocol, T funtion)
        {
            server.BindCallback(protocol, funtion);
        }
        public void Send(StateObject handler, UInt16 protocol, string json)
        {
            if (json == null) return;
            byte[] b = System.Text.Encoding.Default.GetBytes(json);
            Send(handler, protocol, b);
        }
        public void Send(StateObject handler, UInt16 protocol, byte[] data)
        {
            if (data == null) return;
            server.Send(handler, protocol, data);
        }
        public virtual void BroadCast(Data.Packet.Packet packet, StateObject state)
        {
            server.BroadCast(packet, state);
        }
    }
}
