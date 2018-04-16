using System;
using System.Text;

namespace API.Socket.Callback.Client
{
    public abstract class ICallbackBase
    {
        protected ClientBase _clientSocket;
        protected abstract void InitCallback();
        public ICallbackBase(ClientBase clientSocket)
        {
            _clientSocket = clientSocket;
        }
        public void Send(UInt16 protocol, string json)
        {
            if (json == null) return;
            byte[] b = Encoding.Default.GetBytes(json);
            Send(protocol, b);
        }
        public void Send(UInt16 protocol, byte[] data)
        {
            if (data == null) return;
            _clientSocket.Send(protocol, data);
        }
    }

    public abstract class ICallback<T> : ICallbackBase
    {
        public ICallback(ClientBase clientSocket) : base(clientSocket)
        {
        }

        protected void BindCallback(int protocol, Action<T> funtion)
        {
            _clientSocket.BindCallback(protocol, funtion);
        }
        
    }
    public abstract class ICallback<T1, T2> : ICallbackBase
    {
        public ICallback(ClientBase clientSocket) : base(clientSocket)
        {
        }

        protected void BindCallback(int protocol, Action<T1,T2> funtion)
        {
            _clientSocket.BindCallback(protocol, funtion);
        }
    }
}
