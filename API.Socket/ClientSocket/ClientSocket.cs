using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.ClientSocket
{
    public abstract class ClientSocket<FuncKeyType> : ClientBase
    {
        private Dictionary<FuncKeyType, MulticastDelegate> _funcMap;
        protected ClientSocket()
        {
            _funcMap = new Dictionary<FuncKeyType, MulticastDelegate>();
        }
        public void BindCallback<T>(FuncKeyType key, Action<T> func)
        {
            _funcMap.Add(key, func);
        }
        public void BindCallback<T, T1>(FuncKeyType key, Action<T, T1> func)
        {
            _funcMap.Add(key, func);
        }
        public void BindCallback<T, T1, T2>(FuncKeyType key, Action<T, T1, T2> func)
        {
            _funcMap.Add(key, func);
        }
        public void RunCallback(FuncKeyType key, params object[] param)
        {
            try
            {
                if (!_funcMap.ContainsKey(key))
                    throw new KeyNotFoundException();
                _funcMap[key].DynamicInvoke(param);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}