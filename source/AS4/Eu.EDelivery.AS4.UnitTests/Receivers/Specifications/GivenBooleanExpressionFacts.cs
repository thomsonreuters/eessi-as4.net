using Eu.EDelivery.AS4.Receivers.Specifications.Expressions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="BooleanExpression"/>
    /// </summary>
    public class GivenBooleanExpressionFacts
    {
        [Theory]
        [InlineData("AND", "False", "False", false)]
        [InlineData("AND", "False", "True", false)]
        [InlineData("AND", "True", "False", false)]
        [InlineData("AND", "True", "True", true)]
        [InlineData("OR", "False", "False", false)]
        [InlineData("OR", "False", "True", true)]
        [InlineData("OR", "True", "False", true)]
        [InlineData("OR", "True", "True", true)]
        public void GetsRightBooleanExpressionType(string type, string left, string right, bool expected)
        {
            // Arrange
            BooleanExpression expression = BooleanExpression.For(type, left, right);

            // Act
            bool actual = expression.Evaluate();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
