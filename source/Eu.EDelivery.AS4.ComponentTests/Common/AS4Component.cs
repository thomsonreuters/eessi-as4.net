using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    /// <summary>
    /// Responsible to perform the operations within the AS4 Component.
    /// </summary>
    public class AS4Component : IDisposable
    {
        private readonly Process _as4ComponentProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Component"/> class.
        /// </summary>
        /// <param name="as4Process">The as 4 Process.</param>
        private AS4Component(Process as4Process)
        {
            _as4ComponentProcess = as4Process;
        }

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
        /// <param name="location">The location.</param>
        /// <param name="cleanSlate">if set to <c>true</c> [clean slate].</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No AS4 MSH found in the specified location.</exception>
        public static AS4Component Start(string location, bool cleanSlate = true)
        {
            const string appFileName = "Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe";

            if (Directory.Exists(location) == false || File.Exists(appFileName) == false)
            {
                throw new InvalidOperationException("No AS4 MSH found in the specified location.");
            }

            var workingDirectory = new DirectoryInfo(location);

            if (cleanSlate)
            {
                CleanupWorkingDirectory(location);
            }

            var mshInfo = new ProcessStartInfo(Path.Combine(workingDirectory.FullName, appFileName))
            {
                WorkingDirectory = workingDirectory.FullName,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            var as4Msh = new AS4Component(Process.Start(mshInfo));

            // Wait a little bit to make sure the DB is created.
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(25));

            return as4Msh;
        }

        private static void CleanupWorkingDirectory(string location)
        {
            string databaseFolder = Path.Combine(location, "database");

            if (Directory.Exists(databaseFolder))
            {
                var databaseDirectory = new DirectoryInfo(databaseFolder);
                DeleteAllFilesAndFolders(databaseDirectory);
            }
        }

        private static void DeleteAllFilesAndFolders(DirectoryInfo directory)
        {
            DirectoryInfo[] subFolders = directory.GetDirectories();

            if (subFolders.Any())
            {
                foreach (DirectoryInfo subdirectory in subFolders)
                {
                    DeleteAllFilesAndFolders(subdirectory);
                    subdirectory.Delete();
                }
            }

            foreach (FileInfo file in directory.GetFiles("*.*"))
            {
                file.Delete();
            }
        }
        
        /// <summary>
        /// Gets the <see cref="IConfig"/> implementation for the given <see cref="AS4Component"/>.
        /// </summary>
        /// <returns></returns>
        public IConfig GetConfiguration()
        {
            Config config = Config.Instance;

            if (config.IsInitialized == false)
            {
                config.Initialize("settings.xml");
            }

            return config;
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
