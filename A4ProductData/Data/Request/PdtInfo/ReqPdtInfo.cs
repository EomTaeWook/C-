using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Request.PdtInfo
{
    [JsonObject]
    public class ReqPdtInfo : Request
    {
        private Common.PdtInfo.Application app;

        [JsonProperty("Application", NullValueHandling = NullValueHandling.Include)]
        public Common.PdtInfo.Application App { get => app; set => app = value; }
    }
}
