using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

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

        [Fact]
        public async Task Received_Bundled_Response_Should_Process_All_Messages()
        {
            // Arrange
            string storedMessageId = "stored-" + Guid.NewGuid();
            StoreToBeAckOutMessage(storedMessageId);
            AS4Message bundled = CreateBundledMultipleUserMessagesWithRefTo();
            bundled.AddMessageUnit(new Receipt(storedMessageId));

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
                _databaseSpy.GetInMessages(bundled.UserMessages.Select(u => u.MessageId).ToArray()),
                userMessage1 =>
                {
                    Assert.Equal(MessageExchangePattern.Pull, userMessage1.MEP);
                    Assert.Equal(InStatus.Received, userMessage1.Status.ToEnum<InStatus>());
                    Assert.Equal(Operation.ToBeDelivered, userMessage1.Operation);
                },
                userMessage2 =>
                {
                    Assert.Equal(MessageExchangePattern.Pull, userMessage2.MEP);
                    Assert.Equal(InStatus.Received, userMessage2.Status.ToEnum<InStatus>());
                    Assert.Equal(Operation.ToBeDelivered, userMessage2.Operation);
                });
            Assert.Collection(
                _databaseSpy.GetInMessages(bundled.SignalMessages.Select(s => s.MessageId).ToArray()),
                signal =>
                {
                    Assert.Equal(MessageExchangePattern.Pull, signal.MEP);
                    Assert.Equal(InStatus.Received, signal.Status.ToEnum<InStatus>());
                    Assert.Equal(Operation.ToBeNotified, signal.Operation);
                });
            Assert.Collection(
                _databaseSpy.GetOutMessages(storedMessageId),
                stored => Assert.Equal(OutStatus.Ack, stored.Status.ToEnum<OutStatus>()));
        }

        private void StoreToBeAckOutMessage(string storedMessageId)
        {
            var storedUserMessage = new OutMessage(ebmsMessageId: storedMessageId);
            storedUserMessage.EbmsMessageType = MessageType.UserMessage;
            storedUserMessage.SetStatus(OutStatus.Sent);

            _databaseSpy.InsertOutMessage(storedUserMessage);
        }

        private static AS4Message CreateBundledMultipleUserMessagesWithRefTo()
        {
            var userMessage1 = new UserMessage(
                messageId: "user1-" + Guid.NewGuid(),
                collaboration: new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "pullreceive_bundled_pmode"),
                    service: new Service(
                        value: "bundling",
                        type: "as4:net:pullreceive:bundling"),
                    action: "as4:net:pullreceive:bundling",
                    conversationId: "as4:net:pullreceive:conversation"));

            var userMessage2 = new UserMessage(
                messageId: "user2-" + Guid.NewGuid(),
                collaboration: new CollaborationInfo(
                    agreement: new AgreementReference(
                        value: "http://agreements.europa.org/agreement",
                        pmodeId: "some-other-pmode-id"),
                    service: new Service(
                        value: "bundling",
                        type: "as4:net:pullreceive:bundling"),
                    action: "as4:net:pullreceive:bundling",
                    conversationId: "as4:net:pullreceive:conversation"));

            var bundled = AS4Message.Create(userMessage1);
            bundled.AddMessageUnit(userMessage2);

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
