using System;
using System.IO;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.TestUtils;

namespace Eu.EDelivery.AS4.ComponentTests.Samples
{
    public abstract class SampleTestTemplate : IDisposable
    {
        private bool _restoreSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleTestTemplate"/> class.
        /// </summary>
        protected SampleTestTemplate()
        {
            OverrideSettings("sample_settings.xml");

            // ReSharper disable once InconsistentNaming
            string samples_pmodes = Path.Combine(".", "samples", "pmodes");
            CleanSlateReceivingPModesFrom(samples_pmodes);
            CleanSlateSendingPModesFrom(samples_pmodes);

            FileSystemUtils.CreateOrClearDirectory(@".\messages\out");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\in");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\receipts");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\errors");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\exceptions");


            Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        private static void CleanSlateReceivingPModesFrom(string pmodesPath)
        {
            FileSystemUtils.CreateOrClearDirectory(@".\config\receive-pmodes");

            foreach (string file in Directory.EnumerateFiles(pmodesPath, "*receive-pmode.xml"))
            {
                File.Copy(file, @".\config\receive-pmodes\" + Path.GetFileName(file), overwrite: true);
            }
        }

        private static void CleanSlateSendingPModesFrom(string pmodesPath)
        {
            FileSystemUtils.CreateOrClearDirectory(@".\config\send-pmodes");

            File.Copy(Path.Combine(pmodesPath, "signed-response-pmode.xml"), @".\config\send-pmodes\signed-response-pmode.xml", overwrite: true);
            File.Copy(Path.Combine(pmodesPath, "unsigned-response-pmode.xml"), @".\config\send-pmodes\unsigned-response-pmode.xml", overwrite: true);

            foreach (string file in Directory.EnumerateFiles(pmodesPath, "*send-pmode.xml"))
            {
                File.Copy(file, @".\config\send-pmodes\" + Path.GetFileName(file), overwrite: true);
            }
        }

        private void OverrideSettings(string settingsFile)
        {
            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);
            File.Copy($@".\config\componenttest-settings\{settingsFile}", @".\config\settings.xml", true);
            _restoreSettings = true;
        }

        /// <summary>
        /// Gets the MSH.
        /// </summary>
        /// <value>The MSH.</value>
        protected AS4Component Msh { get; }

        /// <summary>
        /// Puts the sample to the polling directory so it can be picked up by the AS4.NET Component.
        /// </summary>
        /// <param name="sampleName">Name of the sample.</param>
        protected void PutSample(string sampleName)
        {
            File.Copy(@".\samples\messages\" + sampleName, @".\messages\out\" + sampleName, overwrite: true);
        }

        public void Dispose()
        {
            Disposing(true);
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }
        }

        protected void Disposing(bool isDisposing)
        {
            Msh.Dispose();
        }
    }
}
