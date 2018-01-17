using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Response.SwInfo
{
    [JsonObject]
    public class ResSWInfo : Response
    {
        private string rptCycle;

        [JsonProperty("rptCycle", NullValueHandling = NullValueHandling.Include)]
        public string RptCycle
        {
            get { return rptCycle; }
            set { rptCycle = value; }
        }
    }
}
