using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace ESP32App.Common
{
    public class WiFiHelper
    {
        public static void Connect()
        {
            var configurations = Wireless80211Configuration.GetAllWireless80211Configurations();
            if (configurations.Length == 0)
            {
                Debug.WriteLine("WiFi 配置为空!");
                return;
            }

            var configuration = configurations[0];

            Connect(configuration);
        }
        
        public static void Connect(Wireless80211Configuration configuration)
        {
            WifiAdapter wifi = WifiAdapter.FindAllAdapters()[0];

            // Connect to network
            WifiConnectionResult result = wifi.Connect(configuration.Ssid, WifiReconnectionKind.Automatic, configuration.Password);
            if (result.ConnectionStatus is WifiConnectionStatus.Success)
            {
                Debug.WriteLine("连接成功");
            }
            else
            {
                Debug.WriteLine($"连接WiFi失败: {result.ConnectionStatus.ToString()}");
            }
        }
    }
}
