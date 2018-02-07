using API.Socket.Data;
using System;

namespace API.Socket.Callback.Server
{
    public abstract class ICallback<T> where T : class
    {
        protected ServerBase _server;
        protected abstract void InitCallback();

        public ICallback(ServerBase server)
        {
            _server = server;
        }
        protected void BindCallback(int protocol, T funtion)
        {
            _server.BindCallback(protocol, funtion);
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
            _server.Send(handler, protocol, data);
        }
        public virtual void BroadCast(Data.Packet.Packet packet, StateObject state)
        {
            _server.BroadCast(packet, state);
        }
    }
}
