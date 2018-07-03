using API.Socket.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.ServerSocket
{
    public abstract class ServerSocket<FuncKeyType> : ServerBase
    {
        private Dictionary<FuncKeyType, MulticastDelegate> _funcMap;
        protected ServerSocket()
        {
            _funcMap = new Dictionary<FuncKeyType, MulticastDelegate>();
        }
        public void BindCallback<T>(FuncKeyType key, Action<StateObject, T> func)
        {
            _funcMap.Add(key, func);
        }
        public void BindCallback<T, T1>(FuncKeyType key, Action<StateObject, T, T1> func)
        {
            _funcMap.Add(key, func);
        }
        public void BindCallback<T, T1, T2>(FuncKeyType key, Action<StateObject, T, T1, T2> func)
        {
            _funcMap.Add(key, func);
        }
        public bool RunCallback(FuncKeyType key, StateObject stateObject, params object[] param)
        {
            try
            {
                if (!_funcMap.ContainsKey(key))
                    throw new KeyNotFoundException();
                object[] parameter = { stateObject };
                if (param != null)
                    parameter = parameter.Concat(param).ToArray();
                _funcMap[key].DynamicInvoke(parameter);
                return true;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}
