using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.PerformanceTests.LargeMessages;

namespace Eu.EDelivery.AS4.PerformanceTests
{
    /// <summary>
    /// Facade for the AS4 Component as a Corner.
    /// </summary>
    public class Corner : IDisposable
    {
        private readonly DirectoryInfo _cornerDirectory;

        private Process _cornerProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="Corner"/> class.
        /// </summary>
        /// <param name="cornerDirectory"></param>
        private Corner(DirectoryInfo cornerDirectory)
        {
            _cornerDirectory = cornerDirectory;
        }

        /// <summary>
        /// Place the given <paramref name="messageContents"/> at the Corner's location to retrieve files.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="messageContents">Content of the message to send.</param>
        public void PlaceLargeMessage(int value, Size metric, string messageContents)
        {
            string fileLocation = LargeMessage.CreateFile(_cornerDirectory, value, metric);
            messageContents = messageContents.Replace("__ATTACHMENTLOCATION__", fileLocation);

            PlaceMessages(messageCount: 1, messageContents: messageContents);
        }

        /// <summary>
        /// Place the given <paramref name="messageContents"/> <paramref name="messageCount"/> times at the Corner's location to retrieve files.
        /// </summary>
        /// <param name="messageCount">Amount of messages to send.</param>
        /// <param name="messageContents">Content of the message to send.</param>
        public void PlaceMessages(int messageCount, string messageContents)
        {
            Console.WriteLine($@"Placing {messageCount} submit messages...");

            for (var i = 0; i < messageCount; i++)
            {
                string id = Guid.NewGuid().ToString();
                string generatedMessage = messageContents.Replace("__ATTACHMENTID__", id);
                string outMessagePath = Path.Combine(_cornerDirectory.FullName, $@"messages\out\{id}.xml");

                File.WriteAllText(outMessagePath, generatedMessage);
            }
        }

        /// <summary>
        /// Gets the first delivered message length from the Corner's delivered target.
        /// </summary>
        /// <param name="searchPattern">The search Pattern.</param>
        /// <returns></returns>
        public int FirstDeliveredMessageLength(string searchPattern = "*")
        {
            FileInfo firstMessage = GetDeliveredFiles(searchPattern).FirstOrDefault();
            return firstMessage != null ? (int)firstMessage.Length : 0;
        }

        /// <summary>
        /// Count the messages that are delivered on the created corner.
        /// </summary>
        /// <param name="searchPattern">
        /// The search string to match against the names of files. This parameter can contain a combination of valid literal path
        /// and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. The default pattern is "*",
        /// which returns all files.
        /// </param>
        /// <returns></returns>
        public int CountDeliveredMessages(string searchPattern = "*")
        {
            return GetDeliveredFiles(searchPattern).Length;
        }

        private FileInfo[] GetDeliveredFiles(string searchPattern = "*")
        {
            return GetDirectory(rootDirectory: "messages", subDirectory: "in").GetFiles(searchPattern);
        }

        /// <summary>
        /// Count the number of files that are present in the Receipts directory.
        /// </summary>
        /// <returns></returns>
        public int CountReceivedReceipts()
        {
            return GetDirectory(rootDirectory: "messages", subDirectory: "receipts").GetFiles().Length;
        }

        /// <summary>
        /// Executes an Action when the specified <paramref name="numberOfReceipts"/> are received.
        /// </summary>
        /// <param name="numberOfReceipts"></param>
        /// <param name="action"></param>
        /// <param name="timeout">The maximum allowed timeframe before the method returns</param>
        /// <returns>True if the specified number of receipts is received; otherwise false.</returns>
        public bool ExecuteWhenNumberOfReceiptsAreReceived(int numberOfReceipts, Action action, TimeSpan timeout)
        {
            string receiptDirectory = GetDirectory(rootDirectory: "messages", subDirectory: "receipts").FullName;

            return ExecuteWhenNumberOfFilesAreFoundInDirectory(receiptDirectory, "*.xml", numberOfReceipts, action, timeout);
        }

        /// <summary>
        /// Executes an Action when the specified <paramref name="numberOfMessages"/> are delivered.
        /// </summary>
        /// <param name="numberOfMessages"></param>
        /// <param name="action"></param>
        /// <param name="timeout">The maximum allowed timeframe before the method returns</param>
        /// <param name="searchPattern"></param>
        /// <returns>True if the specified number of messages are delivered; otherwise false.</returns>
        public bool ExecuteWhenNumberOfMessagesAreDelivered(int numberOfMessages, Action action, TimeSpan timeout, string searchPattern = "*.*")
        {
            string deliverDirectoryName = GetDirectory(rootDirectory: "messages", subDirectory: "in").FullName;

            return ExecuteWhenNumberOfFilesAreFoundInDirectory(deliverDirectoryName, searchPattern, numberOfMessages, action, timeout);
        }

        private static bool ExecuteWhenNumberOfFilesAreFoundInDirectory(string location, string searchPattern, int numberOfFiles, Action action, TimeSpan timeout)
        {
            FileSystemWatcher fs = new FileSystemWatcher(location);
            fs.IncludeSubdirectories = false;

            ManualResetEvent waiter = new ManualResetEvent(false);
            bool allFilesFound = false;

            fs.EnableRaisingEvents = true;

            object syncRoot = new object();

            fs.Created += (o, args) =>
            {
                lock (syncRoot)
                {
                    if (Directory.GetFiles(location, searchPattern).Count() >= numberOfFiles)
                    {
                        fs.EnableRaisingEvents = false;
                        allFilesFound = true;
                        action();

                        waiter.Set();
                    }
                }
            };

            waiter.WaitOne(timeout);

            return allFilesFound;
        }

        /// <summary>
        /// Cleanup the delivered messages from the Corner's deliver directory.
        /// </summary>
        public void CleanupMessages()
        {
            CleanupDirectory(GetDirectory("messages", "in"));
            CleanupDirectory(GetDirectory("messages", "out"));
            CleanupDirectory(GetDirectory("messages", "receipts"));
            CleanupDirectory(GetDirectory("messages", "errors"));
            CleanupDirectory(GetDirectory("messages", "exceptions"));

            CleanupDirectory(GetDirectory("database", "as4messages\\out"));
            CleanupDirectory(GetDirectory("database", "as4messages\\in"));
        }

        private static void CleanupDirectory(DirectoryInfo directory)
        {
            DirectoryInfo messageDirectory = directory;

            foreach (FileInfo deliveredMessage in messageDirectory.GetFiles())
            {
                deliveredMessage.Delete();
            }
        }

        private DirectoryInfo GetDirectory(string rootDirectory, string subDirectory)
        {
            string deliverPath = Path.Combine(_cornerDirectory.FullName, rootDirectory, subDirectory);

            if (!Directory.Exists(deliverPath))
            {
                Directory.CreateDirectory(deliverPath);
            }

            return new DirectoryInfo(deliverPath);
        }

        /// <summary>
        /// Create a new instance of the <see cref="Corner"/> instance.
        /// </summary>
        /// <param name="prefix">Corner Prefix</param>
        /// <returns></returns>
        public static Task<Corner> StartNew(string prefix)
        {
            return Task.Run(
                () =>
                {
                    DirectoryInfo cornerDirectory = SetupCornerFixture(prefix);
                    var corner = new Corner(cornerDirectory);

                    Thread.Sleep(1000);
                    return corner;
                });
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _cornerProcess = CreateCornerProcessAt(_cornerDirectory);
        }

        private static Process CreateCornerProcessAt(FileSystemInfo cornerDirectory)
        {
            var cornerInfo =
                new ProcessStartInfo(
                    Path.Combine(cornerDirectory.FullName, "Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe"))
                {
                    WorkingDirectory = cornerDirectory.FullName
                };

            return Process.Start(cornerInfo);
        }

        private static DirectoryInfo SetupCornerFixture(string cornerPrefix)
        {
            var outputDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            Console.WriteLine($@"Corner {cornerPrefix} Directory set on {outputDirectory.FullName}");
            DirectoryInfo cornerDirectory = CreateCornerIn(outputDirectory, $"output-{cornerPrefix}");

            CopyDirectory(outputDirectory, cornerDirectory.FullName);
            IncludeCornerSettingsIn(cornerDirectory, $"{cornerPrefix}-settings.xml");
            IncludeCornerPModesIn(cornerDirectory);

            return cornerDirectory;
        }

        private static DirectoryInfo CreateCornerIn(FileSystemInfo outputDirectory, string corner)
        {
            return Directory.CreateDirectory(outputDirectory.FullName.Replace("output", corner));
        }

        private static void CopyDirectory(DirectoryInfo sourceDirectory, string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            CopyFiles(sourceDirectory, destDirName);
            CopySubDirectories(sourceDirectory, destDirName);
        }

        private static void CopyFiles(DirectoryInfo sourceDirectory, string destDirName)
        {
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                TryCopyFile(file, temppath);
            }
        }

        private static void TryCopyFile(FileInfo file, string temppath)
        {
            try
            {
                file.CopyTo(temppath, overwrite: true);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static void CopySubDirectories(DirectoryInfo sourceDirectory, string destDirName)
        {
            DirectoryInfo[] directories = sourceDirectory.GetDirectories();
            foreach (DirectoryInfo subDirectory in directories)
            {
                string temppath = Path.Combine(destDirName, subDirectory.Name);
                CopyDirectory(subDirectory, temppath);
            }
        }

        private static void IncludeCornerSettingsIn(DirectoryInfo cornerDirectory, string cornerSettingsFileName)
        {
            Func<FileInfo, bool> matchesSettingsFile =
                f =>
                    f.Name.Equals(cornerSettingsFileName, StringComparison.OrdinalIgnoreCase)
                    && f.DirectoryName?.IndexOf("performancetest-settings", StringComparison.OrdinalIgnoreCase) > -1;

            FileInfo cornerSettings = cornerDirectory
                .GetFiles("*.xml", SearchOption.AllDirectories)
                .FirstOrDefault(matchesSettingsFile);

            if (cornerSettings == null)
            {
                throw new FileNotFoundException($"Could not find the settings file: {cornerSettingsFileName}");
            }

            Console.WriteLine($@"Copy settings file: {cornerSettingsFileName}");
            string newSettingsFilePath = Path.Combine(cornerDirectory.FullName, "config", "settings.xml");
            File.Copy(cornerSettings.FullName, newSettingsFilePath, overwrite: true);

            DropSettingsDatabase(newSettingsFilePath);
        }

        private static void DropSettingsDatabase(string newSettingsFilePath)
        {
            string xml = File.ReadAllText(newSettingsFilePath);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            XmlNode connectionStringNode = xmlDocument.SelectSingleNode("//*[local-name()='ConnectionString']");
            if (connectionStringNode == null)
            {
                throw new XmlException($"No '<ConnectionString/>' node found in settings file: {newSettingsFilePath}");
            }

            string mshConnectionString = connectionStringNode.InnerText;

            // Modify the connectionstring so that we initially connect to the master - database.
            // Otherwise, the connection will fail if the database doesn't exist yet.
            var builder = new SqlConnectionStringBuilder(mshConnectionString);
            string databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            string masterConnectionString = builder.ConnectionString;

            using (var sqlConnection = new SqlConnection(masterConnectionString))
            {
                sqlConnection.Open();

                TryExecuteCommand(new SqlCommand($"DROP DATABASE IF EXISTS {databaseName}", sqlConnection));
            }
        }

        private static void TryExecuteCommand(IDbCommand dropDatabaseCommand)
        {
            try
            {
                Console.WriteLine(dropDatabaseCommand.CommandText);
                dropDatabaseCommand.ExecuteNonQuery();
            }
            catch (SqlException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static void IncludeCornerPModesIn(FileSystemInfo cornerDirectory)
        {
            Func<string, DirectoryInfo> getPModeDirectory =
                subFolder =>
                {
                    string directoryPath = Path.Combine(cornerDirectory.FullName, "config", subFolder);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    return new DirectoryInfo(directoryPath);
                };

            DirectoryInfo volumeSendPModes = getPModeDirectory(@"performancetest-settings\send-pmodes"),
                          volumeReceivePModes = getPModeDirectory(@"performancetest-settings\receive-pmodes");

            DirectoryInfo outputSendPModes = getPModeDirectory(@"send-pmodes"),
                          outputReceivePModes = getPModeDirectory(@"receive-pmodes");

            Console.WriteLine($@"Copy '{volumeSendPModes.FullName}' to '{outputSendPModes.FullName}'");
            CopyFiles(volumeSendPModes, outputSendPModes.FullName);

            Console.WriteLine($@"Copy '{volumeReceivePModes.FullName}' to '{outputReceivePModes.FullName}'");
            CopyFiles(volumeReceivePModes, outputReceivePModes.FullName);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_cornerProcess.HasExited)
            {
                _cornerProcess.Kill();
            }
        }
    }
}
