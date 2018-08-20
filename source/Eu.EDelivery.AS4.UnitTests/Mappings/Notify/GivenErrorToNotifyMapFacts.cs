using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Singletons;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Notify
{
    public class GivenErrorToNotifyMapFacts
    {
        [Fact]
        public void ThenMapMessageInfoSucceeds()
        {
            // Arrange
            var error = new Error($"error-{Guid.NewGuid()}", $"user-{Guid.NewGuid()}");

            // Act
            var notifyMessage = AS4Mapper.Map<NotifyMessage>(error);

            // Assert
            Assert.Equal(error.MessageId, notifyMessage.MessageInfo.MessageId);
            Assert.Equal(error.RefToMessageId, notifyMessage.MessageInfo.RefToMessageId);
            Assert.Equal(Status.Error, notifyMessage.StatusInfo.Status);
        }
    }
}