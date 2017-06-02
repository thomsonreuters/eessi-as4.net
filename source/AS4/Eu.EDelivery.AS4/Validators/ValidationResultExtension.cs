using System;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    public static class ValidationResultExtension
    {
        /// <summary>
        /// Logs the errors.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="logger">The logger.</param>
        public static void LogErrors(this ValidationResult result, ILogger logger)
        {
            foreach (ValidationFailure e in result.Errors)
            {
                logger.Error($"Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }
        }

        /// <summary>
        /// Results the specified happy path.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="happyPath">The happy path.</param>
        /// <param name="unhappyPath">The unhappy path.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void Result(this ValidationResult result, Action<ValidationResult> happyPath, Action<ValidationResult> unhappyPath)
        {
            if (result.IsValid)
            {
                happyPath(result);
            }
            else
            {
                unhappyPath(result);
            }
        }
    }
}
