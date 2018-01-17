using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Response
{
    [JsonObject]
    public class Response : Pong
    {
        private int rstCode = 0;
        private string rstRsn;
        protected ResultObj rstObj;

        public Response()
        {
            this.MsgType = Enum.MsgCode.GetMsgTypeString(Enum.MsgCode.MsgType.Response);
            rstObj = new ResultObj();
        }

        [JsonProperty("rstCode", NullValueHandling = NullValueHandling.Include)]
        public int ResultCode { get => rstCode; set => rstCode = value; }

        [JsonProperty("rstRsn", NullValueHandling = NullValueHandling.Include)]
        public string ResultRsn { get => rstRsn; set => rstRsn = value; }

        [JsonProperty("rstObj", NullValueHandling = NullValueHandling.Ignore)]
        public ResultObj RstObj { get => rstObj; set => rstObj = value; }

        public T GetRstObject<T>() where T : ResultObj
        {
            return (T)rstObj;
        }
    }

    [JsonObject]
    public class ResultObj
    {
    }
}
