using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.MessageSubmitter
{
    internal static class SubmitMessageCreator
    {
        public static void CreateSubmitMessages(SubmitMessageViewModel submitInfo)
        {
            var submitMessages = CreateSubmitMessageObjects(submitInfo);

            SubmitMessags(submitMessages, submitInfo.SubmitLocation);
        }

        private static IEnumerable<SubmitMessage> CreateSubmitMessageObjects(SubmitMessageViewModel submitInfo)
        {
            string CreatePayloadId(PayloadInfoViewModel payloadInfo, string messageId)
            {
                string name = Path.GetFileNameWithoutExtension(payloadInfo.FileName);

                if (submitInfo.NumberOfSubmitMessages > 1)
                {
                    return $"{messageId}.{name}";
                }

                return name;
            }

            for (int i = 0; i < submitInfo.NumberOfSubmitMessages; i++)
            {
                var submitMessage = new SubmitMessage { MessageInfo = { MessageId = Guid.NewGuid().ToString() } };
                submitMessage.Collaboration.AgreementRef.PModeId = submitInfo.SendingProcessingModeName;

                var payloads = new List<Payload>();

                foreach (var payloadInfo in submitInfo.PayloadInformation)
                {
                    payloads.Add(new Payload
                    {
                        Id = CreatePayloadId(payloadInfo, submitMessage.MessageInfo.MessageId),
                        Location = $"file:///{payloadInfo.FileName}",
                        MimeType = MimeMapping.GetMimeMapping(Path.GetFileName(payloadInfo.FileName))
                    });
                }

                submitMessage.Payloads = payloads.ToArray();

                yield return submitMessage;
            }
        }

        private static void SubmitMessags(IEnumerable<SubmitMessage> submitMessages, string location)
        {
            if (location.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                SubmitViaHttp(submitMessages, location);
            }
            else
            {
                SubmitToFilesystem(submitMessages, location);
            }                        
        }

        private static readonly HttpClient _client = new HttpClient();

        private static async void SubmitViaHttp(IEnumerable<SubmitMessage> submitMessages, string location)
        {
            var sendTasks = new List<Task<HttpResponseMessage>>();

            foreach (var submitMessage in submitMessages)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, location);
                request.Content = new StringContent(AS4XmlSerializer.ToString(submitMessage));

                sendTasks.Add(_client.SendAsync(request));
            }

            var httpResults = await Task.WhenAll(sendTasks);

            if (httpResults.Any(r => r.StatusCode != HttpStatusCode.Accepted))
            {
                MessageBox.Show("Not all submit-messages have been accepted.");
            }
        }

        private static void SubmitToFilesystem(IEnumerable<SubmitMessage> submitMessages, string location)
        {
            var serializer = new XmlSerializer(typeof(SubmitMessage));

            foreach (var submitMessage in submitMessages)
            {
                using (var fs = File.Create(Path.Combine(location, $"{submitMessage.MessageInfo.MessageId}.xml")))
                {
                    serializer.Serialize(fs, submitMessage);
                }
            }
        }
    }
}
