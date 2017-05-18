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
            BooleanExpression expression = BooleanExpression.For(type);

            // Act
            bool actual = expression.Evaluate(left, right);

            // Assert
            Assert.Equal(expected, actual);
        }

        public class AndExpressionFacts
        {
            [Theory]
            [InlineData(false, false, false)]
            [InlineData(false, true, false)]
            [InlineData(true, false, false)]
            [InlineData(true, true, true)]
            public void EvaluateAndExpression(bool left, bool right, bool expected)
            {
                // Arrange
                var sut = new AndExpression();

                // Act
                bool actual = sut.Evaluate(left, right);

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        public class OrExpressionFacts
        {
            [Theory]
            [InlineData(false, false, false)]
            [InlineData(false, true, true)]
            [InlineData(true, false, true)]
            [InlineData(true, true, true)]
            public void EvaluateOrExpression(bool left, bool right, bool expected)
            {
                // Arrange
                var sut = new OrExpression();

                // Act
                bool actual = sut.Evaluate(left, right);

                // Assert
                Assert.Equal(expected, actual);
            }
        }
    }
}
