using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="SubmitMessageValidator" />
    /// </summary>
    public class GivenSubmitMessageValidatorFacts
    {
        [Fact]
        public void ThenSubmitMessageIsValid()
        {
            TestInvalidValidation(message => { }, expectedValid: true);
        }

        [Fact]
        public void ThenSubmitMessageIsInvalidWithMissingPModeId()
        {
            TestInvalidValidation(message => message.Collaboration.AgreementRef.PModeId = null, expectedValid: false);
        }

        [Fact]
        public void ThenSubmitMessageIsInvalidWithMissingPayloadLocation()
        {
            TestInvalidValidation(message => message.Payloads.First().Location = null, expectedValid: false);
        }

        private static void TestInvalidValidation(Action<SubmitMessage> arrangeMessage, bool expectedValid)
        {
            // Arrange
            SubmitMessage message = CreateValidSubmitMessage();
            arrangeMessage(message);

            var sut = new SubmitMessageValidator();

            // Act
            ValidationResult result = sut.Validate(message);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }
        

        private static SubmitMessage CreateValidSubmitMessage()
        {
            return new SubmitMessage
            {
                Collaboration = {AgreementRef = {PModeId = "pmode-id"}},
                Payloads = new[] {new Payload("file:///")}
            };
        }
    }
}