using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using NonRepudiationInformation = Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation;
using Parameter = Eu.EDelivery.AS4.Model.PMode.Parameter;
using Service = Eu.EDelivery.AS4.Model.PMode.Service;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;
using PartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;
using Protocol = Eu.EDelivery.AS4.Model.PMode.Protocol;
using PushConfiguration = Eu.EDelivery.AS4.Model.PMode.PushConfiguration;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

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
            userMessage.AddAttachment(
                new Attachment(
                    id: "payload",
                    content: new MemoryStream(Encoding.UTF8.GetBytes("some content!")),
                    contentType: "text/plain"));

            return AS4MessageUtils.SignWithCertificate(userMessage, cert);
        }

        private static AS4Message SignedNRReceipt(X509Certificate2 cert, AS4Message signedUserMessage, Func<int, int> selection)
        {
            IEnumerable<Reference> hashes =
                signedUserMessage
                    .SecurityHeader
                    .GetReferences()
                    .Select(r =>
                    {
                        r.DigestValue = r.DigestValue.Select(v => (byte) selection(v)).ToArray();
                        return Reference.CreateFromReferenceElement(r);
                    });

            AS4Message receipt = AS4Message.Create(
                new Receipt(
                    refToMessageId: signedUserMessage.GetPrimaryMessageId(),
                    nonRepudiation: new NonRepudiationInformation(hashes)));

            return AS4MessageUtils.SignWithCertificate(receipt, cert);
        }

        [Fact(Skip = "Test fails on build server")]
        public Task CorrectHandlingOnSynchronouslyReceiveMulithopReceiptWithForwarding()
        {
            return CorrectHandlingOnSynchronouslyReceivedMultiHopReceipt(
                actAsIntermediaryMsh: false,
                receivePModeId: "ComponentTest_ReceiveAgent_Sample1",
                expectedOutStatus: OutStatus.Ack,
                expectedSignalOperation: Operation.ToBeNotified);
        }

        [Fact(Skip = "Test fails on build server")]
        public Task CorrectHandlingOnSynchronouslyReceiveMulithopReceiptWithNotifing()
        {
            return CorrectHandlingOnSynchronouslyReceivedMultiHopReceipt(
                actAsIntermediaryMsh: false,
                receivePModeId: "Forward_Push",
                expectedOutStatus: OutStatus.Sent,
                expectedSignalOperation: Operation.ToBeForwarded);
        }

        private async Task CorrectHandlingOnSynchronouslyReceivedMultiHopReceipt(
            bool actAsIntermediaryMsh,
            string receivePModeId,
            OutStatus expectedOutStatus,
            Operation expectedSignalOperation)
        {
            // Arrange
            SendingProcessingMode pmode = CreateMultihopPMode(StubListenLocation);
            UserMessage simpleUserMessage = CreateMultihopUserMessage(receivePModeId, pmode);

            AS4Message as4Message = AS4Message.Create(simpleUserMessage, pmode);

            var signal = new ManualResetEvent(false);
            var serializer = new SoapEnvelopeSerializer();
            StubHttpServer.StartServer(
                StubListenLocation,
                res =>
                {
                    res.StatusCode = 200;
                    res.ContentType = Constants.ContentTypes.Soap;
                    AS4Message receipt = CreateMultiHopReceiptFor(as4Message);
                    serializer.Serialize(receipt, res.OutputStream);
                },
                signal);

            // Act
            PutMessageToSend(as4Message, pmode, actAsIntermediaryMsh);

            // Assert
            signal.WaitOne();

            OutMessage sentMessage = await PollUntilPresent(
                () => _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == simpleUserMessage.MessageId),
                timeout: TimeSpan.FromSeconds(10));
            Assert.Equal(expectedOutStatus, sentMessage.Status.ToEnum<OutStatus>());

            InMessage receivedMessage = await PollUntilPresent(
                () => _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == simpleUserMessage.MessageId),
                timeout: TimeSpan.FromSeconds(10));
            Assert.Equal(MessageType.Receipt, receivedMessage.EbmsMessageType);
            Assert.Equal(expectedSignalOperation, receivedMessage.Operation);
        }

        private static UserMessage CreateMultihopUserMessage(string receivePModeId, SendingProcessingMode pmode)
        {
            var collaboration =
                new Model.Core.CollaborationInfo(
                    new Model.Core.AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: receivePModeId),
                    service: new Model.Core.Service(
                        value: "Forward_Push_Service",
                        type: "eu:europa:services"),
                    action: "Forward_Push_Action",
                    conversationId: "eu:europe:conversation");

            IEnumerable<MessageProperty> properties =
                pmode.MessagePackaging?.MessageProperties?.Select(
                    p => new MessageProperty(p.Name, p.Value, p.Type)) ?? new MessageProperty[0];

            return new UserMessage(
                $"multihop-message-id-{Guid.NewGuid()}",
                collaboration,
                PModePartyResolver.ResolveSender(pmode.MessagePackaging?.PartyInfo.FromParty),
                PModePartyResolver.ResolveSender(pmode.MessagePackaging?.PartyInfo.ToParty),
                new Model.Core.PartInfo[0],
                properties);
        }

        private void PutMessageToSend(AS4Message as4Message, SendingProcessingMode pmode, bool actAsIntermediaryMsh)
        {
            var outMessage = new OutMessage(as4Message.GetPrimaryMessageId())
            {
                ContentType = as4Message.ContentType,
                MessageLocation = 
                    Registry.Instance
                            .MessageBodyStore.SaveAS4Message(
                                Config.Instance.OutMessageStoreLocation,
                                as4Message),
                Intermediary = actAsIntermediaryMsh,
                EbmsMessageType = MessageType.UserMessage,
                MEP = MessageExchangePattern.Push,
                Operation = Operation.ToBeSent,
            };
            outMessage.SetPModeInformation(pmode);

            _databaseSpy.InsertOutMessage(outMessage);
        }

        private static AS4Message CreateMultiHopReceiptFor(AS4Message message)
        {
            Model.Core.CollaborationInfo coll = message.FirstUserMessage.CollaborationInfo;
            var receipt = new Receipt(
                message.FirstUserMessage.MessageId,
                message.FirstUserMessage,
                new RoutingInputUserMessage
                {
                    CollaborationInfo = new Xml.CollaborationInfo
                    {
                        Action = coll.Action,
                        Service = new Xml.Service
                        {
                            Value = coll.Service.Value,
                            type = coll.Service.Type.GetOrElse(() => null)
                        },
                        AgreementRef = new Xml.AgreementRef
                        {
                            pmode = coll.AgreementReference.UnsafeGet.PModeId.GetOrElse(() => null),
                            type = coll.AgreementReference.UnsafeGet.Type.GetOrElse(() => null),
                            Value = coll.AgreementReference.UnsafeGet.Value
                        },
                        ConversationId = coll.ConversationId
                    },
                    mpc = message.FirstUserMessage.Mpc,
                    MessageInfo = new Xml.MessageInfo
                    {
                        MessageId = message.FirstUserMessage.MessageId
                    }
                });

            return AS4Message.Create(receipt);
        }

        private static SendingProcessingMode CreateMultihopPMode(string sendToUrl)
        {
            return new SendingProcessingMode
            {
                Id = "PMode-Id",
                PushConfiguration = new PushConfiguration
                {
                    Protocol = new Protocol { Url = sendToUrl }
                },
                ReceiptHandling = new SendReceiptHandling
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = new Method
                    {
                        Type = "FILE",
                        Parameters = new List<Parameter>
                        {
                            new Parameter
                            {
                                Name = "Location",
                                Value = "."
                            }
                        }
                    }
                },
                MepBinding = MessageExchangePatternBinding.Push,
                MessagePackaging = new SendMessagePackaging
                {
                    IsMultiHop = true,
                    PartyInfo = new PartyInfo
                    {
                        FromParty = new Party
                        {
                            PartyIds = new List<PartyId> { new PartyId("org:eu:europa:as4:example:accesspoint:B") },
                            Role = "Sender"
                        },
                        ToParty = new Party
                        {
                            PartyIds = new List<PartyId> { new PartyId("org:eu:europa:as4:example:accesspoint:A") },
                            Role = "Receiver"
                        }
                    },
                    CollaborationInfo = new CollaborationInfo
                    {
                        Action = "Forward_Push_Action",
                        Service = new Service
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

