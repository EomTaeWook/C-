using System;
using System.Text;

namespace API.Socket.Callback.Client
{
    public abstract class ICallback<T> where T : class
    {
        protected ClientBase _clientSocket;
        protected abstract void InitCallback();

        public ICallback(ClientBase clientSocket)
        {
            this._clientSocket = clientSocket;
        }
        protected void BindCallback(int protocol, T funtion)
        {
            _clientSocket.BindCallback(protocol, funtion);
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
}
