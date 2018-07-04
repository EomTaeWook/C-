using API.Socket.Data;
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
        public void BindCallback<T>(ProtocolType protocol, Action<StateObject, T> callback)
        {
            _funcMap.Add(protocol, callback);
        }
        public void BindCallback<T, T1>(ProtocolType protocol, Action<StateObject, T, T1> callback)
        {
            _funcMap.Add(protocol, callback);
        }
        public void BindCallback<T, T1, T2>(ProtocolType protocol, Action<StateObject, T, T1, T2> callback)
        {
            _funcMap.Add(protocol, callback);
        }
        public bool RunCallback(ProtocolType protocol, StateObject stateObject, params object[] param)
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
