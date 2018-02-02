using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._19_Send_Message_via_Dynamic_Discovery
{
    public class SendMessageViaDynamicDiscoveryTest : IntegrationTestTemplate
    {
        private const string ReceiveAgentEndpoint = "http://localhost:9090/msh",
                             HolodeckBId = "org:holodeckb2b:example:company:B",
                             HolodeckPartyRole = "Receiver";

        [Fact]
        public void HolodeckAcceptsDynamicDiscoveryMessage()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.19-pmode.xml");

            AS4Component.OverrideSettings("8.1.19-settings.xml");
            AS4Component.Start();

            InsertSmpConfigurationWith(url: ReceiveAgentEndpoint);

            AS4Message userMessage = UserMessageWithAttachment();

            // Act: send the UserMessage to the AS4.NET Component, so it can be made complete by Dynamic Discover the info from the SMP Configuration
            SendMultiHopAS4Message(userMessage);

            // Assert
            Assert.True(PollingAt(AS4ReceiptsPath), "No Receipt found at AS4.NET Component for Dynamic Discovery Test");
        }

        private void InsertSmpConfigurationWith(string url)
        {
            var smpConfig = new SmpConfiguration
            {
                TlsEnabled = false,
                EncryptionEnabled = false,
                PartyRole = HolodeckPartyRole,
                ToPartyId = HolodeckBId,
                PartyType = HolodeckBId,
                Url = url,
                Action = Constants.Namespaces.TestAction,
                ServiceType = Constants.Namespaces.TestService,
                ServiceValue = Constants.Namespaces.TestService,
                FinalRecipient = HolodeckBId
            };

            PollingAt(Path.GetFullPath(@".\database"), "*.db");

            var spy = new DatastoreSpy(AS4Component.GetConfiguration());
            spy.InsertSmpConfiguration(smpConfig);
        }

        private static AS4Message UserMessageWithAttachment()
        {
            const string payloadId = "earth";

            AS4Message userMessage = AS4Message.Create(new UserMessage(Guid.NewGuid().ToString())
            {
                CollaborationInfo = HolodeckCollaboration(),
                Receiver = new Party(HolodeckPartyRole, new PartyId(HolodeckBId) { Type = HolodeckBId }),
                PayloadInfo = { ImagePayload(id: payloadId) }
            }, new SendingProcessingMode { MessagePackaging = { IsMultiHop = true } });

            userMessage.AddAttachment(ImageAttachment(id: payloadId));

            return userMessage;
        }

        private static CollaborationInfo HolodeckCollaboration()
        {
            return new CollaborationInfo
            {
                AgreementReference =
                {
                    PModeId = "8.1.19-pmode",
                    Value = "http://agreements.holodeckb2b.org/examples/agreement0"
                },
                ConversationId = "eu:edelivery:as4:sampleconversation",
                Action = Constants.Namespaces.TestAction,
                Service =
                {
                    Type = Constants.Namespaces.TestService,
                    Value = Constants.Namespaces.TestService
                }
            };
        }

        private static PartInfo ImagePayload(string id)
        {
            return new PartInfo("cid:" + id)
            {
                Properties = new Dictionary<string, string>
                {
                    ["Part Property"] = "Some Holodeck required Part Property"
                }
            };
        }

        private static Attachment ImageAttachment(string id)
        {
            return new Attachment(id)
            {
                ContentType = "image/jpg",
                Content = new FileStream(
                    Path.GetFullPath($@".\{submitmessage_single_payload_path}"), 
                    FileMode.Open,
                    FileAccess.Read, 
                    FileShare.Read)
            };
        }

        private static void SendMultiHopAS4Message(AS4Message userMessage)
        {
            ISerializer serializer = SerializerProvider.Default.Get(userMessage.ContentType);
            VirtualStream virtualStr = VirtualStream.CreateVirtualStream();
            serializer.Serialize(userMessage, virtualStr, CancellationToken.None);
            virtualStr.Position = 0;

            new StubSender().SendMessage(virtualStr, userMessage.ContentType);
        }
    }
}
