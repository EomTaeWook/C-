using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Response.PdtInfo
{
    [JsonObject]
    public class ResPdtInfo : Response
    {
        public ResPdtInfo()
        {
            RstObj = new ResPdtInfoResult();
        }
    }

    [JsonObject]
    public class ResPdtInfoResult  : ResultObj
    {
        private string downInvition = "";
        private string netAddr = "";
        private string netPort = "";
        private string netType = "";
        private string urlPath = "";
        private string tgtHash = "";
        private string tgtFileNm = "";
        private string tgtFileSize = "";

        [JsonProperty("downInvition", NullValueHandling = NullValueHandling.Include)]
        public string DownInvition { get => downInvition; set => downInvition = value; }

        [JsonProperty("netAddr", NullValueHandling = NullValueHandling.Include)]
        public string NetAddr { get => netAddr; set => netAddr = value; }

        [JsonProperty("netPort", NullValueHandling = NullValueHandling.Include)]
        public string NetPort { get => netPort; set => netPort = value; }

        [JsonProperty("netType", NullValueHandling = NullValueHandling.Include)]
        public string NetType { get => netType; set => netType = value; }

        [JsonProperty("urlPath", NullValueHandling = NullValueHandling.Include)]
        public string UrlPath { get => urlPath; set => urlPath = value; }

        [JsonProperty("tgtHash", NullValueHandling = NullValueHandling.Include)]
        public string TgtHash { get => tgtHash; set => tgtHash = value; }

        [JsonProperty("tgtFileNm", NullValueHandling = NullValueHandling.Include)]
        public string TgtFileNm { get => tgtFileNm; set => tgtFileNm = value; }

        [JsonProperty("tgtFileSize", NullValueHandling = NullValueHandling.Include)]
        public string TgtFileSize { get => tgtFileSize; set => tgtFileSize = value; }
    }
}
