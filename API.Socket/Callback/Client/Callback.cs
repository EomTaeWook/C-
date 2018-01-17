using System;
using System.Text;

namespace API.Socket.Callback.Client
{
    public abstract class Callback<T> where T : class
    {
        private ClientBase clientSocket;
        
        public Callback(ClientBase clientSocket)
        {
            this.clientSocket = clientSocket;
            InitCallback();
        }

        protected abstract void InitCallback();
        protected void BindCallback(int protocol, T funtion)
        {
            clientSocket.BindCallback(protocol, funtion);
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
            clientSocket.Send(protocol, data);
        }
    }
}
