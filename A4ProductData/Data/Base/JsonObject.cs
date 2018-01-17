namespace A4ProductData.Data.Base
{
    public class JsonObject
    {
        public string GetJsonText()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
        public byte[] GetJsonDataConvertByte()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            return System.Text.Encoding.Default.GetBytes(json);
        }
        public static T GetJsonDataConvertObject<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
