using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Xunit;

namespace ExpressionParsing
{
    public class Expression
    {
        [Theory]
        [InlineData("Operation = ToBeSent", new[] { "Operation", "=", "ToBeSent"})]
        public void TestTokenize(string filter, string[] expectedTokens)
        {
            string[] actualTokens = Tokenize(filter);
            Assert.Equal(expectedTokens, actualTokens);
        }

        public string[] Tokenize(string filter)
        {
            string[] result = filter.Split('=');
            for (var i = 0; i < result.Length - 1; i++)
            {
                result[i] += '=';
            }

            return result;
        }
    }
}
