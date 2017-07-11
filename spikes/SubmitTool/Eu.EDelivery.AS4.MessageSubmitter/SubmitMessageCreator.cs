using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Submit;

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
                var submitMessage = new SubmitMessage { MessageInfo = { MessageId = $"{Guid.NewGuid().ToString()}@{Environment.MachineName}" } };
                submitMessage.Collaboration.AgreementRef.PModeId = submitInfo.SendingPMode.Id;

                var originalSenderProperty = new MessageProperty("originalSender",
                                                                 submitInfo.SendingPMode.MessagePackaging.PartyInfo.FromParty.PartyIds.First().Id);

                var finalRecipientProperty = new MessageProperty("finalRecipient",
                                                                 submitInfo.SendingPMode.MessagePackaging.PartyInfo.ToParty.PartyIds.First().Id);

                submitMessage.MessageProperties = new[] { originalSenderProperty, finalRecipientProperty };

                var payloads = new List<Payload>();

                foreach (var payloadInfo in submitInfo.PayloadInformation)
                {
                    var messagePayload = new Payload
                    {
                        Id = CreatePayloadId(payloadInfo, submitMessage.MessageInfo.MessageId),
                        Location = $"file:///{payloadInfo.FileName}",
                        MimeType = MimeMapping.GetMimeMapping(Path.GetFileName(payloadInfo.FileName))
                    };

                    if (messagePayload.MimeType.Equals("text/xml", StringComparison.OrdinalIgnoreCase))
                    {
                        messagePayload.MimeType = "application/xml";
                    }

                    if (payloadInfo.IncludeSEDPartType)
                    {
                        messagePayload.PayloadProperties = new[] { new PayloadProperty("PartType") { Value = "SED" }, };
                    }

                    payloads.Add(messagePayload);
                }

                submitMessage.Payloads = payloads.ToArray();

                yield return submitMessage;
            }
        }

        private static void SubmitMessags(IEnumerable<SubmitMessage> submitMessages, string location)
        {
            SubmitToFilesystem(submitMessages, location);
            
            // Commented out since we're not going to support this right now in this tool.
            // Submitting via HTTP means that our payloads should be referred via http aswell.

            ////if (location.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            ////{
            ////    SubmitViaHttp(submitMessages, location);
            ////}
            ////else
            ////{
            ////    SubmitToFilesystem(submitMessages, location);
            ////}                        

        }

        ////private static readonly HttpClient _client = new HttpClient();

        ////private static async void SubmitViaHttp(IEnumerable<SubmitMessage> submitMessages, string location)
        ////{
        ////    var sendTasks = new List<Task<HttpResponseMessage>>();

        ////    foreach (var submitMessage in submitMessages)
        ////    {
        ////        var request = new HttpRequestMessage(HttpMethod.Post, location);
        ////        request.Content = new StringContent(AS4XmlSerializer.ToString(submitMessage));

        ////        sendTasks.Add(_client.SendAsync(request));
        ////    }

        ////    var httpResults = await Task.WhenAll(sendTasks);

        ////    if (httpResults.Any(r => r.StatusCode != HttpStatusCode.Accepted))
        ////    {
        ////        MessageBox.Show("Not all submit-messages have been accepted.");
        ////    }
        ////}

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
