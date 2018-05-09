using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly Settings _pullReceiveSettings;
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullReceiveAgentFacts"/> class.
        /// </summary>
        public PullReceiveAgentFacts()
        {
            _pullReceiveSettings = OverrideSettings("pullreceiveagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

        [Fact]
        public async Task NoExceptionsAreLoggedWhenPullSenderIsNotAvailable()
        {
            string pullSenderUrl = RetrievePullingUrlFromConfig();

            await RespondToPullRequest(pullSenderUrl, _ => throw new InvalidOperationException());

            Assert.False(_databaseSpy.GetInExceptions(r => true).Any(), "No logged InExceptions are expected.");
        }

        [Fact(Skip = "Not yet fully implemented")]
        public async Task Received_Bundled_Response_Should_Process_All_Messages()
        {
            // Arrange
            string storedMessageId = "stored-" + Guid.NewGuid();
            StoreToBeAckOutMessage(storedMessageId);
            AS4Message bundled = CreateBundledUserReceiptMessageWithRefTo(storedMessageId);

            string pullSenderUrl = RetrievePullingUrlFromConfig();

            // Act
            await RespondToPullRequest(
                pullSenderUrl,
                response =>
                {
                    response.ContentType = bundled.ContentType;
                    using (Stream output = response.OutputStream)
                    {
                        SerializerProvider.Default
                            .Get(bundled.ContentType)
                            .Serialize(bundled, output, CancellationToken.None);
                    }
                });

            // Assert
            Assert.Collection(
                _databaseSpy.GetInMessages(bundled.PrimaryUserMessage.MessageId),
                inUserMessage =>
                {
                    Assert.Equal(InStatus.Received, InStatusUtils.Parse(inUserMessage.Status));
                    Assert.Equal(Operation.ToBeDelivered, OperationUtils.Parse(inUserMessage.Operation));
                });
            Assert.Collection(
                _databaseSpy.GetInMessages(bundled.PrimarySignalMessage.MessageId),
                inReceipt =>
                {
                    Assert.Equal(InStatus.Received, InStatusUtils.Parse(inReceipt.Status));
                    Assert.Equal(Operation.ToBeNotified, OperationUtils.Parse(inReceipt.Operation));
                });
            Assert.Collection(
                _databaseSpy.GetOutMessages(storedMessageId),
                stored => Assert.Equal(OutStatus.Ack, OutStatusUtils.Parse(stored.Status)));
        }

        private void StoreToBeAckOutMessage(string storedMessageId)
        {
            var storedUserMessage = new OutMessage(ebmsMessageId: storedMessageId);
            storedUserMessage.SetEbmsMessageType(MessageType.UserMessage);
            storedUserMessage.SetStatus(OutStatus.Sent);

            _databaseSpy.InsertOutMessage(storedUserMessage);
        }

        private static AS4Message CreateBundledUserReceiptMessageWithRefTo(string storedMessageId)
        {
            var userMessage = new UserMessage(messageId: "usermessage-" + Guid.NewGuid());
            userMessage.CollaborationInfo.AgreementReference.PModeId = "pullreceive_bundled_pmode";
            var receipt = new Receipt(messageId: "receipt-" + Guid.NewGuid());
            receipt.RefToMessageId = storedMessageId;

            var bundled = AS4Message.Create(userMessage);
            bundled.AddMessageUnit(receipt);
            return bundled;
        }

        private string RetrievePullingUrlFromConfig()
        {
            AgentSettings pullReceiveAgent = _pullReceiveSettings.Agents.PullReceiveAgents.FirstOrDefault();

            if (pullReceiveAgent == null)
            {
                throw new ConfigurationErrorsException("There is no PullReceive Agent configured.");
            }

            string pmodeId = pullReceiveAgent.Receiver.Setting.First().Key;

            SendingProcessingMode pmode = _as4Msh.GetConfiguration().GetSendingPMode(pmodeId);

            if (pmode == null)
            {
                throw new ConfigurationErrorsException($"No Sending PMode found with Id {pmodeId}");
            }

            return pmode.PushConfiguration.Protocol.Url;
        }

        private static async Task RespondToPullRequest(string url, Action<HttpListenerResponse> response)
        {
            // Wait a little bit to be sure that everything is started and a PullRequest has already been sent.
            await Task.Delay(TimeSpan.FromSeconds(2));

            var waiter = new ManualResetEvent(false);
            StubHttpServer.StartServer(url, response, waiter);
            waiter.WaitOne(timeout: TimeSpan.FromSeconds(5));

            // Wait till the response is processed correctly.
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
