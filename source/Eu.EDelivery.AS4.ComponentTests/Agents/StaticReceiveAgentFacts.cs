using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class StaticReceiveAgentFacts : ComponentTestTemplate
    {
        private const string StaticReceiveSettings = "staticreceiveagent_http_settings.xml";
        private const string DefaultPModeId = "static-receive-pmode";

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticReceiveAgentFacts"/> class.
        /// </summary>
        public StaticReceiveAgentFacts()
        {
            OverrideTransformerReceivingPModeSetting(
                StaticReceiveSettings, 
                DefaultPModeId);
        }

        [Fact]
        public async Task Agent_Returns_BadRequest_When_Receiving_SignalMessage()
        {
            await TestStaticReceive(
                StaticReceiveSettings,
                async (url, _) =>
                {
                    // Arrange
                    AS4Message receipt = AS4Message.Create(
                        new Receipt($"ebms-id-receipt-{Guid.NewGuid()}", $"reftoid-{Guid.NewGuid()}"));

                    // Act
                    HttpResponseMessage response =
                        await StubSender.SendAS4Message(url, receipt);

                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                });
        }

        [Fact]
        public async Task Agent_Processes_Signed_Encrypted_UserMessage_With_Static_ReceivingPMode()
        {
            await TestStaticReceive(
                StaticReceiveSettings,
                async (url, msh) =>
                {
                    // Arrange
                    string ebmsMessageId = $"user-{Guid.NewGuid()}";
                    AS4Message m = SignedEncryptedAS4UserMessage(msh, ebmsMessageId);

                    // Act
                    HttpResponseMessage response =
                        await StubSender.SendAS4Message(url, m);

                    // Assert
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    var spy = new DatabaseSpy(msh.GetConfiguration());
                    InMessage actual = await PollUntilPresent(
                        () => spy.GetInMessageFor(im => im.EbmsMessageId == ebmsMessageId),
                        timeout: TimeSpan.FromSeconds(5));

                    Assert.Equal(Operation.ToBeDelivered, actual.Operation.ToEnum<Operation>());
                    Assert.Equal(InStatus.Received, actual.Status.ToEnum<InStatus>());
                    Assert.Equal(DefaultPModeId, actual.PModeId);
                });
        }

        private static AS4Message SignedEncryptedAS4UserMessage(AS4Component msh, string ebmsMessageId)
        {
            string attachmentId = "attachment-" + Guid.NewGuid();

            var user = new UserMessage(ebmsMessageId)
            {
                CollaborationInfo = new CollaborationInfo(new AgreementReference(String.Empty, DefaultPModeId))
            };
            user.AddPartInfo(new PartInfo("cid:" + attachmentId));

            AS4Message m = AS4Message.Create(user);

            m.AddAttachment(
                    new Attachment(attachmentId)
                    {
                        ContentType = "image/jpg",
                        Content = new MemoryStream(Properties.Resources.payload)
                    });

            var certRepo = new CertificateRepository(msh.GetConfiguration());

            X509Certificate2 signingCert = certRepo.GetCertificate(X509FindType.FindBySubjectName, "AccessPointA");
            m.Sign(new CalculateSignatureConfig(signingCert));

            X509Certificate2 encryptCert = certRepo.GetCertificate(X509FindType.FindBySubjectName, "AccessPointB");
            m.Encrypt(new KeyEncryptionConfiguration(encryptCert), DataEncryptionConfiguration.Default);

            return m;
        }

        [Fact]
        public async Task Agent_Returns_Error_When_ReceivingPMode_Cannot_Be_Found()
        {
            OverrideTransformerReceivingPModeSetting(
                StaticReceiveSettings, 
                pmodeId: "non-existing-pmode-id");

            await TestStaticReceive(
                StaticReceiveSettings,
                async (url, _) =>
                {
                    AS4Message userMessage = AS4Message.Create(new UserMessage("user-" + Guid.NewGuid()));

                    // Act
                    HttpResponseMessage response =
                        await StubSender.SendAS4Message(url, userMessage);

                    // Assert
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    AS4Message error = await response.DeserializeToAS4Message();
                    Assert.Collection(
                        error.MessageUnits,
                        m =>
                        {
                            Assert.IsType<Error>(m);
                            var e = (Error) m;

                            Assert.Equal(
                                ErrorAlias.ProcessingModeMismatch.ToString(),
                                e.Errors.First().ShortDescription);
                        });
                });

        }

        private async Task TestStaticReceive(string settingsFileName, Func<string, AS4Component, Task> act)
        {
            AS4Component msh = null;
            try
            {
                Settings receiveSettings = OverrideSettings(settingsFileName);
                string url = receiveSettings
                    .Agents
                    .ReceiveAgents.First().Receiver
                    .Setting.First(s => s.Key == "Url").Value;

                msh = AS4Component.Start(Environment.CurrentDirectory);

                await act(url, msh);
            }
            finally
            {
                // TearDown
                msh?.Dispose();
            }
        }

        private void OverrideTransformerReceivingPModeSetting(string settingsFileName, string pmodeId)
        {
            string settingsFilePath = Path.Combine(ComponentTestSettingsPath, settingsFileName);
            var settings = AS4XmlSerializer.FromString<Settings>(File.ReadAllText(settingsFilePath));

            settings.Agents
                    .ReceiveAgents.First()
                    .Transformer
                    .Setting.First(s => s.Key == "ReceivingPMode")
                    .Value = pmodeId;

            File.WriteAllText(settingsFilePath, AS4XmlSerializer.ToString(settings));
        }
    }
}
