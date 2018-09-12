using API.Socket.InternalStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.ServerSocket
{
    public abstract class ServerSocket<ProtocolType> : ServerBase
    {
        private Dictionary<ProtocolType, MulticastDelegate> _funcMap;
        protected ServerSocket()
        {
            _funcMap = new Dictionary<ProtocolType, MulticastDelegate>();
        }
        public void BindCallback<T>(ProtocolType protocol, Action<StateObject, T> func)
        {
            _funcMap.Add(protocol, func);
        }
        public void BindCallback<T, T1>(ProtocolType protocol, Action<StateObject, T, T1> func)
        {
            _funcMap.Add(protocol, func);
        }
        public void BindCallback<T, T1, T2>(ProtocolType protocol, Action<StateObject, T, T1, T2> func)
        {
            _funcMap.Add(protocol, func);
        }
        public bool OnCallback(ProtocolType protocol, StateObject stateObject, params object[] param)
        {
            try
            {
                if (!_funcMap.ContainsKey(protocol))
                    throw new KeyNotFoundException();
                object[] parameter = { stateObject };
                if (param != null)
                    parameter = parameter.Concat(param).ToArray();
                _funcMap[protocol].DynamicInvoke(parameter);
                return true;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}
