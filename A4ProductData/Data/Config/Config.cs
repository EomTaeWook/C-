using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A4ProductData.Data.Config
{
    public class Config
    {
        private string productCategory = "";
        private string softwareNickname = "";
        private string customer = "";
        private string ip = "";
        private string port = "";
        private string run = "";
        private string swID = "";

        #region regInfo
        private string productNickname = "";
        private string version = "0.0.0";
        private string build = "0";
        private string path = "";
        #endregion 

        public string ProductCategory { get => productCategory; set => productCategory = value; }
        public string SoftwareNickname { get => softwareNickname; set => softwareNickname = value; }
        public string Customer { get => customer; set => customer = value; }
        public string IP { get => ip; set => ip = value; }
        public string Port { get => port; set => port = value; }
        public string Run { get => run; set => run = value; }
        public string SWID { get => productCategory + softwareNickname + customer; set => swID = value; }

        public string ProductNickname { get => productNickname; set => productNickname = value; }
        public string Version { get => version; set => version = value; }
        public string Build { get => build; set => build = value; }
        public string Path { get => path; set => path = value; }
    }
}
