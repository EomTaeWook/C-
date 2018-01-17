using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Common.PdtInfo
{
    [JsonObject]
    public class Application
    {
        private string pdtCategory = "";
        private string swNick = "";
        private string swCustom = "";
        private string swSetup = "";
        private string swVersion = "";
        private string swBuild = "";
        private string userID = "";
        private string userPW = "";


        [JsonProperty("pdtCategory", NullValueHandling = NullValueHandling.Include)]
        public string PdtCategory { get => pdtCategory; set => pdtCategory = value; }

        [JsonProperty("swNick", NullValueHandling = NullValueHandling.Include)]
        public string SwNick { get => swNick; set => swNick = value; }

        [JsonProperty("swCustom", NullValueHandling = NullValueHandling.Include)]
        public string SwCustom { get => swCustom; set => swCustom = value; }

        [JsonProperty("swSetup", NullValueHandling = NullValueHandling.Include)]
        public string SwSetup { get => swSetup; set => swSetup = value; }

        [JsonProperty("swVersion", NullValueHandling = NullValueHandling.Include)]
        public string SwVersion { get => swVersion; set => swVersion = value; }

        [JsonProperty("swBuild", NullValueHandling = NullValueHandling.Include)]
        public string SwBuild { get => swBuild; set => swBuild = value; }

        [JsonProperty("userID", NullValueHandling = NullValueHandling.Include)]
        public string UserID { get => userID; set => userID = value; }

        [JsonProperty("userPW", NullValueHandling = NullValueHandling.Include)]
        public string UserPW { get => userPW; set => userPW = value; }
    }
}
