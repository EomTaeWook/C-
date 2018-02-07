using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Socket.Data
{
    public class ServerFunctionMap
    {
        private Dictionary<int, Delegate> _funcList;

        public ServerFunctionMap()
        {
            _funcList = new Dictionary<int, Delegate>();
        }
        public void BindCallback<T>(int protocol, T func)
        {
            try
            {
                if (func == null) return;
                var action = (func as Delegate);
                if (action == null)
                {
                    throw new Exception.Exception("Function Not Delegate Type");
                }
                _funcList.Add(protocol, action);
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        public bool FindKey(int key)
        {
            return _funcList.ContainsKey(key);
        }
        public void Clear()
        {
            _funcList.Clear();
        }
        public void RunCallback(int protocol, Packet.Packet packet, StateObject handler, params object[] arg)
        {
            try
            {
                object[] o = { packet, handler };
                if (arg != null) o = o.Concat(arg).ToArray();
                _funcList[protocol].DynamicInvoke(o);
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception("protocol : " + protocol + " msg : " + ex.Message);
            }
        }
    }

    public class FunctionMap
    {
        private Dictionary<int, Delegate> funcList;
        public FunctionMap()
        {
            funcList = new Dictionary<int, Delegate>();
        }
        public void Clear()
        {
            funcList.Clear();
        }
        public void BindCallback<T>(int protocol, T func)
        {
            try
            {
                if (func == null) return;
                var action = (func as Delegate);
                if (action == null)
                {
                    throw new Exception.Exception("Function Not Delegate Type");
                }
                funcList.Add(protocol, action);
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        public bool FindKey(int key)
        {
            return funcList.ContainsKey(key);
        }
        public void RunCallback(int protocol, Packet.Packet packet, params object[] arg)
        {
            try
            {
                object[] o = { packet };
                if (arg != null) o = o.Concat(arg).ToArray();
                funcList[protocol].DynamicInvoke(o.Concat(arg).ToArray());
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
    }
}
