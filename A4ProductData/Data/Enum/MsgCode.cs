using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Enum
{
    public class MsgCode
    {
        public enum MsgType
        {
            Request = 0,
            Ack,
            Response,
            Report,
            Notify
        }
        public static MsgType GetMsgType(string type)
        {
            string t = "";
            if(!type.Equals(""))
            {
                for (int i = 1; i < type.Length; i++)
                {
                    t += type[i].ToString().ToLower();
                }
            }
            return (MsgType)System.Enum.Parse(typeof(MsgType), t);
        }
        public static string GetMsgTypeString(MsgType msgType)
        {
            string value = "";
            switch (msgType)
            {
                case MsgType.Request:
                    value = "Request";
                    break;
                case MsgType.Ack:
                    value = "Ack";
                    break;
                case MsgType.Response:
                    value = "Response";
                    break;
                case MsgType.Report:
                    value = "Report";
                    break;
                case MsgType.Notify:
                    value = "Notify";
                    break;
            }

            return value;
        }
    }
}
