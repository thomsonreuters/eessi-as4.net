using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    /// <summary>
    /// Responsible to perform the operations within the AS4 Component.
    /// </summary>
    public class AS4Component : IDisposable
    {
        private Process _as4ComponentProcess;

        /// <summary>
        /// Gets the host address on which the AS4 Component will be run.
        /// </summary>
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

        /// <summary>
        /// Start AS4 Component
        /// </summary>
        public void Start()
        {
            _as4ComponentProcess = Process.Start("Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe");

            if (_as4ComponentProcess != null)
            {
                Console.WriteLine($@"Application Started with Process Id: {_as4ComponentProcess.Id}");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_as4ComponentProcess.HasExited)
            {
                _as4ComponentProcess.Kill();
            }
        }
    }
}
