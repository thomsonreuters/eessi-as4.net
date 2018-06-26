using System;
using Eu.EDelivery.AS4.Mappings.Notify;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Singletons;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Notify
{
    /// <summary>
    /// Testing <see cref="ReceiptToNotifyMap" />
    /// </summary>
    public class GivenReceiptToNotifyMapFacts
    {

        public class GivenValidArguments : GivenReceiptToNotifyMapFacts
        {
            [Fact]
            public void ThenMapMessageInfoSucceeds()
            {
                // Arrange
                var receipt = new Receipt("message-id", "ref-to-message-id");

                // Act
                var notifyMessage = AS4Mapper.Map<NotifyMessage>(receipt);

                // Assert
                MessageInfo notifyMessageInfo = notifyMessage.MessageInfo;
                Assert.Equal(receipt.MessageId, notifyMessageInfo.MessageId);
                Assert.Equal(receipt.RefToMessageId, notifyMessageInfo.RefToMessageId);
            }

            [Fact]
            public void ThenNotifyMessageHasStatusDelivered()
            {
                // Arrange
                var receipt = new Receipt("message-id", Guid.NewGuid().ToString());

                // Act
                var notifyMessage = AS4Mapper.Map<NotifyMessage>(receipt);

                // Assert
                Assert.Equal(Status.Delivered, notifyMessage.StatusInfo.Status);
            }
        }
    }
}