using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Request
{
    [JsonObject]
    public class Request : Data.Base.JsonObject
    {
        private int msgCode = 0;
        private string timeStamp;
        private string swId;
        private string msgType = Data.Enum.MsgCode.GetMsgTypeString(Enum.MsgCode.MsgType.Request);
        public Request()
        {
            var timeSpan = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            TimeStamp = timeSpan.TotalSeconds.ToString();
        }

        [JsonProperty("swId", NullValueHandling = NullValueHandling.Ignore)]
        public string SWID { get => swId; set => swId = value; }

        [JsonProperty("msgCode", NullValueHandling = NullValueHandling.Include)]
        public int MsgCode { get => msgCode; set => msgCode = value; }

        [JsonProperty("tmStamp", NullValueHandling = NullValueHandling.Include)]
        public string TimeStamp { get => timeStamp; set => timeStamp = value; }

        [JsonProperty("msgType", NullValueHandling = NullValueHandling.Include)]
        public string MsgType
        {
            get => msgType;
            set
            {
                if (!value.Equals(""))
                {
                    msgType = value[0].ToString().ToUpper();
                    for (int i = 1; i < value.Length; i++)
                    {
                        msgType += value[i].ToString().ToLower();
                    }
                }
            }
        }
        public void SetMsgType(Data.Enum.MsgCode.MsgType msgType)
        {
            MsgType = Data.Enum.MsgCode.GetMsgTypeString(msgType);
        }
    }
}
