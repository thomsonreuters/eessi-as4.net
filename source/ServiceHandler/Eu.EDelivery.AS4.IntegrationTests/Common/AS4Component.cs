using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    public class AS4Component
    {
        public static FileInfo SubmitSinglePayloadImage => new FileInfo(Path.GetFullPath(@".\" + Properties.Resources.submitmessage_single_payload_path));

        public static FileInfo SubmitSecondPayloadXml => new FileInfo(Path.GetFullPath($".{Properties.Resources.submitmessage_second_payload_path}"));

        public static string HostAddress
        {
            get
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                throw new Exception("Local IP Address Not Found!");
            }
        }
    }
}
