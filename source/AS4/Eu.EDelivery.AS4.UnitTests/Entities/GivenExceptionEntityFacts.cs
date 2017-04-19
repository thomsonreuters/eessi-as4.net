using Eu.EDelivery.AS4.Entities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    /// <summary>
    /// Testing <see cref="ExceptionEntity"/>
    /// </summary>
    public class GivenExceptionEntityFacts
    {
        [Fact]
        public void ExceptionEntityLocksInstanceByUpdatingOperation()
        {
            // Arrange
            const Operation expectedOperation = Operation.DeadLettered;
            var sut = new ExceptionEntity();

            // Act
            sut.Lock(expectedOperation.ToString());

            // Assert
            Assert.Equal(expectedOperation, sut.Operation);
        }

        [Fact]
        public void ExceptionEntityDoesntLockInstance_IfOperationIsNotApplicable()
        {
            // Arrange
            const Operation expectedOperation = Operation.NotApplicable;
            var sut = new ExceptionEntity {Operation = Operation.DeadLettered};

            // Act
            sut.Lock(expectedOperation.ToString());

            // Assert
            Assert.NotEqual(expectedOperation, sut.Operation);
        }
    }
}
