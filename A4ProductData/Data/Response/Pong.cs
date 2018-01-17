using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Response
{
    [JsonObject]
    public class Pong : Data.Base.JsonObject
    {
        private int msgCode = 0;
        private string msgType = Enum.MsgCode.GetMsgTypeString(Enum.MsgCode.MsgType.Ack);
        private string taskID = "";
        private string timeStamp = "";
        public Pong()
        {
            var timeSpan = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            TimeStamp = timeSpan.TotalSeconds.ToString();
        }

        [JsonProperty("msgCode", NullValueHandling = NullValueHandling.Include)]
        public int MsgCode { get => msgCode; set => msgCode = value; }

        [JsonProperty("msgType")]
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

        [JsonProperty("taskId", NullValueHandling = NullValueHandling.Include)]
        public string TaskID { get => taskID; set => taskID = value; }

        [JsonProperty("tmStamp", NullValueHandling = NullValueHandling.Include)]
        public string TimeStamp { get => timeStamp; set => timeStamp = value; }

    }
}
