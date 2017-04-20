using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._16_Receive_Singe_Payload_Via_Payload_Service
{
    /// <summary>
    /// Integration Test with the Payload Service
    /// </summary>
    public class ReceiveSinglePayloadViaPayloadService : IntegrationTestTemplate
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveSinglePayloadViaPayloadService"/> class.
        /// </summary>
        public ReceiveSinglePayloadViaPayloadService()
        {
            LeaveAS4ComponentRunningDuringValidation = true;
        }

        [Fact]
        public void RunIntegrationTest()
        {
            // Before
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            CopyPModeToHolodeckA("8.3.16-pmode.xml");
            CopyMessageToHolodeckA("8.3.16-sample.mmd");

            // Act
            AS4Component.Start();

            // Assert
            PollingAt(AS4FullInputPath);
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            FileInfo deliverMessageFile = GetDeliverMessageFrom(files);
            string uploadAddress = GetDownloadUrlFrom(deliverMessageFile);

            AssertPayloadOnLoaction(uploadAddress);
        }

        private static FileInfo GetDeliverMessageFrom(IEnumerable<FileInfo> files)
        {
            return files.First(f => f.Extension.Equals(".xml"));
        }

        private static string GetDownloadUrlFrom(FileInfo deliverMessageFile)
        {
            string deliverMessage = File.ReadAllText(deliverMessageFile.FullName);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(deliverMessage);

            XmlElement locationElement = xmlDocument["DeliverMessage"]["Payloads"]["Payload"]["Location"];
            return locationElement.InnerText;
        }

        private static void AssertPayloadOnLoaction(string location)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(location));

            Task<HttpResponseMessage> sendRequest = HttpClient.SendAsync(request);
            sendRequest.Wait(timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(HttpStatusCode.OK, sendRequest.Result.StatusCode);
        }
    }
}