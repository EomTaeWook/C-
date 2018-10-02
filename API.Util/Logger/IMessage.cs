using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Util.Logger
{
    public interface IMessage : IComparable<IMessage>
    {
        string Message { get; set; }
        DateTimeOffset TimeStamp { get; set; }
    }
}
