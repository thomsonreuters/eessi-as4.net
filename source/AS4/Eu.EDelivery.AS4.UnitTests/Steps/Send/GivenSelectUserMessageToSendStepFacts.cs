using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SelectUserMessageToSendStep"/>
    /// </summary>
    public class GivenSelectUserMessageToSendStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task SelectsUserMessage_IfUserMessageMatchesCriteria()
        {
            // Arrange
            const string expectedId = "message-id", expectedMpc = "message-mpc";
            InsertUserMessageForPullRequest(expectedId, expectedMpc);

            var sut = new SelectUserMessageToSendStep(GetDataStoreContext);
            MessagingContext context = ContextWithPullRequest(expectedId, expectedMpc);

            // Act
            StepResult result = await sut.ExecuteAsync(context, CancellationToken.None);

            // Assert
            UserMessage userMessage = result.MessagingContext.AS4Message.PrimaryUserMessage;
            Assert.Equal(expectedId, userMessage.MessageId);
            Assert.Equal(expectedMpc, userMessage.Mpc);
        }

        private void InsertUserMessageForPullRequest(string expectedId, string expectedMpc)
        {
            InsertOutMessage(
                m =>
                {
                    m.EbmsMessageId = expectedId;
                    m.Operation = Operation.ToBeSent;
                    m.Mpc = expectedMpc;
                    m.MEP = MessageExchangePattern.Pull;
                });
        }

        private void InsertOutMessage(Action<OutMessage> arrangeMessage)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                var message = new OutMessage();
                arrangeMessage(message);

                context.OutMessages.Add(message);
                context.SaveChanges();
            }
        }

        private static MessagingContext ContextWithPullRequest(string id, string mpc)
        {
            var pullRequest = new PullRequest(mpc, id);
            AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(pullRequest).Build();

            return new MessagingContext(as4Message);
        }
    }
}
