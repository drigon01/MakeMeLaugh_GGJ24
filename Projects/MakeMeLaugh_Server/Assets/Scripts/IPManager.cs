using System.Net.NetworkInformation;

public static class IPManager
{
  public static string GetLocalIPAddress()
  {
    var nics = NetworkInterface.GetAllNetworkInterfaces();
    foreach (var nic in nics)
    {
      if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
      {
        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
        {
          if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
          {
            return ip.Address.ToString();
          }
        }
      }
    }

    throw new System.Exception("No network adapters with an IPv4 address in the system!");
  }
}
