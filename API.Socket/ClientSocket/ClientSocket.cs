using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.ClientSocket
{
    public abstract class ClientSocket<ProtocolType> : ClientBase
    {
        private Dictionary<ProtocolType, MulticastDelegate> _funcMap;
        protected ClientSocket()
        {
            _funcMap = new Dictionary<ProtocolType, MulticastDelegate>();
        }
        public void BindCallback<T>(ProtocolType protocol, Action<T> func)
        {
            _funcMap.Add(protocol, func);
        }
        public void BindCallback<T, T1>(ProtocolType protocol, Action<T, T1> func)
        {
            _funcMap.Add(protocol, func);
        }
        public void BindCallback<T, T1, T2>(ProtocolType protocol, Action<T, T1, T2> func)
        {
            _funcMap.Add(protocol, func);
        }
        public void OnCallback(ProtocolType protocol, params object[] param)
        {
            try
            {
                if (!_funcMap.ContainsKey(protocol))
                    throw new KeyNotFoundException();
                _funcMap[protocol].DynamicInvoke(param);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}