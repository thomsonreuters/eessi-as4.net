using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// Facade for the AS4 Component as a Corner.
    /// </summary>
    public class Corner : IDisposable
    {
        private readonly Process _cornerProcess;
        private readonly DirectoryInfo _cornerDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Corner"/> class.
        /// </summary>
        /// <param name="cornerDirectory"></param>
        /// <param name="cornerProcess"></param>
        private Corner(DirectoryInfo cornerDirectory, Process cornerProcess)
        {
            _cornerProcess = cornerProcess;
            _cornerDirectory = cornerDirectory;
        }

        /// <summary>
        /// Place a given <paramref name="message"/> at the Corner's location to retrieve files.
        /// </summary>
        /// <param name="message">Message that the corner must retrieve.</param>
        public void PlaceMessageAtCorner(string message)
        {
            string generatedMessage = message.Replace("__ATTACHMENTID__", Guid.NewGuid().ToString());
            string outMessagePath = Path.Combine(_cornerDirectory.FullName, @"message\out\message.xml");

            File.WriteAllText(outMessagePath, generatedMessage);
        }

        /// <summary>
        /// Count the files that are delivered on the created corner.
        /// </summary>
        /// <param name="searchPattern">
        /// The search string to match against the names of files. This parameter can contain a combination of valid literal path
        /// and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. The default pattern is "*",
        /// which returns all files.
        /// </param>
        /// <returns></returns>
        public int CountDeliveredFiles(string searchPattern)
        {
            string deliverPath = Path.Combine(_cornerDirectory.FullName, "messages", "in");
            var deliverDirectory = new DirectoryInfo(deliverPath);

            return deliverDirectory.GetFiles(searchPattern).Length;
        }

        /// <summary>
        /// Create a new instance of the <see cref="Corner"/> instance.
        /// </summary>
        /// <param name="prefix">Corner Prefix</param>
        /// <returns></returns>
        public static Corner StartNew(string prefix)
        {
            DirectoryInfo cornerDirectory = SetupCornerFixture(prefix);

            var cornerInfo =
                new ProcessStartInfo(
                    Path.Combine(cornerDirectory.FullName, "Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe"));
            cornerInfo.WorkingDirectory = cornerDirectory.FullName;
           
            var corner = new Corner(cornerDirectory, Process.Start(cornerInfo));
            Thread.Sleep(1000);

            return corner;
        }

        private static DirectoryInfo SetupCornerFixture(string cornerPrefix)
        {
            var outputDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo cornerDirectory = CreateCornerIn(outputDirectory, $"output-{cornerPrefix}");

            CopyDirectory(outputDirectory, cornerDirectory.FullName);
            IncludeCornerSettingsIn(cornerDirectory, $"{cornerPrefix}-settings.xml");
            IncludeCornerPModesIn(cornerDirectory);

            CleanUpDirectory(Path.Combine(cornerDirectory.FullName, "database"));

            return cornerDirectory;
        }

        private static DirectoryInfo CreateCornerIn(FileSystemInfo outputDirectory, string corner)
        {
            return Directory.CreateDirectory(outputDirectory.FullName.Replace("output", corner));
        }

        private static void CopyDirectory(DirectoryInfo sourceDirectory, string destDirName)
        {
            CopyFiles(sourceDirectory, destDirName);
            CopySubDirectories(sourceDirectory, destDirName);
        }

        private static void CopyFiles(DirectoryInfo sourceDirectory, string destDirName)
        {
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);

                if (!Directory.Exists(temppath))
                {
                    Directory.CreateDirectory(destDirName);
                }

                file.CopyTo(temppath, overwrite: true);
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
            FileInfo cornerSettings =
                cornerDirectory.GetFiles("*.xml", SearchOption.AllDirectories)
                               .First(f => f.Name.Equals(cornerSettingsFileName));

            File.Copy(cornerSettings.FullName, Path.Combine(cornerDirectory.FullName, "config", "settings.xml"), overwrite: true);
        }

        private static void IncludeCornerPModesIn(FileSystemInfo cornerDirectory)
        {
            Func<string, DirectoryInfo> getPModeDirectory =
                subFolder => new DirectoryInfo(Path.Combine(cornerDirectory.FullName, "config", subFolder));

            DirectoryInfo volumeSendPModes = getPModeDirectory(@"volumetest-pmodes\send-pmodes"),
                          volumeReceivePModes = getPModeDirectory(@"volumetest-pmodes\receive-pmodes");

            DirectoryInfo outputSendPModes = getPModeDirectory(@"send-pmodes"),
                          outputReceivePModes = getPModeDirectory(@"receive-pmodes");

            CopyFiles(volumeSendPModes, outputSendPModes.FullName);
            CopyFiles(volumeReceivePModes, outputReceivePModes.FullName);
        }

        private static void CleanUpDirectory(string directoryPath)
        {
            Console.WriteLine($@"Deleting files at location: {directoryPath}");

            Directory.Delete(directoryPath, recursive: true);
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
