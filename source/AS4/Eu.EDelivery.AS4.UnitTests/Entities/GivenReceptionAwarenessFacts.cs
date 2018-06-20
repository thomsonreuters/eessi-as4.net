using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    /// <summary>
    /// Testing <see cref="ReceptionAwareness"/>
    /// </summary>
    public class GivenReceptionAwarenessFacts
    {
        [Fact]
        public void ReceptionAwarenessHasDefaultStatus()
        {
            Assert.Equal(ReceptionStatus.Pending, new ReceptionAwareness(1, "message-id").Status.ToEnum<ReceptionStatus>());
        }

        [Fact]
        public void ReceptionAwarenessLocksByUpdatingStatus()
        {
            // Arrange
            const ReceptionStatus expectedStatus = ReceptionStatus.Busy;
            var sut = new ReceptionAwareness(1, "message-id");
            sut.SetStatus(ReceptionStatus.Pending);

            // Act
            sut.Lock(expectedStatus.ToString());

            // Assert
            Assert.Equal(expectedStatus, sut.Status.ToEnum<ReceptionStatus>());
        }
    }
}
