using API.Socket.Data;
using API.Socket.Data.Packet;
using System;
using System.Diagnostics;

namespace API.Socket
{
    public abstract class ServerBase
    {
        private ServerFunctionMap _functionMap;
        protected MutexCount _acceptCount;
        #region abstract | virtual
        public abstract void Send(StateObject handler, Packet packet);
        public virtual void Close() { _functionMap.Clear(); _functionMap = null; }
        public abstract void Start();
        public abstract void Reject(StateObject handler);
        protected abstract void AcceptComplete(StateObject state);
        protected abstract void DisconnectedComplete(ulong handerKey);
        protected virtual bool PacketConversionComplete(Packet packet, StateObject handler, out object[] arg)
        {
            arg = null;
            return true;
        }
        public virtual void BroadCast(Data.Packet.Packet packet, StateObject state) { }
        protected virtual void ForwardFunc(Data.Packet.Packet packet, StateObject stateObject) { }
        protected virtual void CallbackComplete(Data.Packet.Packet packet, StateObject stateObject) { }
        protected virtual Data.Enum.VertifyResult VerifyPacket(Data.Packet.Packet packet, StateObject handler)
        {
            return Data.Enum.VertifyResult.Vertify_Accept;
        }
        #endregion
        public ServerBase()
        {
            _functionMap = new ServerFunctionMap();
            _acceptCount = new MutexCount();
        }
        public void BindCallback(int protocol, Action<Packet, StateObject> func)
        {
            BindCallback<Action<Data.Packet.Packet, StateObject>>(protocol, func);
        }
        public void BindCallback<T>(int protocol, T func)
        {
            try
            {
                if (func.GetType().BaseType.Equals(Type.GetType("System.MulticastDelegate")))
                {
                    if (func.GetType().GenericTypeArguments[0].Equals(Type.GetType("API.Socket.Data.Packet.Packet"))
                        && func.GetType().GenericTypeArguments[1].Equals(Type.GetType("API.Socket.Data.StateObject")))
                    {
                        _functionMap.BindCallback(protocol, func);
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
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message, ex);
            }
        }
        protected bool RunCallbackFunc(int protocol, Packet packet, StateObject stateObject, params object[] arg)
        {
            try
            {
                _functionMap.RunCallback(protocol, packet, stateObject, arg);
                return true;
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
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
            Packet packet = new Packet();
            packet.GetHeader().DataSize = Convert.ToUInt32(data.Length);
            packet.GetHeader().Protocol = protocol;
            packet.Data = data;
            Send(handler, packet);
        }
    }
}
