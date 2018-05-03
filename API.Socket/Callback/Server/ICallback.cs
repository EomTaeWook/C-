using API.Socket.Data;
using API.Socket.Data.Packet;
using System;

namespace API.Socket.Callback.Server
{
    public abstract class ICallbackBase
    {
        protected ServerBase _server;
        protected abstract void InitCallback();
        public ICallbackBase(ServerBase server)
        {
            _server = server;
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
        public virtual void BroadCast(Packet packet, StateObject state)
        {
            _server.BroadCast(packet, state);
        }
    }

    public abstract class ICallback : ICallbackBase
    {
        public ICallback(ServerBase server) : base(server)
        {
        }
        protected void BindCallback(int protocol, Action<Packet, StateObject> funtion)
        {
            _server.BindCallback(protocol, funtion);
        }
    }

    public abstract class ICallback<T> : ICallbackBase
    {
        public ICallback(ServerBase server) : base(server)
        {
        }
        protected void BindCallback(int protocol, Action<Packet, StateObject, T> funtion)
        {
            _server.BindCallback(protocol, funtion);
        }
    }

    public abstract class ICallback<T1, T2> : ICallbackBase
    {
        public ICallback(ServerBase server) : base(server)
        {
        }
        protected void BindCallback(int protocol, Action<Packet, StateObject, T1, T2> funtion)
        {
            _server.BindCallback(protocol, funtion);
        }
    }

    public abstract class ICallback<T1, T2, T3> : ICallbackBase
    {
        public ICallback(ServerBase server) : base(server)
        {
        }
        protected void BindCallback(int protocol, Action<Packet, StateObject, T1, T2, T3> funtion)
        {
            _server.BindCallback(protocol, funtion);
        }
    }

    public abstract class ICallback<T1, T2, T3, T4> : ICallbackBase
    {
        public ICallback(ServerBase server) : base(server)
        {
        }
        protected void BindCallback(int protocol, Action<Packet, StateObject, T1, T2, T3, T4> funtion)
        {
            _server.BindCallback(protocol, funtion);
        }
    }
}
