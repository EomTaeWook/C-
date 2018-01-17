using A4ProductData.Data.Common.SWInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Request.SwInfo
{
    [JsonObject]
    public class ReqSWInfo : Request
    {
        private SystemInfo systemInfo;
        private TagInfo tagInfo;

        public ReqSWInfo()
        {
            systemInfo = new SystemInfo();

            tagInfo = new TagInfo();
        }
        [JsonProperty("tag", NullValueHandling = NullValueHandling.Include)]
        public TagInfo TagInfo { get => tagInfo; set => tagInfo = value; }

        [JsonProperty("system", NullValueHandling = NullValueHandling.Include)]
        public SystemInfo SystemInfo { get => systemInfo; set => systemInfo = value; }
    }
}
