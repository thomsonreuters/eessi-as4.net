using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ForwardOutboundProcessingAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardOutboundProcessingAgentFacts"/> class.
        /// </summary>
        public ForwardOutboundProcessingAgentFacts()
        {
            OverrideSettings("forward_outboundprocessing_agent_settings.xml");
            _msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task Untouched_Forwarded_Message_Has_Still_Valid_Signature()
        {
            // Arrange
            string ebmsMessageId = $"user-{Guid.NewGuid()}";
            IConfig configuration = _msh.GetConfiguration();
            AS4Message tobeForwarded = CreateSignedAS4Message(ebmsMessageId, configuration);

            // Act
            InsertToBeForwardedMessage(
                msh: _msh,
                pmodeId: "Forwarding_Untouched_Push",
                mep: MessageExchangePattern.Push,
                tobeForwarded: tobeForwarded);

            // Assert
            
            var databaseSpy = DatabaseSpy.Create(configuration);
            OutMessage tobeSentRecord = await PollUntilPresent(
                () => databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == ebmsMessageId
                                                         && m.Operation == Operation.ToBeSent),
                timeout: TimeSpan.FromSeconds(20));

            Registry.Instance
                    .MessageBodyStore
                    .SaveAS4Message(
                        configuration.InMessageStoreLocation,
                        tobeForwarded);

            using (Stream tobeSentContents =
                await tobeSentRecord.RetrieveMessageBody(Registry.Instance.MessageBodyStore))
            {
                AS4Message tobeSentMessage =
                    await SerializerProvider
                          .Default
                          .Get(tobeSentRecord.ContentType)
                          .DeserializeAsync(tobeSentContents, tobeSentRecord.ContentType);

                bool validSignature = tobeSentMessage.VerifySignature(
                    new VerifySignatureConfig(
                        allowUnknownRootCertificateAuthority: false,
                        attachments: tobeSentMessage.Attachments));

                Assert.True(validSignature,
                            "Forwarded AS4Message hasn't got a valid signature present while the message was forwaded 'untouched'");
            }
        }

        private static AS4Message CreateSignedAS4Message(string ebmsMessageId, IConfig configuration)
        {
            var userMessage = new UserMessage(
                ebmsMessageId,
                new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "Forward_Push"),
                    service: new Service(
                        value: "Forward_Push_Service",
                        type: "eu:europa:services"),
                    action: "Forward_Push_Action",
                    conversationId: "eu:europe:conversation"));

            AS4Message tobeForwarded = AS4Message.Create(userMessage);

            var certificateRepo = new CertificateRepository(configuration);
            X509Certificate2 signingCertificate =
                certificateRepo.GetCertificate(X509FindType.FindBySubjectName, "AccessPointA");
            tobeForwarded.Sign(new CalculateSignatureConfig(signingCertificate));

            return tobeForwarded;
        }

        private static void InsertToBeForwardedMessage(
            AS4Component msh,
            string pmodeId, 
            MessageExchangePattern mep, 
            AS4Message tobeForwarded)
        {
            foreach (MessageUnit m in tobeForwarded.MessageUnits)
            {
                string location =
                    Registry.Instance
                            .MessageBodyStore
                            .SaveAS4Message(
                                msh.GetConfiguration().InMessageStoreLocation,
                                tobeForwarded);

                Operation operation = 
                    m.MessageId == tobeForwarded.PrimaryMessageUnit.MessageId
                        ? Operation.ToBeForwarded
                        : Operation.NotApplicable;

                var inMessage = new InMessage(m.MessageId)
                {
                    Intermediary = true,
                    Operation = operation,
                    MessageLocation = location,
                    MEP = mep,
                    ContentType = tobeForwarded.ContentType
                };

                ReceivingProcessingMode forwardPMode =
                    msh.GetConfiguration()
                       .GetReceivingPModes()
                       .First(p => p.Id == pmodeId);

                inMessage.SetPModeInformation(forwardPMode);
                inMessage.SetStatus(InStatus.Received);
                inMessage.AssignAS4Properties(m);

                DatabaseSpy.Create(msh.GetConfiguration())
                           .InsertInMessage(inMessage);
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            _msh.Dispose();
        }
    }
}
