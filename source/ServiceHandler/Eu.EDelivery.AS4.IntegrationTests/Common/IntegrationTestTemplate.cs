using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.IntegrationTests.Fixture;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Integration Test Class to Perform common Tasks
    /// </summary>
    [Collection(HolodeckCollection.CollectionId)]
    public class IntegrationTestTemplate : IDisposable
    {
        protected static readonly string AS4IntegrationMessagesPath = Path.GetFullPath($@".\{Properties.Resources.submit_messages_path}\integrationtest-messages");
        protected static readonly string AS4MessagesRootPath = Path.GetFullPath($@".\{Properties.Resources.submit_messages_path}");
        protected static readonly string AS4FullOutputPath = Path.GetFullPath($@".\{Properties.Resources.submit_output_path}");
        protected static readonly string AS4ReceiptsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_receipts_path}");
        protected static readonly string AS4ErrorsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_errors_path}");
        protected static readonly string AS4ExceptionsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_exceptions_path}");

        protected readonly string HolodeckBInputPath = Properties.Resources.holodeck_B_input_path;
        protected static readonly string HolodeckMessagesPath = Path.GetFullPath(@".\messages\holodeck-messages");
        public static readonly string AS4FullInputPath = Path.GetFullPath($@".\{Properties.Resources.submit_input_path}");

        private Process _as4ComponentProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTestTemplate"/> class.
        /// </summary>
        public IntegrationTestTemplate()
        {
            Console.WriteLine(Environment.NewLine);

            CopyDirectory(@".\config\integrationtest-settings", @".\config\");
            CopyDirectory(@".\config\integrationtest-pmodes\send-pmodes", @".\config\send-pmodes");
            CopyDirectory(@".\config\integrationtest-pmodes\receive-pmodes", @".\config\receive-pmodes");
            CopyDirectory(@".\messages\integrationtest-messages", @".\messages");

            ReplaceTokensInDirectoryFiles(@".\messages", "__OUTPUTPATH__", Path.GetFullPath("."));
            ReplaceTokensInDirectoryFiles(@".\config\send-pmodes", "__IPADDRESS__", AS4Component.HostAddress);

            LeaveAS4ComponentRunningDuringValidation = false;

        }

        public bool LeaveAS4ComponentRunningDuringValidation { get; set; }
        
        #region Fixture Setup
        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            DirectoryInfo sourceDirectory = GetSourceDirectory(sourceDirName);

            EnsureDestinationDirectory(destDirName);

            CopyFilesFromDestinationToSource(sourceDirectory, destDirName);
        }

        private static DirectoryInfo GetSourceDirectory(string sourceDirName)
        {
            var sourceDirectory = new DirectoryInfo(sourceDirName);

            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            return sourceDirectory;
        }

        private static void EnsureDestinationDirectory(string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
        }

        private static void CopyFilesFromDestinationToSource(DirectoryInfo sourceDirectory, string destDirName)
        {
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite: true);
            }
        }

        private static void ReplaceTokensInDirectoryFiles(string directory, string token, string value)
        {
            foreach (string filePath in Directory.EnumerateFiles(Path.GetFullPath(directory)))
            {
                string oldContents = File.ReadAllText(filePath);
                string newContents = oldContents.Replace(token, value);

                File.WriteAllText(filePath, newContents);
            }
        }

        #endregion

        /// <summary>
        /// Start AS4 Component Application
        /// </summary>
        protected void StartAS4Component()
        {
            _as4ComponentProcess = Process.Start("Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe");

            if (_as4ComponentProcess != null)
            {
                Console.WriteLine($@"Application Started with Process Id: {_as4ComponentProcess.Id}");
            }
        }

        /// <summary>
        /// Cleanup files in a given Directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="predicateFile">The predicate File.</param>
        protected void CleanUpFiles(string directory, Func<string, bool> predicateFile = null)
        {
            EnsureDirectory(directory);

            Console.WriteLine($@"Deleting files at location: {directory}");

            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (predicateFile == null || predicateFile(file))
                {
                    TryDeleteFile(file);
                }
            }
        }

        private static void TryDeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Copy the right PMode configuration to Holodeck B
        /// </summary>
        /// <param name="pmodeFilename"></param>
        protected void CopyPModeToHolodeckB(string pmodeFilename)
        {
            Console.WriteLine($@"Copy PMode {pmodeFilename} to Holodeck B");
            CopyPModeToHolodeck(pmodeFilename, Properties.Resources.holodeck_B_pmodes);
            WaitForHolodeckToPickUp();
        }

        /// <summary>
        /// Copy the right PMode configuration to Holodeck A
        /// </summary>
        /// <param name="pmodeFilename"></param>
        protected void CopyPModeToHolodeckA(string pmodeFilename)
        {
            Console.WriteLine($@"Copy PMode {pmodeFilename} to Holodeck A");
            CopyPModeToHolodeck(pmodeFilename, Properties.Resources.holodeck_A_pmodes);
            WaitForHolodeckToPickUp();
        }

        private static void CopyPModeToHolodeck(string fileName, string directory)
        {
            File.Copy(
                sourceFileName: $".{Properties.Resources.holodeck_test_pmodes}\\{fileName}",
                destFileName: $"{directory}\\{fileName}");
        }

        /// <summary>
        /// Copy the right message to Holodeck B
        /// </summary>
        /// <param name="messageFileName"></param>
        public void CopyMessageToHolodeckB(string messageFileName)
        {
            Console.WriteLine($@"Copy Message {messageFileName} to Holodeck B");

            File.Copy(
                sourceFileName: Path.GetFullPath($@".\messages\holodeck-messages\{messageFileName}"), 
                destFileName: Path.GetFullPath($@"{Properties.Resources.holodeck_B_output_path}\{messageFileName}"));

            WaitForHolodeckToPickUp();
        }

        /// <summary>
        /// Copy the right message to Holodeck A
        /// </summary>
        /// <param name="messageFileName"></param>
        public void CopyMessageToHolodeckA(string messageFileName)
        {
            Console.WriteLine($@"Copy Message {messageFileName} to Holodeck A");

            File.Copy(
                sourceFileName: Path.GetFullPath($@".\messages\holodeck-messages\{messageFileName}"),
                destFileName: Path.GetFullPath($@"{Properties.Resources.holodeck_A_output_path}\{messageFileName}"));

            WaitForHolodeckToPickUp();
        }

        private static void WaitForHolodeckToPickUp()
        {
            Console.WriteLine(@"Wait for Holodeck to pick-up the new PMode");
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Poll to a given target Directory to find files
        /// </summary>
        /// <param name="directoryPath">Directory Path to poll</param>
        /// <param name="extension"></param>
        /// <param name="retryCount">Retry Count in miliseconds</param>
        protected bool PollingAt(string directoryPath, string extension = "*", int retryCount = 2500)
        {
            string location = FindAliasLocation(directoryPath);
            Console.WriteLine($@"Start polling to {location}");

            var i = 0;
            var areFilesFound = false;

            while (i < retryCount)
            {
                areFilesFound = IsMessageFound(directoryPath, extension);
                if (areFilesFound)
                {
                    break;
                }

                Thread.Sleep(2000);
                i += 100;
            }

            StopApplication();
            return areFilesFound;
        }

        private static string FindAliasLocation(string directoryPath)
        {
            if (directoryPath.Contains("holodeck"))
            {
                return "Holodeck";
            }

            if (directoryPath.Contains("messages"))
            {
                return "AS4 Component";
            }

            return directoryPath;
        }

        private bool IsMessageFound(string directoryPath, string extension)
        {
            var startDir = new DirectoryInfo(directoryPath);
            FileInfo[] files = startDir.GetFiles(extension, SearchOption.AllDirectories);
            bool areFilesPresent = files.Length > 0;

            if (!areFilesPresent)
            {
                return false;
            }

            if (!LeaveAS4ComponentRunningDuringValidation)
            {
                StopApplication();
            }

            WriteFilesToConsole(files);
            ValidatePolledFiles(files);

            return true;
        }

        private static void WriteFilesToConsole(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo file in files)
            {
                Console.WriteLine($@"File found at {file.DirectoryName}: {file.Name}");
            }
        }

        private static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected virtual void ValidatePolledFiles(IEnumerable<FileInfo> files) { }

        /// <summary>
        /// Stop the _AS4 Component.
        /// </summary>
        protected void StopApplication()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!_as4ComponentProcess.HasExited)
            {
                _as4ComponentProcess.Kill();
            }

            DisposeChild();
        }

        /// <summary>
        /// Dispose custom resources in subclass implementation.
        /// </summary>
        protected virtual void DisposeChild() {}
    }
}