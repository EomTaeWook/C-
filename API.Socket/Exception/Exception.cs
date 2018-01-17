using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.Exception
{
    public class Exception : System.Exception
    {
        private int errorCode = 0;
        private string message;
        private string functionName = "";
        public Exception(string message, [CallerMemberName] string functionName = "") : this(-1, message, functionName)
        {
        }
        public Exception(ErrorCode errorCode, string message, [CallerMemberName] string functionName = "") : this((int)errorCode, message, functionName)
        {
        }
        public Exception(int errorCode, string message, [CallerMemberName] string functionName = "")
        {
            this.errorCode = errorCode;
            //this.message = "ErrorCode : " + this.errorCode + " Function : " + functionName + " message : " + message;
            this.message = message;
            this.functionName = functionName;
            Debug.WriteLine(this.message);
        }
        public string GetErrorMessage()
        {
            return "ErrorCode : " + this.errorCode + " Function : " + functionName + " message : " + message;
        }

        public int ErrorCode { get => errorCode; }
        public override string Message { get => this.message; }
        public string FunctionName { get => functionName; set => functionName = value; }
    }
}
