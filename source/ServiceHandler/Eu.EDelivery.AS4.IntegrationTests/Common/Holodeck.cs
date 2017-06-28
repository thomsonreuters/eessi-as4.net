using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Wrapper for Holodeck specific operations
    /// </summary>
    public class Holodeck
    {
        private readonly DirectoryInfo _holodeckAInputDirectory =
            new DirectoryInfo(Properties.Resources.holodeck_A_input_path);

        /// <summary>
        /// Copy the right PMode configuration to Holodeck B
        /// </summary>
        /// <param name="pmodeFilename"></param>
        public void CopyPModeToHolodeckB(string pmodeFilename)
        {
            Console.WriteLine($@"Copy PMode {pmodeFilename} to Holodeck B");
            CopyPModeToHolodeck(pmodeFilename, Properties.Resources.holodeck_B_pmodes);
            WaitForHolodeckToPickUp();
        }

        /// <summary>
        /// Copy the right PMode configuration to Holodeck A
        /// </summary>
        /// <param name="pmodeFilename"></param>
        public void CopyPModeToHolodeckA(string pmodeFilename)
        {
            Console.WriteLine($@"Copy PMode {pmodeFilename} to Holodeck A");
            CopyPModeToHolodeck(pmodeFilename, Properties.Resources.holodeck_A_pmodes);
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
        /// Assert the image payload on Holodeck
        /// </summary>
        public void AssertDandelionPayloadOnHolodeckA()
        {
            FileInfo receivedPayload = new DirectoryInfo(IntegrationTestTemplate.AS4FullInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            Assert.Equal(sendPayload.Length, receivedPayload?.Length);
        }

        /// <summary>
        /// Asserts the deliver message on holodeck b.
        /// </summary>
        public void AssertDeliverMessageOnHolodeckB()
        {
            FileInfo deliveredMessage =
                new DirectoryInfo(Properties.Resources.holodeck_B_input_path).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(deliveredMessage);
        }

        /// <summary>
        /// Assert the single payload on Holodeck B.
        /// </summary>
        public void AssertSinglePayloadOnHolodeckB()
        {
            FileInfo receivedPayload =
                new DirectoryInfo(Properties.Resources.holodeck_B_input_path).GetFiles("*.jpg").FirstOrDefault();
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
        /// Asserts the payloads on holodeck a.
        /// </summary>
        /// <param name="files">The files.</param>
        public void AssertPayloadsOnHolodeckA(IEnumerable<FileInfo> files)
        {
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            Assert.All(files, f => Assert.Equal(sendPayload.Length, f.Length));
        }

        /// <summary>
        /// Asserts the XML files on holodeck a.
        /// </summary>
        /// <param name="files">The files.</param>
        public void AssertXmlFilesOnHolodeckA(IEnumerable<FileInfo> files)
        {
            Assert.Equal(2, files.Count());
            Console.WriteLine($@"There're {files.Count()} incoming Xml Documents found");
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

        private void AssertError(ErrorCode errorCode, FileInfo error)
        {
            XmlNode errorTag = SelectErrorTag(error);
            if (errorTag != null)
            {
                Console.WriteLine(@"Error found at Holodeck A");
            }

            Assert.NotNull(errorTag?.Attributes);

            if (errorCode == ErrorCode.NotApplicable)
            {
                return;
            }

            AssertErrorCode(errorCode, errorTag);
        }

        private static void AssertErrorCode(ErrorCode errorCode, XmlNode errorTag)
        {
            string errorCodeString = $"Ebms:{(int) errorCode:0000}";
            XmlAttribute errorCodeAttribute = errorTag.Attributes["errorCode"];

            Assert.Equal(errorCodeString, errorCodeAttribute.InnerText);
        }

        private static XmlNode SelectErrorTag(FileInfo error)
        {
            var xmlDocument = new XmlDocument();
            using (FileStream filesStream = error.Open(FileMode.Open, FileAccess.Read))
            {
                xmlDocument.Load(filesStream);
                XmlNode errorTag = xmlDocument.SelectSingleNode("//*[local-name()='Error']");

                return errorTag;
            }
        }
    }
}