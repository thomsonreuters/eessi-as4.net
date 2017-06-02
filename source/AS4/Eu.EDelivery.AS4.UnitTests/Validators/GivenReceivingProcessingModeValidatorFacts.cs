﻿using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="ReceivingProcessingModeValidator"/>
    /// </summary>
    public class GivenReceivingProcessingModeValidatorFacts
    {
        [Fact]
        public void PModeIsValid()
        {
           TestReceivingPModeValidation(pmode => { }, expected: true);
        }

        [Fact]
        public void PModeIsNotValid_IfNoReceiptHandlingIsPresent()
        {
            TestReceivingPModeValidation(pmode => pmode.ReceiptHandling = null);
        }

        [Fact]
        public void PModeIsNotValid_IfPModeIdIsMissing()
        {
            TestReceivingPModeValidation(pmode => pmode.Id = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoErrorHandlignIsPresent()
        {
            TestReceivingPModeValidation(pmode => pmode.ErrorHandling = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoReceiptHandlingSendingPModeIsPresent()
        {
            TestReceivingPModeValidation(pmode => pmode.ReceiptHandling.SendingPMode = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoErrorHandlignSendingPModeisPresent()
        {
            TestReceivingPModeValidation(pmode => pmode.ErrorHandling.SendingPMode = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoDeliverMethodIsPresentWhenDeliverIsEnabled()
        {
            TestReceivingPModeValidation(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.DeliverMethod = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoPayloadReferenceMethodIsPresentWhenDeliverIsEnabeld()
        {
            TestReceivingPModeValidation(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.PayloadReferenceMethod = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoDeliverParametersArePresentWhenDeliverIsEnabled()
        {
            TestReceivingPModeValidation(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.DeliverMethod.Parameters = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoPayloadReferenceParametersArePresentWhenDeliverIsEnabled()
        {
            TestReceivingPModeValidation(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.PayloadReferenceMethod.Parameters = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfInvalidParametersArePresentInDeliverMethodWhenDeliverIsEnabled()
        {
            TestReceivingPModeValidation(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.DeliverMethod.Parameters.Add(new Parameter {Name = null, Value = null});
                });
        }

        [Fact]
        public void PModeIsNotValid_IfInvalidParametersArePresentInPayloadReferenceMetodWhenDeliverIsEnabeld()
        {
            TestReceivingPModeValidation(
                pmode =>
                {
                    pmode.Deliver.IsEnabled = true;
                    pmode.Deliver.PayloadReferenceMethod.Parameters.Add(new Parameter {Name = null, Value = null});
                });
        }

        private static void TestReceivingPModeValidation(Action<ReceivingProcessingMode> arrangePMode, bool expected = false)
        {
            // Arrange
            ReceivingProcessingMode pmode = CreateValidPMode();
            arrangePMode(pmode);

            var sut = new ReceivingProcessingModeValidator();

            // Act
            ValidationResult result = sut.Validate(pmode);

            // Assert
            Assert.Equal(expected, result.IsValid);
        }

        private static ReceivingProcessingMode CreateValidPMode()
        {
            var method = new Method
            {
                Type = "deliver-type",
                Parameters = new List<Parameter> {new Parameter {Name = "parameter-name", Value = "parameter-value"}}
            };

            return new ReceivingProcessingMode
            {
                Id = "pmode-id",
                ReceiptHandling = new ReceiveReceiptHandling {SendingPMode = "send-pmode"},
                ErrorHandling = new ReceiveErrorHandling {SendingPMode = "send-pmode"},
                Deliver = new Deliver {IsEnabled = true, DeliverMethod = method, PayloadReferenceMethod = method}
            };
        }
    }
}