using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="OutExceptionBuilder" />
    /// </summary>
    public class GivenOutExceptionBuilderFacts
    {
        public class GivenValidArguments : GivenOutMessageBuilderFacts
        {
            [Fact]
            public void ThenBuildOutExceptionSucceedsWithAS4Exeption()
            {
                // Arrange
                AS4Exception as4Exception =
                    AS4ExceptionBuilder.WithDescription("Test Exception").WithPModeString("<PMode></PMode>").Build();

                // Act
                OutException outException = OutExceptionBuilder.ForAS4Exception(as4Exception).Build();

                // Assert
                Assert.Equal(as4Exception.PMode, outException.PMode);
                string exceptionString = as4Exception.ToString();
                Assert.Equal(exceptionString, outException.Exception);
            }

            [Fact]
            public void ThenBuildOutExceptionSucceedsWithEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();

                // Act
                OutException outException = OutExceptionBuilder.ForAS4Exception(new AS4Exception(""))
                                                               .WithEbmsMessageId(messageId).Build();

                // Assert
                Assert.Equal(messageId, outException.EbmsRefToMessageId);
            }

            [Fact]
            public void ThenBuildOutExceptionSucceedsWithOperation()
            {
                // Arrange
                const Operation operation = Operation.Delivered;
                const string operationMethod = "FILE";

                // Act
                OutException outException = OutExceptionBuilder.ForAS4Exception(new AS4Exception("Test exception"))
                                                               .WithOperation(operation, operationMethod).Build();

                // Assert
                Assert.Equal(operation, outException.Operation);
                Assert.Equal(operationMethod, outException.OperationMethod);
            }
            
        }
    }
}