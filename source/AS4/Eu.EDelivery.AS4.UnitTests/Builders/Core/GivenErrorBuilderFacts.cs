using System;
using System.Linq;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Testing <see cref="ErrorBuilder" />
    /// </summary>
    public class GivenErrorBuilderFacts
    {
        public class GivenValidArguments : GivenErrorBuilderFacts
        {
            [Fact]
            public void BuildsWithErrorResult()
            {
                // Arrange
                var result = new ErrorResult(
                    Guid.NewGuid().ToString(),
                    ErrorCode.Ebms0001,
                    default(ErrorAlias));
                string messageId = Guid.NewGuid().ToString();

                // Act
                Error error = new ErrorBuilder(messageId).WithErrorResult(result).Build();

                // Assert
                ErrorDetail firstDetail = error.Errors.First();
                Assert.Equal(result.Description, firstDetail.Detail);
                Assert.Equal($"EBMS:{(int) result.Code:0000}", firstDetail.ErrorCode);
            }

            [Fact]
            public void ThenBuildErrorSucceedsWithEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                string refToMessageId = Guid.NewGuid().ToString();

                // Act
                Error error = new ErrorBuilder(messageId).WithRefToEbmsMessageId(refToMessageId).Build();

                // Assert
                Assert.Equal(refToMessageId, error.RefToMessageId);
            }
        }
    }
}