using Eu.EDelivery.AS4.Entities;
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
            Assert.Equal(ReceptionStatus.Pending, ReceptionStatusUtils.Parse(new ReceptionAwareness().Status));
        }

        [Fact]
        public void ReceptionAwarenessLocksByUpdatingStatus()
        {
            // Arrange
            const ReceptionStatus expectedStatus = ReceptionStatus.Busy;
            var sut = new ReceptionAwareness();
            sut.SetStatus(ReceptionStatus.Pending);

            // Act
            sut.Lock(expectedStatus.ToString());

            // Assert
            Assert.Equal(expectedStatus, ReceptionStatusUtils.Parse(sut.Status));
        }
    }
}
