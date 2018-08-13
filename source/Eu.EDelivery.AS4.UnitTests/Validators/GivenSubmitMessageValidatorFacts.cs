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
            ExerciseValidation(
                message => { }, 
                expectedValid: true);
        }

        [Fact]
        public void ThenSubmitMessageIsInvalidWithDuplicatePayloadId()
        {
            ExerciseValidation(
                message =>
                {
                    message.Payloads.First().Id = "same id";

                    Payload[] temp = message.Payloads;
                    Array.Resize(ref temp, 2);
                    temp[1] = message.Payloads.First();

                    message.Payloads = temp;
                },
                expectedValid: false);
        }

        private static void ExerciseValidation(Action<SubmitMessage> arrangeMessage, bool expectedValid)
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
                Payloads = new[] {new Payload("file:///")
                {
                    Schemas = new [] {new Schema("location")},
                    PayloadProperties = new [] {new PayloadProperty("name")}
                }}
            };
        }
    }
}