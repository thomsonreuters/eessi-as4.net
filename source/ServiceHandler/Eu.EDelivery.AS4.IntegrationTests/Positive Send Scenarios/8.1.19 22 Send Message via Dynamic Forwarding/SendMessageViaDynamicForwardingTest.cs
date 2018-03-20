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

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._19_22_Send_Message_via_Dynamic_Forwarding
{
    public class SendMessageViaDynamicForwardingTest : IntegrationTestTemplate
    {
        public static readonly string ReceiveAgentEndpoint = "http://localhost:9090/msh",
                                      HolodeckBId = "org:holodeckb2b:example:company:B",
                                      HolodeckPartyRole = "Receiver",
                                      DynamicDiscoverySettings = "8.1.19-settings.xml";

        /// <summary>
        /// 8.1.19 Integration Test with Dynamic Discovery:
        /// Send "simple" message with dynamic discovered minimum information.
        /// </summary>
        [Fact]
        public void HolodeckAcceptsSimpleDynamicForwardedMessage()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.19-pmode.xml");

            AS4Component.OverrideSettings(DynamicDiscoverySettings);
            AS4Component.Start();

            InsertSmpConfigurationForAS4Component(url: ReceiveAgentEndpoint, enableEncryption: false);

            AS4Message userMessage = UserMessageWithAttachment(argRefPModeId: "8.1.19-pmode");

            // Act: send the UserMessage to the AS4.NET Component, so it can be made complete by Dynamic Discover the info from the SMP Configuration
            SendMultiHopAS4Message(userMessage);

            // Assert
            Assert.True(
                PollingAt(AS4ReceiptsPath),
                "No Receipt found at AS4.NET Component for Simple Dynamic Discovery Test");
        }

        /// <summary>
        /// 8.1.20 Integration Test with Dynamic Discovery:
        /// Send message with dynamic discovered encryption information to Holodeck.
        /// </summary>
        [Fact]
        public void HolodeckAcceptsEncryptedDynamicForwardedMessage()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.20-pmode.xml");

            AS4Component.OverrideSettings(DynamicDiscoverySettings);
            AS4Component.Start();

            InsertSmpConfigurationForAS4Component(url: ReceiveAgentEndpoint, enableEncryption: true);

            AS4Message userMessage = UserMessageWithAttachment(argRefPModeId: "8.1.20-pmode");

            // Act: send the UserMessage to the AS4.NET Component, so it can be made complete by Dynamic Discover the info from the SMP Configuration
            SendMultiHopAS4Message(userMessage);

            // Assert
            Assert.True(
                PollingAt(AS4ReceiptsPath),
                "No Receipt found at AS4.NET Component for Encrypted Dynamic Forwarding Test");
        }

        [Fact]
        public void AS4ComponentDoesntAlterSignatureFromOrignalHolodeckMessage()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.21-pmode.xml");

            AS4Component.OverrideSettings(DynamicDiscoverySettings);
            AS4Component.Start();

            InsertEnabledEncryptionForHolodeck(url: ReceiveAgentEndpoint);

            // Act
            new StubSender().SendMessage(Properties.Resources._8_1_21_message, Constants.ContentTypes.Soap);

            // Assert
            Assert.True(
                PollingAt(AS4ReceiptsPath),
                "No Receipt found at AS4.NET Component for Simple Dynamic Discovery Test");
        }

        private void InsertEnabledEncryptionForHolodeck(string url)
        {
            var smpConfig = new SmpConfiguration
            {
                TlsEnabled = false,
                EncryptionEnabled = true,
                PartyRole = HolodeckPartyRole,
                ToPartyId = "org:eu:europa:as4:example",
                PartyType = "org:eu:europa:as4:example",
                Url = url,
                Action = "StoreMessage",
                ServiceType = "org:holodeckb2b:services",
                ServiceValue = "Test",
                FinalRecipient = "org:eu:europa:as4:example",
                EncryptAlgorithm = Encryption.Default.Algorithm,
                EncryptPublicKeyCertificate = Properties.Resources.AccessPointB_PublicCert,
                EncryptAlgorithmKeySize = Encryption.Default.AlgorithmKeySize,
                EncryptKeyDigestAlgorithm = KeyEncryption.Default.DigestAlgorithm,
                EncryptKeyMgfAlorithm = KeyEncryption.Default.MgfAlgorithm,
                EncryptKeyTransportAlgorithm = KeyEncryption.Default.TransportAlgorithm
            };

            InsertSmpConfiguration(smpConfig);
        }

        [Fact]
        public void AS4ComponentDoesntAlterEncryptedDataFromOriginalHolodeckMessage()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.22-pmode.xml");

            AS4Component.OverrideSettings(DynamicDiscoverySettings);
            AS4Component.Start();

            InsertSmpConfigurationForAS4Component(ReceiveAgentEndpoint, enableEncryption: false);

            var str = VirtualStream.CreateVirtualStream();
            str.Write(Properties.Resources._8_1_22_message, 0, Properties.Resources._8_1_22_message.Length);
            str.Position = 0;

            const string contentType =
                "multipart/related; boundary= \"MIMEBoundary_4ac2a25e8a3af891754f9f7316ac08062c50de1368ddfada\"; type=\"application/soap+xml\";";

            // Act
            new StubSender().SendMessage(str, contentType);

            // Assert
            Assert.True(
                PollingAt(AS4ReceiptsPath),
                "No Receipt found at AS4.NET Component for Encrypted Dynamic Forwarding Test");
        }

        [Fact(Skip="Test is not finished yet")]
        public void AS4ComponentDoesntAlterSignedAndEncryptedDataFromOriginalHolodeckMessage()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.22-pmode.xml");

            AS4Component.OverrideSettings(DynamicDiscoverySettings);
            AS4Component.Start();

            InsertSmpConfigurationForAS4Component(ReceiveAgentEndpoint, enableEncryption: false);

            var str = VirtualStream.CreateVirtualStream();
            str.Write(Properties.Resources._8_1_23_message, 0, Properties.Resources._8_1_23_message.Length);
            str.Position = 0;

            const string contentType =
                "multipart/related; boundary= \"MIMEBoundary_941dcf57ed25018c43557aa9d032f4d52727c829935b8988\"; type=\"application/soap+xml\";";

            // Act
            new StubSender().SendMessage(str, contentType);

            // Assert
            Assert.True(
                PollingAt(AS4ReceiptsPath),
                "No Receipt found at AS4.NET Component for Signed and Encrypted Dynamic Forward Test");
        }

        private void InsertSmpConfigurationForAS4Component(string url, bool enableEncryption)
        {
            var smpConfig = new SmpConfiguration
            {
                TlsEnabled = false,
                EncryptionEnabled = enableEncryption,
                PartyRole = HolodeckPartyRole,
                ToPartyId = HolodeckBId,
                PartyType = HolodeckBId,
                Url = url,
                Action = Constants.Namespaces.TestAction,
                ServiceType = Constants.Namespaces.TestService,
                ServiceValue = Constants.Namespaces.TestService,
                FinalRecipient = HolodeckBId,
                EncryptAlgorithm = Encryption.Default.Algorithm,
                EncryptPublicKeyCertificate = Properties.Resources.AccessPointB_PublicCert,
                EncryptAlgorithmKeySize = Encryption.Default.AlgorithmKeySize,
                EncryptKeyDigestAlgorithm = KeyEncryption.Default.DigestAlgorithm,
                EncryptKeyMgfAlorithm = KeyEncryption.Default.MgfAlgorithm,
                EncryptKeyTransportAlgorithm = KeyEncryption.Default.TransportAlgorithm
            };

            InsertSmpConfiguration(smpConfig);
        }

        private void InsertSmpConfiguration(SmpConfiguration smpConfig)
        {
            PollingAt(Path.GetFullPath(@".\database"), "*.db");

            // Wait for migrations to complete on datastore
            Thread.Sleep(TimeSpan.FromSeconds(5));

            var spy = new DatastoreSpy(AS4Component.GetConfiguration());
            spy.InsertSmpConfiguration(smpConfig);
        }

        private static AS4Message UserMessageWithAttachment(string argRefPModeId)
        {
            const string payloadId = "earth";

            AS4Message userMessage = AS4Message.Create(new UserMessage(Guid.NewGuid().ToString())
            {
                CollaborationInfo = HolodeckCollaboration(argRefPModeId),
                Receiver = new Party(HolodeckPartyRole, new PartyId(HolodeckBId) {Type = HolodeckBId}),
                PayloadInfo = {ImagePayload(payloadId)}
            }, new SendingProcessingMode {MessagePackaging = {IsMultiHop = true}});

            userMessage.AddAttachment(ImageAttachment(payloadId));

            return userMessage;
        }

        private static CollaborationInfo HolodeckCollaboration(string argRefPModeId)
        {
            return new CollaborationInfo
            {
                AgreementReference =
                {
                    PModeId = argRefPModeId,
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
                    Path.GetFullPath($@".\{Properties.Resources.submitmessage_single_payload_path}"),
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
