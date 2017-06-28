using System;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Notify;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Singletons;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Notify
{
    /// <summary>
    /// Testing <see cref="ErrorToNotifyMap" />
    /// </summary>
    public class GivenErrorToNotifyMapFacts
    {
        public class GivenValidArguments : GivenErrorToNotifyMapFacts
        {
            [Fact]
            public void ThenMapMessageInfoSucceeds()
            {
                // Arrange
                var error = new Error("message-id");

                // Act
                var notifyMessage = AS4Mapper.Map<NotifyMessage>(error);

                // Assert
                Assert.Equal(error.MessageId, notifyMessage.MessageInfo.MessageId);
                Assert.Equal(error.RefToMessageId, notifyMessage.MessageInfo.RefToMessageId);
            }

            [Fact]
            public void ThenNotifyMessageHasStatusError()
            {
                // Arrange
                var error = new Error("message-id");

                // Act
                var notifyMessage = AS4Mapper.Map<NotifyMessage>(error);

                // Assert
                Assert.Equal(Status.Error, notifyMessage.StatusInfo.Status);
            }
        }
    }
}