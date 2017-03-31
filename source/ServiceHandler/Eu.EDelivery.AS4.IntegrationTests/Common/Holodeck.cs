using System;
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
        private readonly DirectoryInfo _holodeckDirectory;

        public Holodeck()
        {
            this._holodeckDirectory = new DirectoryInfo(Properties.Resources.holodeck_A_input_path);
        }

        /// <summary>
        /// Assert the image payload on Holodeck
        /// </summary>
        public void AssertImagePayload()
        {
            FileInfo receivedPayload = new DirectoryInfo(IntegrationTestTemplate.AS4FullInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            if (receivedPayload != null) Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        /// <summary>
        /// Assert the received <Receipt/> with Holodeck
        /// </summary>
        public void AssertReceiptOnHolodeckA()
        {
            FileInfo receipt = this._holodeckDirectory.GetFiles("*.xml").FirstOrDefault();

            if (receipt != null) Console.WriteLine(@"Receipt found at Holodeck A");
            Assert.NotNull(receipt);
        }

        public void AssertErrorOnHolodeckA(ErrorCode errorCode = ErrorCode.NotApplicable)
        {
            FileInfo error = this._holodeckDirectory.GetFiles("*.xml").FirstOrDefault();
            Assert.NotNull(error);
            //AssertError(errorCode, error);
        }

        private void AssertError(ErrorCode errorCode, FileInfo error)
        {
            XmlNode errorTag = SelectErrorTag(error);
            if (errorTag != null) Console.WriteLine(@"Error found at Holodeck A");
            Assert.NotNull(errorTag?.Attributes);

            if (errorCode == ErrorCode.NotApplicable) return;
            AssertErrorCode(errorCode, errorTag);
        }

        private void AssertErrorCode(ErrorCode errorCode, XmlNode errorTag)
        {
            string errorCodeString = $"Ebms:{(int)errorCode:0000}";
            XmlAttribute errorCodeAttribute = errorTag.Attributes["errorCode"];

            Assert.Equal(errorCodeString, errorCodeAttribute.InnerText);
        }

        private XmlNode SelectErrorTag(FileInfo error)
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
