using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="DeliverMessageTransformer"/>
    /// </summary>
    public class GivenDeliverMessageTransformerFacts
    {
        [Fact]
        public async Task FailsToTransform_IfNoUserMessageCanBeFound()
        {
            // Arrange
            var sut = new DeliverMessageTransformer();
            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(
                updateInMessage: m => m.EbmsMessageId = "ignored id",
                as4Message: new AS4Message());

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.TransformAsync(receivedMessage, CancellationToken.None));
        }

        [Fact]
        public async Task TransformerRemoveUnnecessaryUserMessages_IfMessageIsntReferenced()
        {
            // Arrange
            const string expectedId = "usermessage-id";

            AS4Message as4Message = new AS4MessageBuilder()
                 .WithUserMessage(new FilledUserMessage(expectedId))
                 .WithUserMessage(new FilledUserMessage())
                 .Build();

            ReceivedMessageEntityMessage receivedMessage = CreateReceivedMessage(m => m.EbmsMessageId = expectedId, as4Message);
            var sut = new DeliverMessageTransformer();

            // Act
            InternalMessage actualMessage = await sut.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.Equal(1, actualMessage.AS4Message.UserMessages.Count);
            UserMessage actualUserMessage = actualMessage.AS4Message.PrimaryUserMessage;
            Assert.Equal(expectedId, actualUserMessage.MessageId);
        }

        private static ReceivedMessageEntityMessage CreateReceivedMessage(Action<InMessage> updateInMessage, AS4Message as4Message)
        {
            var inMessage = new InMessage();
            updateInMessage(inMessage);

            return new ReceivedMessageEntityMessage(inMessage)
            {
                RequestStream = as4Message.ToStream(),
                ContentType = Constants.ContentTypes.Soap
            };
        }
    }
}