using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Testing <see cref="AS4ExceptionBuilder" />
    /// </summary>
    public class GivenAS4ExceptionBuilderFacts
    {
        private const ErrorAlias TesExceptionType = ErrorAlias.NonApplicable;
        private const string TestAS4Description = "Test AS4 Description";
        private const ErrorCode TestErrorCode = ErrorCode.Ebms0001;
        private readonly string[] _testMessageIds = {"Test AS4 Message Id"};

        public class GivenValidArguments : GivenAS4ExceptionBuilderFacts
        {
            [Fact]
            public void ThenBuildAS4ExceptionWithDescription()
            {
                // Act
                AS4Exception as4Exception = AS4ExceptionBuilder.WithDescription(TestAS4Description).Build();

                // Assert
                Assert.NotNull(as4Exception);
                Assert.Equal(TestAS4Description, as4Exception.Message);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithDescriptionAndMessageIds()
            {
                // Act
                AS4Exception as4Exception = AS4ExceptionBuilder
                    .WithDescription(TestAS4Description)
                    .WithMessageIds(_testMessageIds)
                    .Build();

                // Assert
                Assert.Equal(_testMessageIds, as4Exception.MessageIds);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithDescriptionMessageIdsAndErrorCode()
            {
                // Act
                AS4Exception as4Exception = AS4ExceptionBuilder
                    .WithDescription(TestAS4Description)
                    .WithMessageIds(_testMessageIds)
                    .WithErrorCode(TestErrorCode)
                    .Build();

                // Assert
                Assert.Equal(TestErrorCode, as4Exception.ErrorCode);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithDescriptionMessageIdsErrorCodeAndExceptionType()
            {
                // Act
                AS4Exception as4Exception = AS4ExceptionBuilder
                    .WithDescription(TestAS4Description)
                    .WithMessageIds(_testMessageIds)
                    .WithErrorCode(TestErrorCode)
                    .WithErrorAlias(TesExceptionType)
                    .Build();

                // Assert
                Assert.Equal(TesExceptionType, as4Exception.ErrorAlias);
            }

            [Fact]
            public void ThenInnerAS4ExceptionTakesOverThePublics()
            {
                // Arrange
                var existingAS4Exception = new AS4Exception("Test Exising AS4 Exception")
                {
                    ErrorCode = ErrorCode.Ebms0001,
                    ErrorAlias = ErrorAlias.ConnectionFailure,
                    PMode = "<PMode></PMode>"
                };

                existingAS4Exception.SetMessageIds(new[] {"message-id-1"});

                // Act
                AS4Exception as4Exception = AS4ExceptionBuilder
                    .WithDescription("New Description")
                    .WithErrorCode(ErrorCode.Ebms0002)
                    .WithErrorAlias(ErrorAlias.ExternalPayloadError)
                    .WithMessageIds("message-id-2")
                    .WithPModeString("<PMode></PMode>")
                    .WithInnerException(existingAS4Exception)
                    .Build();

                // Assert
                Assert.Equal(existingAS4Exception.ErrorCode, as4Exception.ErrorCode);
                Assert.Equal(existingAS4Exception.ErrorAlias, as4Exception.ErrorAlias);
                Assert.Equal(2, as4Exception.MessageIds.Length);
            }

            [Fact]
            public void ThenStartFromAnExistingExceptionExpandPublicAndOverrulesPrivate()
            {
                // Arrange
                var existingAS4Exception = new AS4Exception("Test Existing AS4 Exception");

                // Act
                AS4Exception as4Exception = AS4ExceptionBuilder
                    .WithDescription(TestAS4Description)
                    .WithErrorCode(TestErrorCode)
                    .WithExistingAS4Exception(existingAS4Exception)
                    .Build();

                // Assert
                Assert.Equal(existingAS4Exception.Message, as4Exception.Message);
                Assert.NotEqual(TestAS4Description, as4Exception.Message);
                Assert.Equal(TestErrorCode, as4Exception.ErrorCode);
            }
        }

        public class GivenInvalidArguments : GivenAS4ExceptionBuilderFacts
        {
            [Fact]
            public void ThenBuildAS4ExceptionWithNullExisting()
            {
                Assert.Throws<ArgumentNullException>(
                    () => AS4ExceptionBuilder
                        .WithDescription("AS4 Exception Facts")
                        .WithExistingAS4Exception(null)
                        .Build());
            }
        }
    }
}