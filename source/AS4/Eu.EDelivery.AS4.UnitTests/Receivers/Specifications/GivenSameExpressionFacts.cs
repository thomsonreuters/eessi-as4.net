using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Receivers.Specifications.Expressions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="SameExpression{T}"/>
    /// </summary>
    public class GivenSameExpressionFacts
    {
        [Fact]
        public void IsTheSame_Operation()
        {
            // Arrange
            var inMessage = new InMessage {Operation = Operation.ToBeDelivered};
            var sut = EqualExpression<InMessage>.For("Operation = ToBeDelivered", inMessage);

            // Act
            bool isTheSame = sut.Evaluate();

            // Assert
            Assert.True(isTheSame);
        }

        [Theory]
        [InlineData("message-id", false)]
        [InlineData(null, true)]
        public void IsTheSameAsNull_If(string expectedId, bool expectedEvaluation)
        {
            // Arrange
            var inMessage = new InMessage {EbmsMessageId = expectedId};
            var sut = EqualExpression<InMessage>.For("EbmsMessageId = NULL", inMessage);

            // Act
            bool actualEvaluation = sut.Evaluate();

            // Assert
            Assert.Equal(expectedEvaluation, actualEvaluation);
        }
    }
}
