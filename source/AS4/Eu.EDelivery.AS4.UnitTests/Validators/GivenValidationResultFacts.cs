using Eu.EDelivery.AS4.Validators;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NLog;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    public class GivenValidationResultFacts
    {
        [Fact]
        public void ExpectedErrorsGetsLogged()
        {
            // Arrange
            var result = new ValidationResult();
            result.Errors.Add(new ValidationFailure("property 1", "error message"));
            result.Errors.Add(new ValidationFailure("property 2", "error message"));

            var spyLogger = Mock.Of<ILogger>();

            // Act
            result.LogErrors(spyLogger);

            // Assert
            Mock.Get(spyLogger).Verify(l => l.Error(It.IsAny<string>()), Times.Exactly(result.Errors.Count));
        }

        [Theory]
        [InlineData("", false, true)]
        [InlineData("not empty string", true, false)]
        public void ResultPathsGetsCorrectlyCalled(string testInstance, bool expectedHappyPath, bool expectedUnhappyPath)
        {
            // Arrange
            var sut = Mock.Of<AbstractValidator<string>>();
            bool isValid = !string.IsNullOrEmpty(testInstance);
            sut.RuleFor(s => isValid);
            Mock.Get(sut).Setup(v => v.Validate(It.IsAny<string>())).Returns(new StubValidationResult(isValid));

            bool happyPathCalled = false, unhappyPathCalled = false;

            // Act
            sut.Validate(testInstance)
               .Result(onValidationSuccess: result => happyPathCalled = true, onValidationFailed: result => unhappyPathCalled = true);

            // Assert
            Assert.Equal(expectedHappyPath, happyPathCalled);
            Assert.Equal(expectedUnhappyPath, unhappyPathCalled);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExerciseStubValidationResult(bool expected)
        {
            // Arrange
            var sut = new StubValidationResult(expected);

            // Act
            bool actual = sut.IsValid;

            // Assert
            Assert.Equal(expected, actual);
        }

        private class StubValidationResult : ValidationResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StubValidationResult"/> class.
            /// </summary>
            /// <param name="expected"></param>
            public StubValidationResult(bool expected)
            {
                IsValid = expected;
            }

            /// <summary>
            /// Returns true if ... is valid.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
            /// </value>
            public override bool IsValid { get; }
        }
    }
}
