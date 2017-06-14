using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly DirectoryInfo _holodeckAInputDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Holodeck"/> class.
        /// </summary>
        public Holodeck()
        {
            _holodeckAInputDirectory = new DirectoryInfo(Properties.Resources.holodeck_A_input_path);
        }

        /// <summary>
        /// Assert the image payload on Holodeck
        /// </summary>
        public void AssertDandelionPayload()
        {
            FileInfo receivedPayload = new DirectoryInfo(IntegrationTestTemplate.AS4FullInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            Assert.Equal(sendPayload.Length, receivedPayload?.Length);
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
        /// Assert if the given <paramref name="receivedPayload" /> matches the 'Earth' payload.
        /// </summary>
        /// <param name="receivedPayload"></param>
        public void AssertEarthPayload(FileInfo receivedPayload)
        {
            var sendPayload = new FileInfo(Path.GetFullPath($".\\{Properties.Resources.submitmessage_single_payload_path}"));

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