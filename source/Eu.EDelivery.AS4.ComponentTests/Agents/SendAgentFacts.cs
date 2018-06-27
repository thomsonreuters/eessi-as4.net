using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using Service = Eu.EDelivery.AS4.Model.PMode.Service;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class SendAgentFacts : ComponentTestTemplate
    {
        private const string StubListenLocation = "http://localhost:9997/msh/";

        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        public SendAgentFacts()
        {
            OverrideSettings("sendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
            _databaseSpy.ClearDatabase();
        }

        [Fact]
        public async Task ThenUpdateReceiptWithReceived_IfNRReceiptHasValidHashes()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();

            // Act
            TestReceiveNRReceiptWith(ebmsMessageId, hash => hash);

            // Assert
            InMessage receipt = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == ebmsMessageId),
                timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(InStatus.Received, receipt.Status.ToEnum<InStatus>());
        }

        [Fact]
        public async Task ThenUpdateReceiptWithException_IfNRReceiptHasInvalidHashes()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            int CorruptHash(int hash) => hash + 10;

            // Act
            TestReceiveNRReceiptWith(ebmsMessageId, CorruptHash);

            // Assert
            IEnumerable<InException> inExceptions = await PollUntilPresent(
                () => _databaseSpy.GetInExceptions(m => m.EbmsRefToMessageId == ebmsMessageId),
                timeout: TimeSpan.FromSeconds(5));

            Assert.NotEmpty(inExceptions);
        }

        private void TestReceiveNRReceiptWith(string ebmsMessageId, Func<int, int> selection)
        {
            SendingProcessingMode nrrPMode = VerifyNRReceiptsPMode();
            X509Certificate2 cert = new StubCertificateRepository().GetStubCertificate();

            AS4Message userMessage = SignedUserMessage(ebmsMessageId, nrrPMode, cert);
            AS4Message nrReceipt = SignedNRReceipt(cert, userMessage, selection);

            var waitHandle = new ManualResetEvent(initialState: false);
            StubHttpServer.StartServer(StubListenLocation, new AS4MessageResponseHandler(nrReceipt).WriteResponse, waitHandle);

            PutMessageToSend(userMessage, nrrPMode, actAsIntermediaryMsh: false);
            waitHandle.WaitOne();
        }

        private static SendingProcessingMode VerifyNRReceiptsPMode()
        {
            return new SendingProcessingMode
            {
                Id = "verify-nrr",
                PushConfiguration = new PushConfiguration { Protocol = { Url = StubListenLocation } },
                ReceiptHandling = { VerifyNRR = true }
            };
        }

        private static AS4Message SignedUserMessage(string messageId, SendingProcessingMode nrrPMode, X509Certificate2 cert)
        {
            AS4Message userMessage = AS4Message.Create(new UserMessage(messageId), nrrPMode);
            userMessage.AddAttachment(new Attachment("payload")
            {
                Content = new MemoryStream(Encoding.UTF8.GetBytes("some content!")),
                ContentType = "text/plain"
            });

            return AS4MessageUtils.SignWithCertificate(userMessage, cert);
        }

        private static AS4Message SignedNRReceipt(X509Certificate2 cert, AS4Message signedUserMessage, Func<int, int> selection)
        {
            IEnumerable<MessagePartNRInformation> hashes = new NonRepudiationInformationBuilder()
                .WithSignedReferences(signedUserMessage.SecurityHeader.GetReferences())
                .Build()
                .MessagePartNRInformation.Select(i => new MessagePartNRInformation
                {
                    Reference = new Reference
                    {
                        DigestValue = i.Reference.DigestValue.Select(v => (byte)selection(v)).ToArray(),
                        DigestMethod = i.Reference.DigestMethod,
                        Transforms = i.Reference.Transforms,
                        URI = i.Reference.URI
                    }
                });

            AS4Message receipt = AS4Message.Create(new Receipt
            {
                RefToMessageId = signedUserMessage.GetPrimaryMessageId(),
                NonRepudiationInformation = new NonRepudiationInformation { MessagePartNRInformation = hashes.ToList() }
            });

            return AS4MessageUtils.SignWithCertificate(receipt, cert);
        }

        [Theory]
        [InlineData(false, OutStatus.Ack, Operation.ToBeNotified)]
        [InlineData(true, OutStatus.Sent, Operation.ToBeForwarded)]
        public async Task CorrectHandlingOnSynchronouslyReceivedMultiHopReceipt(
            bool actAsIntermediaryMsh,
            OutStatus expectedOutStatus,
            Operation expectedSignalOperation)
        {
            // Arrange
            string messageId = $"multihop-message-id-{Guid.NewGuid()}";

            SendingProcessingMode pmode = CreateMultihopPMode(StubListenLocation);
            AS4Message as4Message = CreateMultiHopAS4UserMessage(messageId, pmode);
            as4Message.FirstUserMessage.CollaborationInfo = 
                new Model.Core.CollaborationInfo(new Model.Core.AgreementReference("agreement", "Forward_Push"));

            var signal = new ManualResetEvent(false);
            var r = new AS4MessageResponseHandler(CreateMultiHopReceiptFor(as4Message));
            StubHttpServer.StartServer(StubListenLocation, r.WriteResponse, signal);

            // Act
            PutMessageToSend(as4Message, pmode, actAsIntermediaryMsh);

            // Assert
            signal.WaitOne();

            OutMessage sentMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId),
                timeout: TimeSpan.FromSeconds(10));

            InMessage receivedMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == messageId),
                timeout: TimeSpan.FromSeconds(10));

            Assert.NotNull(sentMessage);
            Assert.NotNull(receivedMessage);

            Assert.Equal(expectedOutStatus, sentMessage.Status.ToEnum<OutStatus>());
            Assert.Equal(MessageType.Receipt, receivedMessage.EbmsMessageType);
            Assert.Equal(expectedSignalOperation, receivedMessage.Operation);
        }

        private void PutMessageToSend(AS4Message as4Message, SendingProcessingMode pmode, bool actAsIntermediaryMsh)
        {
            string fileName = @".\database\as4messages\out\sendagent_test.as4";

            string directory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            Console.WriteLine($@"Put AS4Message to {directory}");
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                SerializerProvider.Default.Get(as4Message.ContentType).Serialize(as4Message, fs, CancellationToken.None);
            }

            var outMessage = new OutMessage(as4Message.GetPrimaryMessageId())
            {
                ContentType = as4Message.ContentType,
                MessageLocation = $"FILE:///{fileName}",
                Intermediary = actAsIntermediaryMsh,
            };

            outMessage.EbmsMessageType = MessageType.UserMessage;
            outMessage.MEP = MessageExchangePattern.Push;
            outMessage.Operation = Operation.ToBeSent;
            outMessage.SetPModeInformation(pmode);

            _databaseSpy.InsertOutMessage(outMessage);
        }

        private static AS4Message CreateMultiHopAS4UserMessage(string messageId, SendingProcessingMode sendingPMode)
        {
            var simpleUserMessage = UserMessageFactory.Instance.Create(sendingPMode);
            simpleUserMessage.MessageId = messageId;

            return AS4Message.Create(simpleUserMessage, sendingPMode);
        }

        private static AS4Message CreateMultiHopReceiptFor(AS4Message message)
        {
            using (MessagingContext context = new MessagingContext(message, MessagingContextMode.Receive))
            {
                var createReceipt = new CreateAS4ReceiptStep();
                var result = createReceipt.ExecuteAsync(context).Result;

                Assert.True(result.Succeeded, "Unable to create Receipt");
                Assert.True(result.MessagingContext.AS4Message.IsMultiHopMessage, "Receipt is not created as a multihop receipt");

                return result.MessagingContext.AS4Message;
            }
        }

        private static SendingProcessingMode CreateMultihopPMode(string sendToUrl)
        {
            return new SendingProcessingMode()
            {
                Id = "PMode-Id",
                PushConfiguration = new PushConfiguration()
                {
                    Protocol = new Protocol()
                    {
                        Url = sendToUrl
                    }
                },
                ReceiptHandling = new SendReceiptHandling()
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = new Method { Type = "FILE", Parameters = new List<Parameter> { new Parameter() { Name = "Location", Value = "." } } }
                },
                MepBinding = MessageExchangePatternBinding.Push,
                MessagePackaging = new SendMessagePackaging
                {
                    IsMultiHop = true,
                    PartyInfo = new PartyInfo()
                    {
                        FromParty = new Party()
                        {
                            PartyIds = new List<PartyId> { new PartyId("org:eu:europa:as4:example:accesspoint:B") },
                            Role = "Sender"
                        },
                        ToParty = new Party()
                        {
                            PartyIds = new List<PartyId> { new PartyId("org:eu:europa:as4:example:accesspoint:A") },
                            Role = "Receiver"
                        }
                    },
                    CollaborationInfo = new CollaborationInfo()
                    {
                        Action = "Forward_Push_Action",
                        Service = new Service()
                        {
                            Type = "eu:europa:services",
                            Value = "Forward_Push_Service"
                        }
                    }
                }

            };
        }
    }
}
