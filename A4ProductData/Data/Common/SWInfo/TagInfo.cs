using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Common.SWInfo
{
    [JsonObject]
    public class TagInfo
    {
        private string pdtCategory = "";
        private string swID = "";
        private string swDisp = "";
        private string swClass = "";
        private string swVersion = "";
        private string swBuild = "";
        private string istDate = "";
        private string istDrive = "";

        public TagInfo()
        {
            IstDrive = Environment.CurrentDirectory;
        }

        [JsonProperty("pdtCategroy")]
        public string ProductCategory { get => pdtCategory; set => pdtCategory = value; }
        [JsonProperty("swId")]
        public string SWID { get => swID; set => swID = value; }
        [JsonProperty("swDisp")]
        public string SWDisp { get => swDisp; set => swDisp = value; }
        [JsonProperty("swClass")]
        public string SWClass { get => swClass; set => swClass = value; }
        [JsonProperty("swVersion")]
        public string SWVersion { get => swVersion; set => swVersion = value; }
        [JsonProperty("swBuild")]
        public string SWBuild { get => swBuild; set => swBuild = value; }
        [JsonProperty("istDate")]
        public string IstDate { get => istDate; set => istDate = value; }
        [JsonProperty("istDrive")]
        public string IstDrive { get => istDrive; set => istDrive = value; }
    }
}
