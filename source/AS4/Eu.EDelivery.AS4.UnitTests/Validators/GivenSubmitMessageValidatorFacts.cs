using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Validators;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="SubmitMessageValidator"/>
    /// </summary>
    public class GivenSubmitMessageValidatorFacts
    {
        public class GivenValidArguments : GivenSubmitMessageValidatorFacts
        {
            [Fact]
            public void ThenSubmitMessageIsValid()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreateValidSubmitMessage();
                IValidator<SubmitMessage> validator = new SubmitMessageValidator();
                // Act
                bool isValid = validator.Validate(submitMessage);
                // Assert
                Assert.True(isValid);
            }
        }

        public class GivenInvalidArguments : GivenSubmitMessageValidatorFacts
        {
            [Theory]
            [InlineData(null)]
            public void ThenSubmitMessageIsInvalidWithMissingPModeId(string pmodeId)
            {
                // Arrange
                SubmitMessage submitMessage = base.CreateValidSubmitMessage();
                submitMessage.Collaboration.AgreementRef.PModeId = pmodeId;
                IValidator<SubmitMessage> validator = new SubmitMessageValidator();
                // Act
                Assert.Throws<AS4Exception>(() => validator.Validate(submitMessage));
            }

            [Theory]
            [InlineData(null)]
            public void ThenSubmitMessageIsInvalidWithMissingPayloadLocation(string location)
            {
                // Arrange
                SubmitMessage submitMessage = base.CreateValidSubmitMessage();
                submitMessage.Payloads.First().Location = location;
                IValidator<SubmitMessage> validator = new SubmitMessageValidator();
                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(submitMessage));
            }
        }

        protected SubmitMessage CreateValidSubmitMessage()
        {
            return new SubmitMessage
            {
                Collaboration = {AgreementRef = {PModeId = "pmode-id"}},
                Payloads = new[] {new Payload(location: "file:///"),}
            };
        }
    }
}