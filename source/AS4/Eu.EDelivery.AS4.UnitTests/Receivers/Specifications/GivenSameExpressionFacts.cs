using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Receivers.Specifications.Expressions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="SameExpression"/>
    /// </summary>
    public class GivenSameExpressionFacts
    {
        [Fact]
        public void IsTheSame_Operation()
        {
            // Arrange
            var sut = new SameExpression();
            var inMessage = new InMessage {Operation = Operation.ToBeDelivered};

            // Act
            bool isTheSame = sut.Equals("Operation", "ToBeDelivered", inMessage);

            // Assert
            Assert.True(isTheSame);
        }

        [Theory]
        [InlineData("message-id", false)]
        [InlineData(null, true)]
        public void IsTheSameAsNull_If(string expectedId, bool expectedEvaluation)
        {
            // Arrange
            var sut = new SameExpression();
            var inMessage = new InMessage {EbmsMessageId = expectedId};

            // Act
            bool actualEvaluation = sut.Equals("EbmsMessageId", "NULL", inMessage);

            // Assert
            Assert.Equal(expectedEvaluation, actualEvaluation);
        }
    }
}
