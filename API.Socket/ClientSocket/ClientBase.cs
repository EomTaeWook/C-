using API.Socket.Data;
using API.Socket.Data.Packet;
using System;

namespace API.Socket
{
    public abstract class ClientBase
    {
        private FunctionMap _functionMap;

        #region abstract
        protected abstract void ConnectCompleteEvent(StateObject state);
        public abstract void Send(int protocol, byte[] data);
        protected abstract void DisconnectedEvent();
        protected abstract void ClosePeer();
        public virtual void Close() { _functionMap.Clear(); _functionMap = null; }
        public abstract void Connect(string ip, int port, int timeout = 5000);
        protected virtual bool PacketConversionComplete(Packet packet, out object[] arg)
        {
            arg = null;
            return true;
        }
        protected virtual Data.Enum.VertifyResult VerifyPacket(Packet packet) { return Data.Enum.VertifyResult.Vertify_Accept; }
        protected virtual void ForwardFunc(Packet packet) { }
        #endregion abstract

        public ClientBase()
        {
            _functionMap = new FunctionMap();
        }
        protected bool FindKey(int key)
        {
            return _functionMap.FindKey(key);
        }
        public void BindCallback<T1, T2, T3, T4>(int protocol, Action<Packet, T1, T2, T3, T4> func)
        {
            try
            {
                _functionMap.BindCallback(protocol, func);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex);
            }
        }
        public void BindCallback<T1, T2, T3>(int protocol, Action<Packet, T1, T2, T3> func)
        {
            try
            {
                _functionMap.BindCallback(protocol, func);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex);
            }
        }
        public void BindCallback<T1, T2>(int protocol, Action<Packet, T1, T2> func)
        {
            try
            {
                _functionMap.BindCallback(protocol, func);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex);
            }
        }
        public void BindCallback<T>(int protocol, Action<Packet, T> func)
        {
            try
            {
                _functionMap.BindCallback(protocol, func);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex);
            }
        }
        public void BindCallback(int protocol, Action<Packet> func)
        {
            try
            {
                _functionMap.BindCallback(protocol, func);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex);
            }
        }
        protected void RunCallbackFunc(int protocol, Packet packet)
        {
            _functionMap.RunCallback(protocol, packet);
        }
    }
}
