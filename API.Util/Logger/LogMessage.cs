using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Util.Logger
{
    public class LogMessage : IMessage
    {
        public string Message { get; set; }
        public DateTimeOffset TimeStamp { get; set; }

        public int CompareTo(IMessage other)
        {
            return TimeStamp.Ticks.CompareTo(other.TimeStamp.Ticks);
        }
    }
}
