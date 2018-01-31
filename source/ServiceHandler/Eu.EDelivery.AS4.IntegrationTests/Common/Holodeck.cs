using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Exceptions;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Wrapper for Holodeck specific operations
    /// </summary>
    public class Holodeck
    {
        public static readonly HolodeckLocations HolodeckALocations;
        public static readonly HolodeckLocations HolodeckBLocations;

        static Holodeck()
        {
            HolodeckALocations = HolodeckLocations.ProbeForHolodeckInstance("holodeck-b2b-A");
            HolodeckBLocations = HolodeckLocations.ProbeForHolodeckInstance("holodeck-b2b-B");
        }

        private readonly DirectoryInfo _holodeckAInputDirectory =
            new DirectoryInfo(HolodeckALocations.InputPath);

        public FileInfo HolodeckAPayload => new FileInfo(HolodeckALocations.JpegPayloadPath);

        /// <summary>
        /// Copy the right PMode configuration to Holodeck B
        /// </summary>
        /// <param name="pmodeFilename"></param>
        public void CopyPModeToHolodeckB(string pmodeFilename)
        {
            Console.WriteLine($@"Copy PMode {pmodeFilename} to Holodeck B");
            CopyPModeToHolodeck(pmodeFilename, HolodeckBLocations.PModePath);
            WaitForHolodeckToPickUp();
        }

        /// <summary>
        /// Copy the right PMode configuration to Holodeck A
        /// </summary>
        /// <param name="pmodeFilename"></param>
        public void CopyPModeToHolodeckA(string pmodeFilename)
        {
            Console.WriteLine($@"Copy PMode {pmodeFilename} to Holodeck A");
            CopyPModeToHolodeck(pmodeFilename, HolodeckALocations.PModePath);
            WaitForHolodeckToPickUp();
        }

        private static void CopyPModeToHolodeck(string fileName, string directory)
        {
            File.Copy(
                sourceFileName: $".{Properties.Resources.holodeck_test_pmodes}\\{fileName}",
                destFileName: $"{directory}\\{fileName}",
                overwrite: true);
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
                destFileName: Path.GetFullPath($@"{HolodeckBLocations.OutputPath}\{messageFileName}"));

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
                destFileName: Path.GetFullPath($@"{HolodeckALocations.OutputPath}\{messageFileName}"));

            WaitForHolodeckToPickUp();
        }

        private static void WaitForHolodeckToPickUp()
        {
            Console.WriteLine(@"Wait for Holodeck to pick-up the new PMode");
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Assert the image payload on Holodeck
        /// </summary>
        public void AssertDandelionPayloadOnHolodeckA()
        {
            FileInfo receivedPayload = new DirectoryInfo(IntegrationTestTemplate.AS4FullInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(HolodeckALocations.JpegPayloadPath);

            Assert.Equal(sendPayload.Length, receivedPayload?.Length);
        }

        /// <summary>
        /// Asserts the deliver message on holodeck b.
        /// </summary>
        public void AssertDeliverMessageOnHolodeckB()
        {
            FileInfo deliveredMessage =
                new DirectoryInfo(HolodeckBLocations.InputPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(deliveredMessage);
        }

        /// <summary>
        /// Assert the single payload on Holodeck B.
        /// </summary>
        public void AssertSinglePayloadOnHolodeckB()
        {
            FileInfo receivedPayload =
                new DirectoryInfo(HolodeckBLocations.InputPath).GetFiles("*.jpg").FirstOrDefault();
            FileInfo sendPayload = AS4Component.SubmitSinglePayloadImage;

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        /// <summary>
        /// Asserts the error on holodeck a.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public void AssertErrorOnHolodeckA(ErrorCode errorCode = ErrorCode.NotApplicable)
        {
            FileInfo error = _holodeckAInputDirectory.GetFiles("*.xml").FirstOrDefault();
            Assert.NotNull(error);
        }

        /// <summary>
        /// Asserts the receipt on holodeck b.
        /// </summary>
        /// <param name="files">The files.</param>
        public void AssertReceiptOnHolodeckB(IEnumerable<FileInfo> files)
        {
            FileInfo receipt = files.First();
            string xml = File.ReadAllText(receipt.FullName);

            Assert.Contains("Receipt", xml);
        }

        /// <summary>
        /// Assert the received <Receipt /> with Holodeck
        /// </summary>
        public void AssertReceiptOnHolodeckA()
        {
            FileInfo receipt = _holodeckAInputDirectory.GetFiles("*.xml").FirstOrDefault();

            if (receipt != null)
            {
                Console.WriteLine(@"Receipt found at Holodeck A");
            }

            Assert.NotNull(receipt);
        }
    }
}