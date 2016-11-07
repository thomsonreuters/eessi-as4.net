using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Testing <see cref="AS4ExceptionBuilder"/>
    /// </summary>
    public class GivenAS4ExceptionBuilderFacts
    {
        protected string[] TestMessageIds = {"Test AS4 Message Id"};
        protected string TestAS4Description = "Test AS4 Description";
        protected ErrorCode TestErrorCode = ErrorCode.Ebms0001;
        protected ExceptionType TesExceptionType = ExceptionType.NonApplicable;

        public class GivenValidArguments : GivenAS4ExceptionBuilderFacts
        {
            [Fact]
            public void ThenBuildAS4ExceptionWithDescription()
            {
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder()
                    .WithDescription(base.TestAS4Description).Build();
                // Assert
                Assert.NotNull(as4Exception);
                Assert.Equal(base.TestAS4Description, as4Exception.Message);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithDescriptionAndMessageIds()
            {
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder()
                    .WithDescription(base.TestAS4Description)
                    .WithMessageIds(base.TestMessageIds)
                    .Build();
                // Assert
                Assert.Equal(base.TestMessageIds, as4Exception.MessageIds);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithDescriptionMessageIdsAndErrorCode()
            {
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder()
                    .WithDescription(base.TestAS4Description)
                    .WithMessageIds(base.TestMessageIds)
                    .WithErrorCode(base.TestErrorCode)
                    .Build();
                // Assert
                Assert.Equal(base.TestErrorCode, as4Exception.ErrorCode);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithDescriptionMessageIdsErrorCodeAndExceptionType()
            {
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder()
                    .WithDescription(base.TestAS4Description)
                    .WithMessageIds(base.TestMessageIds)
                    .WithErrorCode(base.TestErrorCode)
                    .WithExceptionType(base.TesExceptionType)
                    .Build();
                // Assert
                Assert.Equal(base.TesExceptionType, as4Exception.ExceptionType);
            }

            [Fact]
            public void ThenStartFromAnExistingExceptionExpandPublicAndOverrulesPrivate()
            {
                // Arrange
                var existingAS4Exception = new AS4Exception("Test Existing AS4 Exception");
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder()
                    .WithDescription(base.TestAS4Description)
                    .WithErrorCode(base.TestErrorCode)
                    .WithExistingAS4Exception(existingAS4Exception)
                    .Build();
                // Assert
                Assert.Equal(existingAS4Exception.Message, as4Exception.Message);
                Assert.NotEqual(base.TestAS4Description, as4Exception.Message);
                Assert.Equal(base.TestErrorCode, as4Exception.ErrorCode);
            }

            [Fact]
            public void ThenInnerAS4ExceptionTakesOverThePublics()
            {
                // Arrange
                var existingAS4Exception = new AS4Exception("Test Exising AS4 Exception")
                {
                    ErrorCode = ErrorCode.Ebms0001,
                    ExceptionType = ExceptionType.ConnectionFailure,
                    PMode = "<PMode></PMode>",
                    MessageIds = new[] {"message-id-1"}
                };
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder()
                    .WithDescription("New Description")
                    .WithErrorCode(ErrorCode.Ebms0002)
                    .WithExceptionType(ExceptionType.ExternalPayloadError)
                    .WithMessageIds(new string[] {"message-id-2"})
                    .WithPModeString("<PMode></PMode>")
                    .WithInnerException(existingAS4Exception)
                    .Build();
                // Assert
                Assert.Equal(existingAS4Exception.ErrorCode, as4Exception.ErrorCode);
                Assert.Equal(existingAS4Exception.ExceptionType, as4Exception.ExceptionType);
                Assert.Equal(2, as4Exception.MessageIds.Length);
            }
        }

        public class GivenInvalidArguments : GivenAS4ExceptionBuilderFacts
        {
            [Fact]
            public void ThenBuildAS4ExceptionWithoutDescription()
            {
                // Act
                AS4Exception as4Exception = new AS4ExceptionBuilder().Build();
                // Assert
                Assert.NotNull(as4Exception);
                Assert.NotEmpty(as4Exception.Message);
            }

            [Fact]
            public void ThenBuildAS4ExceptionWithNullExisting()
            {
                // Arrange
                AS4Exception existingAS4Exception = null;
                // Act / Assert
                Assert.Throws<ArgumentNullException>(() 
                    => new AS4ExceptionBuilder()
                    .WithExistingAS4Exception(existingAS4Exception)
                    .Build());
            }
        }
    }
}