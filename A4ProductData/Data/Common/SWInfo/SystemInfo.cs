using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Management.ManagementObjectCollection;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace A4ProductData.Data.Common.SWInfo
{
    [JsonObject]
    public class SystemInfo
    {
        private string osType;
        private string osVersion;
        private string coreType;
        private string ramTotal;
        private string ramFree;
        private string mchnName;
        private string cpuusage = "0";

        private string adtName = "";
        private string adtType = "";
        private string adtIp4 = "";
        private string adtIp6 = "";
        private string adtMacAddr = "";

        private string hddName = "";
        private string hddType = "";
        private string hddFormat = "";
        private string hddFreeByte = "0";
        private string hddTotalByte = "0";

        public SystemInfo()
        {

            #region adtinfo
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            var v = nics.Where(r => r.OperationalStatus == OperationalStatus.Up && r.NetworkInterfaceType != NetworkInterfaceType.Loopback && r.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
            var net = v.ToList();
            if (net.Count > 0)
            {
                adtName = net[0].Description;
                adtType = net[0].NetworkInterfaceType.ToString();
                string tmp = net[0].GetPhysicalAddress().ToString();
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (i % 2 == 0 && i != 0)
                    {
                        adtMacAddr += "-";
                    }
                    adtMacAddr += tmp[i];
                }
                foreach (var ipInfo in net[0].GetIPProperties().UnicastAddresses)
                {
                    if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        adtIp6 = ipInfo.Address.ToString();
                    }
                    if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        adtIp4 = ipInfo.Address.ToString();
                    }
                }
            }
            #endregion adtInfo

            #region HDDInfo
            var rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var drive = allDrives.Where(r => r.Name == rootPath);
            if (drive.Count() > 0)
            {
                var arr = drive.ToArray();
                hddName = rootPath.Replace(":\\", "");
                hddType = arr[0].DriveType.ToString();
                hddFormat = arr[0].DriveFormat;
                hddFreeByte = arr[0].TotalFreeSpace.ToString();
                hddTotalByte = arr[0].TotalSize.ToString();
            }
            #endregion HDDInfo

            var computerInfo = new ComputerInfo();
            mchnName = Environment.MachineName;
            osType = computerInfo.OSFullName;
            osVersion = computerInfo.OSVersion;
            ramTotal = computerInfo.TotalPhysicalMemory.ToString();
            ramFree = computerInfo.AvailablePhysicalMemory.ToString();

            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            ManagementObjectEnumerator enumer = moc.GetEnumerator();
            if (enumer.MoveNext())
            {
                string name = enumer.Current.Properties["Name"].Value.ToString();
                coreType = name;
            }
            mc = new ManagementClass("Win32_BaseBoard");
            moc = mc.GetInstances();
            enumer = moc.GetEnumerator();
            if (enumer.MoveNext())
            {
                string name = enumer.Current.Properties["Product"].Value.ToString();
            }
        }

        [JsonProperty("osType")]
        public string OSType { get => osType; set => osType = value; }
        [JsonProperty("osVersion")]
        public string OSVersion { get => osVersion; set => osVersion = value; }
        [JsonProperty("coreType")]
        public string CoreType { get => coreType; set => coreType = value; }
        [JsonProperty("ramTotal")]
        public string RamTotal { get => ramTotal; set => ramTotal = value; }
        [JsonProperty("ramFree")]
        public string RamFree { get => ramFree; set => ramFree = value; }
        [JsonProperty("mchnName")]
        public string MchnName { get => mchnName; set => mchnName = value; }
        [JsonProperty("cpuUsage")]
        public string CpuUsage { get => cpuusage; set => cpuusage = value; }

        [JsonProperty("adtName")]
        public string AdtName { get => adtName; set => adtName = value; }
        [JsonProperty("adtType")]
        public string AdtType { get => adtType; set => adtType = value; }
        [JsonProperty("ip4")]
        public string AdtIP4 { get => adtIp4; set => adtIp4 = value; }
        [JsonProperty("ip6")]
        public string AdtIP6 { get => adtIp6; set => adtIp6 = value; }
        [JsonProperty("macAddr")]
        public string AdtMacAddr { get => adtMacAddr; set => adtMacAddr = value; }

        [JsonProperty("hddDrvName")]
        public string HddName { get => hddName; set => hddName = value; }
        [JsonProperty("hddType")]
        public string HddType { get => hddType; set => hddType = value; }
        [JsonProperty("hddFormat")]
        public string HddFormat { get => hddFormat; set => hddFormat = value; }
        [JsonProperty("hddFreeByte")]
        public string HddFreeSpace { get => hddFreeByte; set => hddFreeByte = value; }
        [JsonProperty("hddTotalByte")]
        public string HddTotalSpace { get => hddTotalByte; set => hddTotalByte = value; }
    }
}
