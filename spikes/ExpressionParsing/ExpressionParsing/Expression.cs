using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ExpressionParsing
{
    public class Expression
    {
        [Theory]
        [InlineData("1 = 1 AND 2 != 3", "True")]
        [InlineData("1 != 2 OR 1 = 2", "True")]
        [InlineData("(1 = 1 AND 2 = 2) OR 3 != 4", "True")]
        [InlineData("(1 = 1 AND 3 != 4) AND (3 = 1 OR 2 = 6)", "False")]
        [InlineData("(11 != 11 OR (22 = 22 AND 33 = 33) AND 22 != 22) OR 44 = 44", "True")]
        [InlineData("1 = 4 OR 1 = 6 OR 1 = 1", "True")]
        public void TestExpressionEvaluate(string expression, string expected)
        {
            string actual = ExpressionEvaluate(expression);

            Assert.Equal(expected, actual);
        }

        public string ExpressionEvaluate(string expression)
        {
            var stack = new Stack<string>();

            foreach (string token in Tokenize(expression))
            {
                if (token.Equals(")"))
                {
                    stack.Push(EvaluateEverythingInsideParenthesis(stack));
                }
                else
                {
                    stack.Push(token.Contains("=") ? EqualEvaluate(token) : token);
                }
            }

            return LogicalEval(stack.ToArray());
        }

        private string EvaluateEverythingInsideParenthesis(Stack<string> stack)
        {
            var everythingInParenthesis = new List<string>();
            string nextToken = stack.Pop();

            while (!nextToken.Equals("("))
            {
                everythingInParenthesis.Add(nextToken);
                nextToken = stack.Pop();
            }

            return LogicalEval(everythingInParenthesis.ToArray());
        }

        [Theory]
        [InlineData("1 = 1 AND 2 != 2", new [] {"1 = 1", "AND", "2 != 2"})]
        [InlineData("(1 = 1 AND 2 = 2) OR 3 != 4", new [] {"(", "1 = 1", "AND", "2 = 2", ")", "OR", "3 != 4"})]
        public void TestTokenize(string filter, string[] expectedTokens)
        {
            string[] actualTokens = Tokenize(filter);
            Assert.Equal(expectedTokens, actualTokens);
        }

        public string[] Tokenize(string filter)
        {
            return
                Regex.Split(filter, @"(AND|OR|\(|\))")
                     .Select(t => t.Trim())
                     .Where(t => !string.IsNullOrEmpty(t))
                     .ToArray();
        }

        [Theory]
        [InlineData("1 = 1", "True")]
        [InlineData("2=2", "True")]
        [InlineData("1 != 2", "True")]
        public void TestEqualEvaluate(string expression, string expectedEval)
        {
            string actualEval = EqualEvaluate(expression);

            Assert.Equal(expectedEval, actualEval);
        }

        public string EqualEvaluate(string expression)
        {
            string separator = expression.Contains("!=") ? "!=" : "=";
            string[] entries =
                expression.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries)
                          .Select(e => e.Trim())
                          .ToArray();

            string left = entries[0];
            string right = entries[1];

            return GetEqualExpression(separator)(left, right).ToString();
        }

        private static Func<string, string, bool> GetEqualExpression(string equal)
        {
            return new Dictionary<string, Func<string, string, bool>>
            {
                ["!="] = (a, b) => a != b,
                ["="] = (a, b) => a == b
            }[equal];
        }

        [Theory]
        [InlineData(new [] {"True", "AND", "False"}, "False")]
        [InlineData(new [] {"False", "AND", "True", "AND", "True"}, "False")]
        [InlineData(new [] {"True", "OR", "False"}, "True")]
        [InlineData(new [] {"False", "OR", "False", "OR", "False"}, "False")]
        public void TestLogicalEval(string[] tokens, string expectedEval)
        {
            string actualEval = LogicalEval(tokens);

            Assert.Equal(expectedEval, actualEval);
        }

        public string LogicalEval(string[] expression)
        {
            var stack = new Stack<string>();

            foreach (string token in expression)
            {
                stack.Push(token);

                if (stack.Count == 3)
                {
                    stack.Push(EvaluateBooleanExpression(stack));
                }
            }

            return stack.Pop();
        }

        private string EvaluateBooleanExpression(Stack<string> stack)
        {
            bool.TryParse(stack.Pop(), out var left);
            Func<bool, bool, bool> booleanFunc = GetLogicalExpression(stack.Pop());
            bool.TryParse(stack.Pop(), out var right);

            return booleanFunc(left, right).ToString();
        }

        public Func<bool, bool, bool> GetLogicalExpression(string booleanFunc)
        {
            return new Dictionary<string, Func<bool, bool, bool>>
            {
                ["AND"] = (a, b) => a && b,
                ["OR"] = (a, b) => a || b
            }[booleanFunc];
        }
    }
}
