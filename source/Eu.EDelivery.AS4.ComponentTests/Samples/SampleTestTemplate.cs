using System;
using System.IO;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.TestUtils.FileSystemUtils;

namespace Eu.EDelivery.AS4.ComponentTests.Samples
{
    [Collection(WindowsServiceCollection.CollectionId)]
    public abstract class SampleTestTemplate : IDisposable
    {
        private bool _restoreSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleTestTemplate"/> class.
        /// </summary>
        protected SampleTestTemplate()
        {
            try
            {
                OverrideConsoleSettings("sample_console_settings.xml");
                OverrideServiceSettings("sample_service_settings.xml");

                // ReSharper disable once InconsistentNaming
                string samples_pmodes = Path.Combine(".", "samples", "pmodes");
                CleanSlateReceivingPModesFrom(samples_pmodes);
                CleanSlateSendingPModesFrom(samples_pmodes);

                CreateOrClearDirectory(@".\messages\out");
                CreateOrClearDirectory(@".\messages\in");
                CreateOrClearDirectory(@".\messages\receipts");
                CreateOrClearDirectory(@".\messages\errors");
                CreateOrClearDirectory(@".\messages\exceptions");

                SenderMsh = AS4Component.Start(Environment.CurrentDirectory, cleanSlate: false);
            }
            catch (Exception)
            {
                SenderMsh?.Dispose();
                throw;
            }
        }

        private static void CleanSlateReceivingPModesFrom(string pmodesPath)
        {
            CreateOrClearDirectory(@".\config\receive-pmodes");

            foreach (string file in Directory.EnumerateFiles(pmodesPath, "*receive-pmode.xml"))
            {
                File.Copy(file, @".\config\receive-pmodes\" + Path.GetFileName(file), overwrite: true);
            }
        }

        private static void CleanSlateSendingPModesFrom(string pmodesPath)
        {
            CreateOrClearDirectory(@".\config\send-pmodes");

            File.Copy(Path.Combine(pmodesPath, "signed-response-pmode.xml"), @".\config\send-pmodes\signed-response-pmode.xml", overwrite: true);
            File.Copy(Path.Combine(pmodesPath, "unsigned-response-pmode.xml"), @".\config\send-pmodes\unsigned-response-pmode.xml", overwrite: true);

            foreach (string file in Directory.EnumerateFiles(pmodesPath, "*send-pmode.xml"))
            {
                File.Copy(file, @".\config\send-pmodes\" + Path.GetFileName(file), overwrite: true);
            }
        }

        private void OverrideConsoleSettings(string settingsFile)
        {
            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);
            File.Copy($@".\config\componenttest-settings\{settingsFile}", @".\config\settings.xml", true);
            _restoreSettings = true;
        }

        private void OverrideServiceSettings(string settingsFile)
        {
            File.Copy(@".\config\settings-service.xml", @".\config\settings_original.xml", true);
            File.Copy($@".\config\componenttest-settings\{settingsFile}", @".\config\settings-service.xml", true);
            _restoreSettings = true;
        }

        /// <summary>
        /// Gets the MSH.
        /// </summary>
        /// <value>The MSH.</value>
        protected AS4Component SenderMsh { get; }

        /// <summary>
        /// Puts the sample to the polling directory so it can be picked up by the AS4.NET Component.
        /// </summary>
        /// <param name="sampleName">Name of the sample.</param>
        protected void PutSample(string sampleName)
        {
            Console.WriteLine($@"Put the sample file: {sampleName} to be picked up");
            File.Copy(@".\samples\messages\" + sampleName, @".\messages\out\" + sampleName, overwrite: true);
        }

        public void Dispose()
        {
            Disposing(true);
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }

            AS4Component.WriteLogFilesToConsole();
        }

        protected virtual void Disposing(bool isDisposing)
        {
            SenderMsh.Dispose();
        }
    }
}
