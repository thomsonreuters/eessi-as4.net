using Eu.EDelivery.AS4.Entities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    /// <summary>
    /// Testing <see cref="MessageEntity"/>
    /// </summary>
    public class GivenMessageEntityFacts
    {
        [Fact]
        public void MessageEntityLocksInstanceByUpdatingOperation()
        {
            // Arrange
            var sut = new StubMessageEntity();
            const Operation expectedOperation = Operation.Sending;

            // Act
            sut.Lock(expectedOperation.ToString());

            // Assert
            Assert.Equal(Operation.Sending, sut.Operation);
        }

        [Fact]
        public void MessageEntityDoesntLockInstance_IfUpdateOperationIsNotApplicable()
        {
            // Arrange
            const Operation expectedOperation = Operation.Notified;
            var sut = new StubMessageEntity {Operation = expectedOperation};

            // Act
            sut.Lock(Operation.NotApplicable.ToString());

            // Assert
            Assert.Equal(expectedOperation, sut.Operation);
        }

        private class StubMessageEntity : MessageEntity
        {
            public override string StatusString { get; set; }
        }
    }
}