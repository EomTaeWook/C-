using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace API.Socket.Exception
{
    public class Exception : System.Exception
    {
        private int _errorCode = 0;
        private string _message;
        private string _functionName = "";
        public Exception(string message, [CallerMemberName] string functionName = "") : this(-1, message, functionName)
        {
        }
        public Exception(ErrorCode errorCode, string message, [CallerMemberName] string functionName = "") : this((int)errorCode, message, functionName)
        {
        }
        public Exception(int errorCode, string message, [CallerMemberName] string functionName = "")
        {
            _errorCode = errorCode;
            _message = message;
            _functionName = functionName;
            Debug.WriteLine(_message);
        }
        public string GetErrorMessage()
        {
            return "ErrorCode : " + _errorCode + " Function : " + _functionName + " message : " + _message;
        }

        public int ErrorCode { get => _errorCode; }
        public override string Message { get => _message; }
        public string FunctionName { get => _functionName; set => _functionName = value; }
    }
}
