using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._14_Send_Pull_Request_result_in_User_Message
{
    /// <summary>
    /// Testing if the AS4 Component correctly sends a Pull Request to another party.
    /// </summary>
    public class SendPullRequestResultInUserMessageTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenSendingPullRequestSucceeds()
        {
            // Setup
            CleanUpFiles(HolodeckBInputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_output_path);
            
            TryDeleteFile("settings-temp.xml");
            TryDeleteFile("8.1.14-settings.xml");

            TryMoveFile("settings.xml", "settings-temp.xml");
            TryMoveFile("settings-8.1.14.xml", "settings.xml");

            CopyPModeToHolodeckB("8.1.14-pmode.xml");
            CopyMessageToHolodeckB("8.1.14-sample.mmd");

            // Act
            StartAS4Component();

            //// Assert
            bool areFilesFound = PollingAt(AS4FullInputPath);
            Assert.True(areFilesFound);
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayload(files.FirstOrDefault(f => f.Extension.Equals(".jpg")));
            AssertReceipt(files.FirstOrDefault(f => f.Extension.Equals(".xml")));
        }

        private void AssertPayload(FileInfo receivedPayload)
        {
            FileInfo sendPayload = AS4Component.SubmitSinglePayloadImage;

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private void AssertReceipt(FileInfo receipt)
        {
            Assert.NotNull(receipt);
        }

        /// <summary>
        /// Dispose custom resources in subclass implementation.
        /// </summary>
        protected override void DisposeChild()
        {
            TryDeleteFile("settings-temp.xml");
            TryDeleteFile("settings.xml");
            TryCopyFile(@"integrationtest-settings\settings.xml", @"settings.xml");
        }

        private void TryCopyFile(string sourceFile, string destFile)
        {
            try
            {
                File.Copy(Path.GetFullPath($@".\config\{sourceFile}"), Path.GetFullPath($@".\config\{destFile}"));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void TryMoveFile(string sourceFile, string destFile)
        {
            try
            {
                File.Move(Path.GetFullPath($@".\config\{sourceFile}"), Path.GetFullPath($@".\config\{destFile}"));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void TryDeleteFile(string fileName)
        {
            try
            {
                File.Delete(Path.GetFullPath($@".\config\{fileName}"));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}