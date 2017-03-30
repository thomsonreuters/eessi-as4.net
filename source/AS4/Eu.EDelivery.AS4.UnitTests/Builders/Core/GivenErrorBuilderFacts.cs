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
            public void ThenBuildErrorSucceedsWithAS4Exception()
            {
                // Arrange
                AS4Exception as4Exception = CreateDefaultAS4Exception();
                as4Exception.ErrorCode = ErrorCode.Ebms0001;
                string messageId = Guid.NewGuid().ToString();

                // Act
                Error error = new ErrorBuilder(messageId).WithAS4Exception(as4Exception).Build();

                // Assert
                Assert.Equal(as4Exception, error.Exception);
                Assert.Equal(2, error.Exception.MessageIds.Length);
                AssertErrorDetails(as4Exception, error);
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

            [Fact]
            public void ThenBuildErrorSucceedsWithOriginalAS4Exception()
            {
                // Arrange
                AS4Exception as4Exception = CreateDefaultAS4Exception();
                as4Exception.ErrorCode = ErrorCode.Ebms0001;
                string messageId = Guid.NewGuid().ToString();

                // Act
                Error error = new ErrorBuilder(messageId).WithAS4Exception(as4Exception).BuildWithOriginalAS4Exception();

                // Assert
                Assert.Equal(as4Exception, error.Exception);
                Assert.Equal(1, error.Exception.MessageIds.Length);
                AssertErrorDetails(as4Exception, error);
            }

            private static void AssertErrorDetails(AS4Exception as4Exception, Error error)
            {
                string errorCodeString = $"EBMS:{(int)as4Exception.ErrorCode:0000}";
                ErrorDetail firstErrorDetail = error.Errors.First();

                Assert.Equal(errorCodeString, firstErrorDetail.ErrorCode);
                Assert.Equal(as4Exception.Message, firstErrorDetail.Detail);
                Assert.Equal(Severity.FAILURE, firstErrorDetail.Severity);
            }
        }

        protected AS4Exception CreateDefaultAS4Exception()
        {
            return AS4ExceptionBuilder
                .WithDescription("Test AS4 Exception")
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }
    }
}