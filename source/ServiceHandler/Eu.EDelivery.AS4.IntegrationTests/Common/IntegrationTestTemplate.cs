using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Integration Test Class to Perform common Tasks
    /// </summary>
    public class IntegrationTestTemplate : IDisposable
    {
        protected static readonly string OutputPrefix = @"..\..\..\..\..\output";
        protected static readonly string AS4MessagesPath = $@".\{Properties.Resources.submit_messages_path}";
        protected static readonly string AS4FullOutputPath = Path.GetFullPath($@".\{Properties.Resources.submit_output_path}");
        protected static readonly string AS4ReceiptsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_receipts_path}");
        protected static readonly string AS4ErrorsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_errors_path}");
        protected static readonly string AS4ExceptionsPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_exceptions_path}");

        protected readonly string HolodeckBInputPath = Properties.Resources.holodeck_B_input_path;
        protected static readonly string HolodeckMessagesPath = AS4MessagesPath + "\\holodeck-messages";
        private Process _process;

        public IntegrationTestTemplate()
        {
            Console.WriteLine(Environment.NewLine);
        }

        public static readonly string AS4FullInputPath = Path.GetFullPath($@".\{Properties.Resources.submit_input_path}");

        /// <summary>
        /// Start AS4 Component Application
        /// </summary>
        protected void StartApplication()
        {
            string applicationExePath = OutputPrefix + @"\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe";

            this._process = Process.Start(applicationExePath);
            Console.WriteLine($"Application Started with Process Id: {this._process.Id}");

        }

        /// <summary>
        /// Cleanup files in a given Directory
        /// </summary>
        /// <param name="directory"></param>
        protected void CleanUpFiles(string directory)
        {
            Console.WriteLine($@"Deleting files at location: {directory}");

            foreach (string file in Directory.EnumerateFiles(directory))
                TryDeleteFile(file);
        }

        private void TryDeleteFile(string file)
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

        private void CopyPModeToHolodeck(string fileName, string directory)
        {
            File.Copy(
                sourceFileName: $".{Properties.Resources.holodeck_test_pmodes}\\{fileName}",
                destFileName: $"{directory}\\{fileName}");
        }

        private void WaitForHolodeckToPickUp()
        {
            Console.WriteLine(@"Wait for Holodeck to pick-up the new PMode");
            Thread.Sleep(6000);
        }

        /// <summary>
        /// Poll to a given target Directory to find files
        /// </summary>
        /// <param name="directoryPath">Directory Path to poll</param>
        /// <param name="extension"></param>
        /// <param name="retryCount">Retry Count in miliseconds</param>
        protected bool PollTo(string directoryPath, string extension = "*", int retryCount = 1000)
        {
            string location = FindAliasLocation(directoryPath);
            Console.WriteLine($@"Start polling to {location}");

            var i = 0;
            var areFilesFound = false;

            while (i < retryCount)
            {
                areFilesFound = IsMessageFound(directoryPath, extension);
                if (areFilesFound) break;
                Thread.Sleep(2000);
                i += 10;
            }

            StopApplication();
            return areFilesFound;
        }

        private string FindAliasLocation(string directoryPath)
        {
            if (directoryPath.Contains("holodeck")) return "Holodeck";
            if (directoryPath.Contains("messages")) return "AS4 Component";

            return directoryPath;
        }

        private bool IsMessageFound(string directoryPath, string extension)
        {
            var startDir = new DirectoryInfo(directoryPath);
            FileInfo[] files = startDir.GetFiles(extension, SearchOption.AllDirectories);
            bool areFilesPresent = files.Length > 0;
            if (!areFilesPresent) return false;

            StopApplication();
            WriteFilesToConsole(files);
            ValidatePolledFiles(files);

            return true;
        }

        private void WriteFilesToConsole(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo file in files)
                Console.WriteLine($@"File found at {file.DirectoryName}: {file.Name}");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        protected virtual void ValidatePolledFiles(IEnumerable<FileInfo> files) { }

        protected void StopApplication()
        {
            if (!this._process.HasExited)
                this._process.Kill();
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!this._process.HasExited)
                this._process.Kill();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Dispose();
        }
    }
}