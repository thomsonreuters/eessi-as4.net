using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Receivers.Specifications.Expressions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="Token"/>
    /// </summary>
    public class GivenTokenFacts
    {
        [Theory]
        [InlineData(";alskdjf")]
        [InlineData("(1 = = 1)")]
        [InlineData("2 IS  IS 2")]
        [InlineData("1 AND OR 2")]
        [InlineData(";laksjdf = L:KAJSDlk = ;laksjdfa")]
        [InlineData("(()))")]
        public void FailsToTokenize_IfInvalidExpression(string expression)
        {
            // Act / Assert
            Assert.ThrowsAny<Exception>(() => Token.Tokenize(expression).ToList());
        }

        [Theory]
        [InlineData("(1 = 2 OR 2 != 3)", 5)]
        [InlineData("(1 IS 1 AND (2 IS NOT 3 OR 2 IS NOT 4))", 9)]
        public void TokenizeIntoExpectedAmount(string expression, int expectedAmount)
        {
            // Act
            IEnumerable<Token> tokens = Token.Tokenize(expression);

            // Assert
            Assert.Equal(expectedAmount, tokens.Count());
        }

        [Theory]
        [InlineData("Operation = ToBeDelivered")]
        [InlineData("Operation != ToBeDelivered")]
        [InlineData("Operation IS ToBeDelivered")]
        [InlineData("Operation IS NOT ToBeDelivered")]
        public void IsEqualExpression(string expression)
        {
            // Act
            IEnumerable<Token> tokens = Token.Tokenize(expression);

            // Assert
            Token firstToken = tokens.First();
            Assert.True(firstToken.IsEqualExpression);
        }

        [Theory]
        [InlineData("(1 = 1) AND (2 IS NOT 3)")]
        [InlineData("(2 IS 2) OR (1 IS NOT 2)")]
        public void IsOpenParentthesis(string expression)
        {
            // Act
            IEnumerable<Token> tokens = Token.Tokenize(expression);

            // Assert
            Assert.True(tokens.First().IsOpenParenthesis);
            Assert.True(tokens.Last().IsClosedParenthesis);
        }

        [Theory]
        [InlineData("True", true)]
        [InlineData("False", false)]
        public void EvaluateBoolean(string booleanValue, bool expected)
        {
            // Arrange
            IEnumerable<Token> tokens = Token.Tokenize(booleanValue);
            Token sut = tokens.First();

            // Act
            bool actual = sut.Evaluate;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
