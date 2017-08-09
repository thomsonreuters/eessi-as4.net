using System;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;

namespace Eu.EDelivery.AS4.Model.PMode
{
    [Serializable]
    public class InvalidPModeException : Exception
    {
        public InvalidPModeException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPModeException"/> class.
        /// </summary>
        public InvalidPModeException(string message)
            : base(message)
        {
        }

        public InvalidPModeException(string message, ValidationResult validationResult) :
            base(validationResult.AppendValidationErrorsToErrorMessage(message))
        {
        }

        public InvalidPModeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}