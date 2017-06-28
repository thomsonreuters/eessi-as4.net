using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._8_Send_message_to_incorrectly_configured_R_MSH
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SendMessageToIncorrectlyConfiguredRMSHIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.8-sample.xml";
        private readonly string _as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";
        private readonly string _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        
        [Fact]
        public void ThenSendMessageFailes()
        {
            // Before
            AS4Component.Start();

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            Assert.True(AreErrorFilesFound());
        }

        private bool AreErrorFilesFound()
        {
            const int milisecondsRetryCount = 3000;
            return PollingAt(AS4ErrorsPath, "*.xml", milisecondsRetryCount);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            FileInfo notifyErrorFile = files.FirstOrDefault(f => f.Extension.Equals(".xml"));
            Assert.NotNull(notifyErrorFile);

            Console.WriteLine($@"Notify Error Message found at: {notifyErrorFile.FullName}");
            XmlDocument xmlDocument = TryGetXmlDocument(notifyErrorFile);
            Assert.NotNull(xmlDocument);

            AssertNotifyDescription(xmlDocument);
            AssertNotifyStatus(xmlDocument);
        }

        private static XmlDocument TryGetXmlDocument(FileSystemInfo notifyErrorFile)
        {
            var xmlDocument = new XmlDocument();
            string xml = File.ReadAllText(notifyErrorFile.FullName);
            xmlDocument.LoadXml(xml);

            return xmlDocument;
        }

        private static void AssertNotifyStatus(XmlNode xmlDocument)
        {
            XmlNode statusNode = xmlDocument.SelectSingleNode("//*[local-name()='Status']");
            Assert.NotNull(statusNode);
            Assert.Equal("Error", statusNode.InnerText);

            Console.WriteLine($@"Notify Error Message Status = {statusNode.InnerText}");
        }

        private static void AssertNotifyDescription(XmlNode xmlDocument)
        {
            XmlNode errorDetailNode = xmlDocument.SelectSingleNode("//*[local-name()='ErrorDetail']");
            Assert.NotNull(errorDetailNode);
            
            Console.WriteLine($@"Notify Error Message Error Detail: {errorDetailNode.InnerText}");
        }
    }
}