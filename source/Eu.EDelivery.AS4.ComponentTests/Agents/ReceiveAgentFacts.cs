using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using MessagePartNRInformation = Eu.EDelivery.AS4.Model.Core.MessagePartNRInformation;
using NonRepudiationInformation = Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation;
using Parameter = Eu.EDelivery.AS4.Model.PMode.Parameter;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class ReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;
        private readonly string _receiveAgentUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveAgentFacts" /> class.
        /// </summary>
        public ReceiveAgentFacts()
        {
            Settings receiveSettings = OverrideSettings("receiveagent_http_settings.xml");

            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());

            _receiveAgentUrl = receiveSettings.Agents.ReceiveAgents.First().Receiver.Setting
                                              .FirstOrDefault(s => s.Key == "Url")
                                              ?.Value;

            Assert.False(
                string.IsNullOrWhiteSpace(_receiveAgentUrl),
                "The URL where the receive agent is listening on, could not be retrieved.");
        }

        #region Scenario where ReceiveAgent receives invalid Messages (no AS4 Messages)

        [Fact]
        public async Task ThenAgentReturnsBadRequest_IfReceivedMessageIsNotAS4Message()
        {
            // Arrange
            byte[] content = Encoding.UTF8.GetBytes(Convert.ToBase64String(receiveagent_message));

            // Act
            HttpResponseMessage response = await StubSender.SendRequest(_receiveAgentUrl, content,
                                                                        "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Scenario's for received UserMessages that result in errors.

        [Fact]
        public async Task ThenAgentReturnsError_IfResponseSendPModeIsNotFound()
        {
            // Arrange
            var message = AS4Message.Create(new UserMessage());
            message.FirstUserMessage.CollaborationInfo = 
                new CollaborationInfo(
                    new AgreementReference(
                        value: "agreement", 
                        pmodeId: "receiveagent-non-exist-response-pmode"));

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, message);

            // Assert
            AS4Message errorMessage = await response.DeserializeToAS4Message();
            var e = errorMessage.PrimaryMessageUnit as Error;
            Assert.True(e != null, "Primary Message Unit should be an 'Error'");
            Assert.Equal(
                ErrorAlias.ProcessingModeMismatch,
                e.Errors.First().ShortDescription.ToEnum<ErrorAlias>());
        }

        [Fact]
        public async Task ThenAgentReturnsError_IfMessageHasNonExsistingAttachment()
        {
            // Arrange
            byte[] content = receiveagent_message_nonexist_attachment;

            // Act
            HttpResponseMessage response = await StubSender.SendRequest(_receiveAgentUrl, content,
                                                                        "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AS4Message as4Message = await response.DeserializeToAS4Message();
            Assert.IsType<Error>(as4Message.PrimaryMessageUnit);
        }

        [Fact]
        public async Task ThenAgentReturnsError_IfReceivingPModeIsNotValid()
        {
            const string messageId = "some-message-id";

            // Arrange
            var message = AS4Message.Create(new UserMessage
            {
                MessageId = messageId,
                Sender =
                {
                    PartyIds = {new PartyId{Id = "org:eu:europa:as4:example:accesspoint:A" } },
                    Role = "Sender"
                },
                Receiver =
                {
                    PartyIds =
                    {
                        new PartyId{ Id = "org:eu:europa:as4:example:accesspoint:B"}
                    },
                    Role = "Receiver"
                },
                CollaborationInfo = new CollaborationInfo(
                    new AgreementReference("http://agreements.europa.org/agreement"),
                    new Model.Core.Service("Invalid_PMode_Test_Service", "eu:europa:services"),
                    "Invalid_PMode_Test_Action",
                    CollaborationInfo.DefaultConversationId)
            });

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            AS4Message result = await SerializerProvider.Default
                .Get(Constants.ContentTypes.Soap)
                .DeserializeAsync(await response.Content.ReadAsStreamAsync(), Constants.ContentTypes.Soap, CancellationToken.None);

            var errorMsg = result.FirstSignalMessage as Error;
            Assert.NotNull(errorMsg);
            Assert.Collection(
                errorMsg.Errors, 
                e => Assert.Equal($"EBMS:{(int)ErrorCode.Ebms0010:0000}", e.ErrorCode));

            InMessage inMessageRecord = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            Assert.Equal(InStatus.Received, inMessageRecord.Status.ToEnum<InStatus>());
        }

        [Fact]
        public async Task ReturnsErrorMessageWhenDecryptionCertificateCannotBeFound()
        {
            var userMessage = new UserMessage();
            userMessage.CollaborationInfo = 
                new CollaborationInfo(new AgreementReference("agreement", "type", "receiveagent-non_existing_decrypt_cert-pmode"));

            var as4Message = CreateAS4MessageWithAttachment(userMessage);

            var encryptedMessage = AS4MessageUtils.EncryptWithCertificate(as4Message, new StubCertificateRepository().GetStubCertificate());

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, encryptedMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contentType = response.Content.Headers.ContentType.MediaType;
            var result = await SerializerProvider.Default.Get(contentType)
                                        .DeserializeAsync(await response.Content.ReadAsStreamAsync(), contentType, CancellationToken.None);

            Assert.True(result.IsSignalMessage);

            var errorMessage = result.FirstSignalMessage as Error;
            Assert.NotNull(errorMessage);
            Assert.Equal("EBMS:0102", errorMessage.Errors.First().ErrorCode);
        }

        private static AS4Message CreateAS4MessageWithAttachment(UserMessage msg)
        {
            var as4Message = AS4Message.Create(msg);

            // Arrange
            byte[] attachmentContents = Encoding.UTF8.GetBytes("some random attachment");
            var attachment = new Attachment("attachment-id") { Content = new MemoryStream(attachmentContents) };

            as4Message.AddAttachment(attachment);

            return as4Message;
        }

        #endregion

        #region MessageHandling scenarios

        [Fact]
        public async Task ThenInMessageOperationIsToBeDelivered()
        {
            // Arrange
            byte[] content = receiveagent_message;

            // Act
            HttpResponseMessage response = await StubSender.SendRequest(_receiveAgentUrl, content,
                                                                        "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            AS4Message receivedAS4Message = await response.DeserializeToAS4Message();
            Assert.IsType<Receipt>(receivedAS4Message.PrimaryMessageUnit);

            InMessage receivedUserMessage = GetInsertedUserMessageFor(receivedAS4Message);
            Assert.NotNull(receivedUserMessage);
            Assert.Equal(Operation.ToBeDelivered, receivedUserMessage.Operation.ToEnum<Operation>());
        }

        private InMessage GetInsertedUserMessageFor(AS4Message receivedAS4Message)
        {
            return
                _databaseSpy.GetInMessageFor(
                    i => i.EbmsMessageId.Equals(receivedAS4Message.FirstSignalMessage.RefToMessageId));
        }

        [Fact]
        public async Task ThenInMessageOperationIsToBeForwarded()
        {
            const string messageId = "forwarding_message_id";

            var as4Message = AS4Message.Create(new UserMessage
            {
                MessageId = messageId,
                CollaborationInfo = new CollaborationInfo(
                    new AgreementReference(
                        value: "forwarding/agreement",
                        type: "forwarding",
                        // Make sure that the forwarding receiving pmode is used; therefore
                        // explicitly set the Id of the PMode that must be used by the receive-agent.
                        pModeId: "Forward_Push"))
            });

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.True(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));

            InMessage receivedUserMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            Assert.NotNull(receivedUserMessage);
            Assert.Equal(Operation.ToBeForwarded, receivedUserMessage.Operation.ToEnum<Operation>());
        }

        [Fact]
        public async Task ReturnsEmptyMessageFromInvalidMessage_IfReceivePModeIsCallback()
        {
            // Act
            HttpResponseMessage response = await StubSender.SendRequest(_receiveAgentUrl, receiveagent_wrong_encrypted_message,
                                                                        "multipart/related; boundary=\"=-WoWSZIFF06iwFV8PHCZ0dg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsStringAsync());
        }

        #endregion

        #region SignalMessage receive scenario's

        [Fact]
        public async Task ThenRelatedUserMessageIsAcked()
        {
            // Arrange
            const string expectedId = "message-id";
            CreateExistingOutMessage(expectedId, CreateSendingPMode());

            AS4Message as4Message = CreateAS4ReceiptMessage(expectedId);

            // Act
            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.True(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()), "An empty response was expected");

            AssertIfStatusOfOutMessageIs(expectedId, OutStatus.Ack);
            AssertIfInMessageExistsForSignalMessage(expectedId);
        }

        private static AS4Message CreateAS4ReceiptMessage(string refToMessageId)
        {
            var r = new Receipt { RefToMessageId = refToMessageId };

            return AS4Message.Create(r, CreateSendingPMode());
        }

        [Fact]
        public async Task ThenRelatedUserMessageIsNotAcked()
        {
            // Arrange
            const string expectedId = "message-id";

            CreateExistingOutMessage(expectedId, CreateSendingPMode());

            AS4Message as4Message = CreateAS4ErrorMessage(expectedId);

            // Act
            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.True(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()), "An empty response was expected");

            AssertIfStatusOfOutMessageIs(expectedId, OutStatus.Nack);
            AssertIfInMessageExistsForSignalMessage(expectedId);
        }
       
        [Fact]
        public async Task ThenResponseWithAccepted_IfNRReceiptHasValidHashes()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();

            // Act
            HttpResponseMessage response = await TestSendNRReceiptWith(ebmsMessageId, hash => hash);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task ThenResponseWithError_IfNRReceiptHasInvalidHashes()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            int CorruptHash(int hash) => hash + 10;

            // Act
            HttpResponseMessage response = await TestSendNRReceiptWith(ebmsMessageId, CorruptHash);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            InMessage insertedReceipt = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == ebmsMessageId);
            Assert.Equal(InStatus.Exception, insertedReceipt.Status.ToEnum<InStatus>());
            Assert.NotEmpty(_databaseSpy.GetInExceptions(m => m.EbmsRefToMessageId == insertedReceipt.EbmsMessageId));
        }

        private async Task<HttpResponseMessage> TestSendNRReceiptWith(string messageId, Func<int, int> selection)
        {
            // Arrange
            var nrrPMode = new SendingProcessingMode {Id = "verify-nrr", ReceiptHandling = {VerifyNRR = true}};
            X509Certificate2 cert = new StubCertificateRepository().GetStubCertificate();

            AS4Message signedUserMessage = SignedUserMessage(messageId, nrrPMode, cert);
            InsertRelatedSignedUserMessage(nrrPMode, signedUserMessage);

            AS4Message signedReceipt = SignedNRReceipt(cert, signedUserMessage, selection);

            // Act
            return await StubSender.SendAS4Message(_receiveAgentUrl, signedReceipt);

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

        private void InsertRelatedSignedUserMessage(IPMode nrrPMode, AS4Message signedUserMessage)
        {
            string location = Registry.Instance.MessageBodyStore
                .SaveAS4Message(_as4Msh.GetConfiguration().OutMessageStoreLocation, signedUserMessage);

            var outMessage = new OutMessage(signedUserMessage.GetPrimaryMessageId())
            {
                ContentType = signedUserMessage.ContentType,
                MessageLocation = location,

            };
            outMessage.SetPModeInformation(nrrPMode);

            _databaseSpy.InsertOutMessage(outMessage);
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
                        DigestValue = i.Reference.DigestValue.Select(v => (byte) selection(v)).ToArray(),
                        DigestMethod = i.Reference.DigestMethod,
                        Transforms = i.Reference.Transforms,
                        URI = i.Reference.URI
                    }
                });

            AS4Message receipt = AS4Message.Create(new Receipt
            {
                RefToMessageId = signedUserMessage.GetPrimaryMessageId(),
                NonRepudiationInformation = new NonRepudiationInformation {MessagePartNRInformation = hashes.ToList()}
            });

            return AS4MessageUtils.SignWithCertificate(receipt, cert);
        }

        [Fact]
        public async Task OnInvalidReceipt_ExceptionIsLogged()
        {
            string userMessageId = Guid.NewGuid().ToString();

            var receiptString = Encoding.UTF8.GetString(receipt_with_invalid_signature).Replace("{{RefToMessageId}}", userMessageId);

            CreateExistingOutMessage(userMessageId, CreateSendingPMode());

            var response = await StubSender.SendRequest(_receiveAgentUrl, Encoding.UTF8.GetBytes(receiptString), "application/soap+xml");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);

            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == userMessageId);
            Assert.NotNull(inMessage);
            Assert.Equal(MessageType.Receipt, inMessage.EbmsMessageType.ToEnum<MessageType>());
            Assert.Equal(InStatus.Exception, inMessage.Status.ToEnum<InStatus>());

            var inExceptions = _databaseSpy.GetInExceptions(m => m.EbmsRefToMessageId == inMessage.EbmsMessageId);
            Assert.NotNull(inExceptions);
            Assert.NotEmpty(inExceptions);

            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsRefToMessageId == userMessageId);
            Assert.True(outMessage == null, "No OutMessage should be created for the received SignalMessage");
        }

        [Fact]
        public async Task ThenReceivedMultihopUserMessageIsSetAsIntermediaryAndForwarded()
        {
            // Arrange
            var userMessage = new UserMessage("test-" + Guid.NewGuid())
            {
                CollaborationInfo = new CollaborationInfo(new AgreementReference("agreement", "Forward_Push_Multihop"))
            };
            var multihopPMode = new SendingProcessingMode {MessagePackaging = {IsMultiHop = true}};
            AS4Message multihopMessage = AS4Message.Create(userMessage, multihopPMode);

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, multihopMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            InMessage inUserMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == userMessage.MessageId);

            Assert.NotNull(inUserMessage);
            Assert.True(inUserMessage.Intermediary);
            Assert.Equal(Operation.ToBeForwarded, inUserMessage.Operation.ToEnum<Operation>());
        }

        [Fact]
        public async Task ThenReceivedMultihopUserMessageIsntSetToIntermediaryButDeliveredWithCorrespondingSentReceipt()
        {
            // Arrange
            var userMessage = new UserMessage("test-" + Guid.NewGuid())
            {
                CollaborationInfo = new CollaborationInfo(new AgreementReference("agreement", "ComponentTest_ReceiveAgent_Sample1"))
            };
            var multihopPMode = new SendingProcessingMode {MessagePackaging = {IsMultiHop = true}};
            AS4Message multihopMessage = AS4Message.Create(userMessage, multihopPMode);

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, multihopMessage);

            // Assert
            AS4Message responseReceipt = await response.DeserializeToAS4Message();
            AssertMessageMultihopAttributes(responseReceipt.EnvelopeDocument);
            Assert.True(responseReceipt.IsMultiHopMessage);

            InMessage inUserMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == userMessage.MessageId);

            Assert.NotNull(inUserMessage);
            Assert.False(inUserMessage.Intermediary);
            Assert.Equal(Operation.ToBeDelivered, inUserMessage.Operation.ToEnum<Operation>());

            OutMessage outReceipt = _databaseSpy.GetOutMessageFor(m => m.EbmsRefToMessageId == userMessage.MessageId);
            Assert.Equal(OutStatus.Sent, outReceipt.Status.ToEnum<OutStatus>());
        }

        private static void AssertMessageMultihopAttributes(XmlDocument doc)
        {
            var messagingNode = doc.SelectSingleNode("//*[local-name()='Messaging']") as XmlElement;

            Assert.NotNull(messagingNode);
            Assert.Equal(Constants.Namespaces.EbmsNextMsh, messagingNode.GetAttribute("role", Constants.Namespaces.Soap12));
            Assert.True(XmlConvert.ToBoolean(messagingNode.GetAttribute("mustUnderstand", Constants.Namespaces.Soap12)));
        }

        [Fact]
        public async Task ThenReceivedNonMultihopSignalMessageWithoutRelatedUserMessageIsSetToException()
        {
            const string messageId = "message-id";

            AS4Message as4Message = CreateAS4ErrorMessage(messageId);

            // Act
            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            // Assert
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(InStatus.Exception, inMessage.Status.ToEnum<InStatus>());

            var inException = _databaseSpy.GetInExceptions(e => e.EbmsRefToMessageId == inMessage.EbmsMessageId);
            Assert.NotNull(inException);
        }

        [Fact]
        public async Task ThenMultiHopSignalMessageIsToBeForwarded()
        {
            // Arrange
            const string messageId = "multihop-signalmessage-id";
            AS4Message as4Message = CreateMultihopSignalMessage(messageId, "someusermessageid");

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            InMessage inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.True(inMessage.Intermediary);
            Assert.Equal(Operation.ToBeForwarded, inMessage.Operation.ToEnum<Operation>());

            Stream messageBody = await Registry.Instance
                .MessageBodyStore
                .LoadMessageBodyAsync(inMessage.MessageLocation);

            AS4Message savedMessage = await SerializerProvider.Default
                .Get(inMessage.ContentType)
                .DeserializeAsync(messageBody, inMessage.ContentType, CancellationToken.None);

            Assert.NotNull(savedMessage.EnvelopeDocument.SelectSingleNode("//*[local-name()='RoutingInput']"));
        }

        [Fact]
        public async Task ThenMultiHopSignalMessageThatHasReachedItsDestinationIsNotified()
        {
            const string messageId = "some-user-message-id";

            var sendingPMode = new SendingProcessingMode()
            {
                ReceiptHandling = new SendReceiptHandling()
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = new Method()
                    {
                        Type = "FILE",
                        Parameters = new List<Parameter>() { new Parameter { Name = "Location", Value = @".\messages\receipts" } }
                    }
                }
            };

            CreateExistingOutMessage(messageId, sendingPMode);

            var as4Message = CreateMultihopSignalMessage("multihop-signalmessage-id", messageId);

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == messageId);
            Assert.NotNull(inMessage);
            Assert.Equal(Operation.ToBeNotified, inMessage.Operation.ToEnum<Operation>());

            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);
            Assert.NotNull(outMessage);
            Assert.Equal(OutStatus.Ack, outMessage.Status.ToEnum<OutStatus>());
        }

        [Fact]
        public async Task CanReceiveErrorSignalWithoutRefToMessageId()
        {
            var errorMessage = new ErrorBuilder().WithErrorResult(new ErrorResult("An Error occurred", ErrorAlias.NonApplicable)).Build();
            var as4Message = AS4Message.Create(errorMessage);

            string id = as4Message.GetPrimaryMessageId();

            var response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.True(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));

            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == id);
            Assert.NotNull(inMessage);
            Assert.Equal(Operation.NotApplicable, inMessage.Operation.ToEnum<Operation>());
        }

        private static AS4Message CreateMultihopSignalMessage(string messageId, string refToMessageId)
        {
            var receipt = new Receipt(messageId, refToMessageId);

            receipt.MultiHopRouting = new RoutingInputUserMessage()
            {
                mpc = "some-mpc",
                PartyInfo = new Xml.PartyInfo()
                {
                    To = new To()
                    {
                        PartyId = new[]
                        {
                            new Xml.PartyId()
                            {
                                Value = "org:eu:europa:as4:example:accesspoint:B"
                            },
                        },
                        Role = "Receiver"
                    },
                    From = new From()
                    {
                        PartyId = new[]
                        {
                            new Xml.PartyId()
                            {
                                Value = "org:eu:europa:as4:example:accesspoint:A",
                            }
                        },
                        Role = "Sender"
                    }
                },
                CollaborationInfo = new Xml.CollaborationInfo()
                {
                    AgreementRef = new Xml.AgreementRef { pmode = "Forward_Push" },
                    Action = "Forward_Push_Action",
                    Service = new Xml.Service()
                    {
                        Value = "Forward_Push_Service",
                        type = "eu:europa:services"
                    }
                }
            };

            return AS4Message.Create(receipt);
        }

        private void CreateExistingOutMessage(string messageId, SendingProcessingMode sendingPMode)
        {
            var outMessage = new OutMessage(messageId);

            outMessage.SetStatus(OutStatus.Sent);
            outMessage.SetPModeInformation(sendingPMode);

            _databaseSpy.InsertOutMessage(outMessage);
        }

        #endregion

        private static SendingProcessingMode CreateSendingPMode()
        {
            return new SendingProcessingMode
            {
                Id = "receive_agent_facts_pmode",
                ReceiptHandling = { NotifyMessageProducer = true },
                ErrorHandling = { NotifyMessageProducer = true }
            };
        }

        private static AS4Message CreateAS4ErrorMessage(string refToMessageId)
        {
            var result = new ErrorResult("An error occurred", ErrorAlias.NonApplicable);
            Error error = new ErrorBuilder().WithRefToEbmsMessageId(refToMessageId).WithErrorResult(result).Build();

            return AS4Message.Create(error, CreateSendingPMode());
        }

        private void AssertIfInMessageExistsForSignalMessage(string expectedId)
        {
            InMessage inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == expectedId);
            Assert.NotNull(inMessage);
            Assert.Equal(InStatus.Received, inMessage.Status.ToEnum<InStatus>());
            Assert.Equal(Operation.ToBeNotified, inMessage.Operation.ToEnum<Operation>());
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertIfStatusOfOutMessageIs(string expectedId, OutStatus expectedStatus)
        {
            OutMessage outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == expectedId);

            Assert.NotNull(outMessage);
            Assert.Equal(expectedStatus, outMessage.Status.ToEnum<OutStatus>());
        }

        // TODO:
        // - Create a test that verifies if the Status for a received receipt/error is set to
        // --> ToBeNotified when the receipt is valid

        // - Create a test that verifies if the Status for a received UserMessage is set to
        // - Exception when the UserMessage is not valid (an InException should be present).
        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }
    }
}