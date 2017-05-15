using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="ReceivingProcessingModeValidator" />
    /// </summary>
    public class GivenReceivingProcessingModeValidatorFacts
    {
        [Fact]
        public void ValidatesValidReceivingPMode()
        {
            // Arrange
            var sut = new ReceivingProcessingModeValidator();
            ReceivingProcessingMode pmode = ValidReceivingPMode();

            // Act
            ValidationResult validationResult = sut.Validate(pmode);

            // Assert
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void InvalidReceivePMode_WithoutId()
        {
            TestInvalidateReceivingPMode(pmode => pmode.Id = null);
        }

        [Fact]
        public void InvalidReceivePMode_WithoutValidReceiptHandling()
        {
            TestInvalidateReceivingPMode(pmode => pmode.ReceiptHandling = new ReceiveReceiptHandling {SendingPMode = null});
        }

        [Fact]
        public void InvalidReceivingPMode_WithoutValidErrorHandling()
        {
            TestInvalidateReceivingPMode(pmode => pmode.ErrorHandling = new ReceiveErrorHandling {SendingPMode = null});
        }

        [Fact]
        public void InvalidReceivingPMode_WithoutValidDeliverMethodIfEnabled()
        {
            TestInvalidateReceivingPMode(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.DeliverMethod = new Method {Type = null};
                });
        }

        [Fact]
        public void InvalidReceivingPMode_WithoutValidPayloadMethodIfEnabled()
        {
            TestInvalidateReceivingPMode(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.PayloadReferenceMethod = new Method {Type = null};
                });
        }

        private static void TestInvalidateReceivingPMode(Action<ReceivingProcessingMode> arrangePMode)
        {
            // Arrange
            ReceivingProcessingMode pmode = ValidReceivingPMode();
            arrangePMode(pmode);

            IValidator<ReceivingProcessingMode> sut = new ReceivingProcessingModeValidator();

            // Act / Assert
            Assert.Throws<AS4Exception>(() => sut.Validate(pmode));
        }

        private static ReceivingProcessingMode ValidReceivingPMode()
        {
            var deliverMethod = new Method
            {
                Type = "deliver-type",
                Parameters = new List<Parameter> {new Parameter {Name = "parameter-name", Value = "parameter-value"}}
            };

            return new ReceivingProcessingMode
            {
                Id = "pmode-id",
                ReceiptHandling = new ReceiveReceiptHandling {SendingPMode = "receipt-pmode"},
                ErrorHandling = new ReceiveErrorHandling {SendingPMode = "error-pmode"},
                Deliver =
                    new Deliver
                    {
                        IsEnabled = true,
                        DeliverMethod = deliverMethod,
                        PayloadReferenceMethod = deliverMethod
                    }
            };
        }
    }
}