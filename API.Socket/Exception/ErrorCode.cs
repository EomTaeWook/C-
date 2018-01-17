using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.Exception
{
    public enum ErrorCode
    {

        UnknownError = -1,
        Success = 0,
        ParameterIncorrect = 87,

        //500
        InternalError = 500,



        //10000
        SocketDisConnect = 10057,

    }
}
