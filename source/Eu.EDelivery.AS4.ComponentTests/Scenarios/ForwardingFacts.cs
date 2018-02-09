using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Scenarios
{
    public class ForwardingFacts : IDisposable
    {
        private bool _restoreSettings = false;

        private readonly AS4Component _as4Msh;

        private readonly string _receiveAgentUrl;


        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardingFacts"/> class.
        /// </summary>
        public ForwardingFacts()
        {
            AS4Mapper.Initialize();

            string RetrieveReceiveAgentUrl(AS4Component as4Component)
            {
                var receivingAgent =
                    as4Component.GetConfiguration().GetAgentsConfiguration().FirstOrDefault(a => a.Name.Equals("Receive Agent"));

                Assert.True(receivingAgent != null, "The Agent with name Receive Agent could not be found");

                return receivingAgent.Settings.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;
            }

            FileSystemUtils.ClearDirectory(@".\config\send-pmodes");
            FileSystemUtils.ClearDirectory(@".\config\receive-pmodes");

            FileSystemUtils.CopyDirectory(@".\config\scenariotest-settings\send-pmodes", @".\config\send-pmodes");
            FileSystemUtils.CopyDirectory(@".\config\scenariotest-settings\receive-pmodes", @".\config\receive-pmodes");

            OverrideSettings("forwardingscenario_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            _receiveAgentUrl = RetrieveReceiveAgentUrl(_as4Msh);
        }

        protected void OverrideSettings(string settingsFile)
        {
            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);
            File.Copy($@".\config\componenttest-settings\{settingsFile}", @".\config\settings.xml", true);
            _restoreSettings = true;
        }

        [Fact]
        public async Task CanReceiveEncryptedMultihopMessage()
        {
            // It would be better if we could modify this test to have
            // 2 running AS4.NET instances where C1 receives the holodeck message,
            // determines that it is for forwarding, encrypts it and sends it to C2

            var message = await DeserializeToAS4Message(Properties.Resources.signed_holodeck_message,
                                                        @"multipart/related;boundary=""MIMEBoundary_bcb27a6f984295aa9962b01ef2fb3e8d982de76d061ab23f""");

            // Encrypt with a certificate that we have in the store so that it can be decrypted.
            var certificate = new CertificateRepository().GetCertificate(X509FindType.FindBySubjectName, "AccessPointB");

            message = EncryptMessage(message, certificate);

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, message);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // TODO: read as bytes and use method that we already have.
            var responseContent = await response.Content.ReadAsStreamAsync();
            responseContent.Position = 0;


            var responseAS4Message = await SerializerProvider.Default.Get(response.Content.Headers.ContentType.MediaType).DeserializeAsync(responseContent,
                                                                                                                                     response
                                                                                                                                         .Content
                                                                                                                                         .Headers
                                                                                                                                         .ContentType
                                                                                                                                         .MediaType,
                                                                                                                                     CancellationToken
                                                                                                                                         .None);

            Assert.True(responseAS4Message.IsSignalMessage);
            Assert.True(responseAS4Message.PrimarySignalMessage is Receipt);

        }

        private static async Task<AS4Message> DeserializeToAS4Message(byte[] message, string contentType)
        {
            var serializer = SerializerProvider.Default.Get(contentType);

            return await serializer.DeserializeAsync(new MemoryStream(message), contentType, CancellationToken.None);
        }

      //  private static byte[] SerializeAS4Message(AS4Message message) { }

        private static AS4Message EncryptMessage(AS4Message message, X509Certificate2 certificate)
        {
            var encryptionStrategy = EncryptionStrategyBuilder.Create(message, new KeyEncryptionConfiguration(certificate)).Build();

            message.SecurityHeader.Encrypt(encryptionStrategy);

            return message;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _as4Msh?.Dispose();
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }
        }
    }
}
