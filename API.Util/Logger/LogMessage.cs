using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Util.Logger
{
    public class LogMessage : IComparable<LogMessage>
    {
        public string Message { get; set; }
        public DateTimeOffset CreateTime { get; private set; } = DateTimeOffset.Now;

        public int CompareTo(LogMessage other)
        {
            return CreateTime.Ticks.CompareTo(other.CreateTime.Ticks);
        }
    }
}
