using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.ComponentTests.Extensions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
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
            string RetrieveReceiveAgentUrl(AS4Component as4Component)
            {
                AgentSettings receivingAgent =
                    as4Component.GetConfiguration().GetSettingsAgents().FirstOrDefault(a => a.Name.Equals("Receive Agent"));

                Assert.True(receivingAgent != null, "The Agent with name Receive Agent could not be found");

                return receivingAgent.Receiver?.Setting?.FirstOrDefault(s => s.Key == "Url")?.Value;
            }

            OverrideSettings("receiveagent_http_settings.xml");

            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);

            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());

            _receiveAgentUrl = RetrieveReceiveAgentUrl(_as4Msh);

            Assert.False(
                string.IsNullOrWhiteSpace(_receiveAgentUrl),
                "The URL where the receive agent is listening on, could not be retrieved.");
        }

        #region Scenario's for received UserMessages that result in errors.

        [Fact]
        public async Task ThenAgentReturnsError_IfMessageHasNonExsistingAttachment()
        {
            // Arrange
            byte[] content = receiveagent_message_nonexist_attachment;

            // Act
            HttpResponseMessage response = await StubSender.SendRequest(_receiveAgentUrl, content,
                                                                        "multipart/related; boundary=\"=-C3oBZDXCy4W2LpjPUhC4rw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"");

            // Assert
            AS4Message as4Message = await response.DeserializeToAS4Message();
            Assert.True(as4Message.IsSignalMessage);
            Assert.True(as4Message.PrimarySignalMessage is Error);
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
                CollaborationInfo =
                {
                    AgreementReference =
                    {
                        Value = "http://agreements.europa.org/agreement"
                    },
                    Action = "Invalid_PMode_Test_Action",
                    Service =
                    {
                        Type = "eu:europa:services",
                        Value = "Invalid_PMode_Test_Service"
                    }
                }
            });

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, message);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var inMessageRecord = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            var inExceptionRecord = _databaseSpy.GetInExceptions(e => e.EbmsRefToMessageId == messageId).FirstOrDefault();

            Assert.Equal(InStatus.Exception, inMessageRecord.Status);
            Assert.NotNull(inExceptionRecord);
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
            Assert.True(receivedAS4Message.IsSignalMessage);
            Assert.True(receivedAS4Message.PrimarySignalMessage is Receipt);

            InMessage receivedUserMessage = GetInsertedUserMessageFor(receivedAS4Message);
            Assert.NotNull(receivedUserMessage);
            Assert.Equal(Operation.ToBeDelivered, receivedUserMessage.Operation);
        }

        [Fact]
        public async Task ThenInMessageOperationIsToBeForwarded()
        {
            const string messageId = "forwarding_message_id";

            var as4Message = AS4Message.Create(new UserMessage
            {
                MessageId = messageId,
                CollaborationInfo = { AgreementReference = new AgreementReference()
                {
                    // Make sure that the forwarding receiving pmode is used; therefore
                    // explicitly set the Id of the PMode that must be used by the receive-agent.
                    PModeId = "Forward_Push"
                }}
            });

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.True(String.IsNullOrWhiteSpace(await response.Content.ReadAsStringAsync()));

            InMessage receivedUserMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            Assert.NotNull(receivedUserMessage);
            Assert.Equal(Operation.ToBeForwarded, receivedUserMessage.Operation);
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
      
        private InMessage GetInsertedUserMessageFor(AS4Message receivedAS4Message)
        {
            return
                _databaseSpy.GetInMessageFor(
                    i => i.EbmsMessageId.Equals(receivedAS4Message.PrimarySignalMessage.RefToMessageId));
        }

        #endregion

        #region SignalMessage receive scenario's

        [Fact]
        public async Task ThenRelatedUserMessageIsAcked()
        {
            // Arrange
            const string expectedId = "message-id";
            await CreateExistingOutMessage(expectedId);

            AS4Message as4Message = CreateAS4ReceiptMessage(expectedId);

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            AssertIfStatusOfOutMessageIs(expectedId, OutStatus.Ack);
            AssertIfInMessageExistsForSignalMessage(expectedId);
        }

        private static AS4Message CreateAS4ReceiptMessage(string refToMessageId)
        {
            var r = new Receipt { RefToMessageId = refToMessageId };

            return AS4Message.Create(r, GetSendingPMode());
        }

        [Fact]
        public async Task ThenRelatedUserMessageIsNotAcked()
        {
            // Arrange
            const string expectedId = "message-id";
            await CreateExistingOutMessage(expectedId);

            AS4Message as4Message = CreateAS4ErrorMessage(expectedId);

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            AssertIfStatusOfOutMessageIs(expectedId, OutStatus.Nack);
            AssertIfInMessageExistsForSignalMessage(expectedId);
        }

        [Fact]
        public async Task ThenReceivedNonMultihopSignalMessageWithoutRelatedUserMessageIsSetToException()
        {
            const string messageId = "message-id";

            AS4Message as4Message = CreateAS4ErrorMessage(messageId);

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert
            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == messageId);

            Assert.NotNull(inMessage);
            Assert.Equal(InStatus.Exception, inMessage.Status);

            var inException = _databaseSpy.GetInExceptions(e => e.EbmsRefToMessageId == inMessage.EbmsMessageId);
            Assert.NotNull(inException);
        }

        [Fact]
        public async Task ThenMultiHopSignalMessageIsToBeForwarded()
        {
            const string messageId = "multihop-signalmessage-id";
            var as4Message = CreateMultihopSignalMessage(messageId, "someusermessageid");

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert

            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsMessageId == messageId);
            Assert.NotNull(inMessage);
            Assert.Equal(Operation.ToBeForwarded, inMessage.Operation);
        }

        [Fact]
        public async Task ThenMultiHopSignalMessageThatHasReachedItsDestinationIsNotified()
        {
            const string messageId = "some-user-message-id";

            var sendingPMode = new SendingProcessingMode()
            {
                ReceiptHandling = new SendHandling()
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = new Method()
                    {
                        Type = "FILE",
                        Parameters = new List<Parameter>() { new Parameter { Name = "Location", Value = @".\messages\receipts" } }
                    }
                }
            };

            _databaseSpy.InsertOutMessage(new OutMessage()
            {
                EbmsMessageId = messageId,
                Operation = Operation.Sent,
                Status = OutStatus.Sent,
                PMode = AS4XmlSerializer.ToString(sendingPMode)
            });

            var as4Message = CreateMultihopSignalMessage("multihop-signalmessage-id", messageId);

            // Act
            await StubSender.SendAS4Message(_receiveAgentUrl, as4Message);

            // Assert

            var inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == messageId);
            Assert.NotNull(inMessage);
            Assert.Equal(Operation.ToBeNotified, inMessage.Operation);

            var outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);
            Assert.NotNull(outMessage);
            Assert.Equal(OutStatus.Ack, outMessage.Status);
        }

        private static AS4Message CreateMultihopSignalMessage(string messageId, string refToMessageId)
        {
            var receipt = new Receipt(messageId)
            {
                RefToMessageId = refToMessageId
            };

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
                    Action = "Receive_Agent_Forwarding_Action",
                    Service = new Xml.Service()
                    {
                        Value = "Receive_Agent_Forwarding_Service",
                        type = "eu:europa:services"
                    }
                }
            };

            return AS4Message.Create(receipt);
        }

        private async Task CreateExistingOutMessage(string messageId)
        {
            var outMessage = new OutMessage
            {
                EbmsMessageId = messageId,
                Status = OutStatus.Sent,
                PMode = await AS4XmlSerializer.ToStringAsync(GetSendingPMode())
            };

            _databaseSpy.InsertOutMessage(outMessage);
        }

        #endregion

        private static SendingProcessingMode GetSendingPMode()
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

            return AS4Message.Create(error, GetSendingPMode());
        }
       
        private void AssertIfInMessageExistsForSignalMessage(string expectedId)
        {
            InMessage inMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == expectedId);
            Assert.NotNull(inMessage);
            Assert.Equal(InStatus.Received, inMessage.Status);
            Assert.Equal(Operation.ToBeNotified, inMessage.Operation);
        }

        private void AssertIfStatusOfOutMessageIs(string expectedId, OutStatus expectedStatus)
        {
            OutMessage outMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == expectedId);

            Assert.NotNull(outMessage);
            Assert.Equal(expectedStatus, outMessage.Status);
        }

        // TODO:
        // - Create a test that verifies if the Status for a received receipt/error is set to
        // --> ToBeNotified when the receipt is valid
        // --> Exception when the receipt is invalid (also, an InException should be created)

        // - Create a test that verifies if the Status for a received UserMessage is set to
        // - Exception when the UserMessage is not valid (an InException should be present).
        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }
    }
}