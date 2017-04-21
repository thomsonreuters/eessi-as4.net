using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    /// <summary>
    /// Testing <see cref="InExceptionBuilder" />
    /// </summary>
    public class GivenInExceptionBuilderFacts
    {
        public class GivenValidArgumens : GivenInExceptionBuilderFacts
        {
            [Fact]
            public void ThenBuildInExceptionSucceedsWithAS4Exception()
            {
                AS4Exception as4Exception = CreateDefaultAS4Exception();

                // Act
                InException inException = new InExceptionBuilder().WithAS4Exception(as4Exception).Build();

                // Assert
                Assert.Equal(as4Exception.PMode, inException.PMode);
                Assert.Equal(as4Exception.ErrorAlias, inException.ErrorAlias);
            }

            [Fact]
            public void ThenBuildInExceptionSucceedsWithEbmsMessageId()
            {
                // Arrange
                AS4Exception as4Exception = CreateDefaultAS4Exception();
                string messageId = Guid.NewGuid().ToString();

                // Act
                InException inException = new InExceptionBuilder()
                    .WithAS4Exception(as4Exception)
                    .WithEbmsMessageId(messageId)
                    .Build();

                // Assert
                Assert.Equal(messageId, inException.EbmsRefToMessageId);
            }

            [Fact]
            public void ThenBuildInExceptionSucceedsWithExceptionAndEbmsMessageId()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                AS4Exception as4Exception = CreateDefaultAS4Exception();

                // Act
                InException inException = new InExceptionBuilder()
                    .WithEbmsMessageId(messageId)
                    .WithAS4Exception(as4Exception)
                    .Build();

                // Assert
                Assert.Equal(messageId, inException.EbmsRefToMessageId);
                Assert.Equal(as4Exception.PMode, inException.PMode);
                Assert.Equal(as4Exception.ErrorAlias, inException.ErrorAlias);
            }
        }

        protected AS4Exception CreateDefaultAS4Exception()
        {
            // Arrange
            return AS4ExceptionBuilder
                .WithDescription("Test Exception")
                .WithPModeString("<PMode></PMode>")
                .WithErrorAlias(ErrorAlias.ConnectionFailure)
                .Build();
        }
    }
}