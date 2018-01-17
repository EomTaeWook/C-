using API.Socket.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace API.Socket
{
    public abstract class ClientBase
    {
        private FunctionMap functionMap;
        
        #region abstract
        protected abstract void ConnectCompleteEvent(StateObject state);
        public abstract void Send(int protocol, byte[] data);
        protected abstract void DisconnectedEvent();
        protected abstract void ClosePeer();
        public virtual void Close() { functionMap.Clear(); functionMap = null; }
        public abstract void Connect(string ip, int port, int timeout = 5000);
        protected virtual bool PacketConversionComplete(Data.Packet.Packet packet, out object[] arg)
        {
            arg = null;
            return true;
        }
        protected virtual Data.Enum.VertifyResult VerifyPacket(Data.Packet.Packet packet) { return Data.Enum.VertifyResult.Vertify_Accept; }
        protected virtual void ForwardFunc(Data.Packet.Packet packet) { }
        #endregion abstract

        public ClientBase()
        {
            functionMap = new FunctionMap();
        }
        public void BindCallback(int protocol, Action<Data.Packet.Packet> func)
        {
            functionMap.BindCallback(protocol, func);
        }
        protected bool FindKey(int key)
        {
            return functionMap.FindKey(key);
        }
        public void BindCallback<T>(int protocol, T func)
        {
            if (func.GetType().BaseType.Equals(Type.GetType("System.MulticastDelegate")))
            {
                if (func.GetType().GenericTypeArguments[0].Equals(Type.GetType("API.Socket.Data.Packet.Packet")))
                {
                    functionMap.BindCallback(protocol, func);
                }
                else
                {
                    throw new FormatException("Invalid delegate parameter order");
                }
            }
            else
            {
                throw new FormatException("Type is not a function delegate");
            }
        }
        protected void RunCallbackFunc(int protocol, Data.Packet.Packet packet)
        {
            functionMap.RunCallback(protocol, packet);
        }
    }
}
