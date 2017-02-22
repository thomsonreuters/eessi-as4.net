using Castle.Core.Internal;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Submit;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validate the <see cref="SubmitMessage"/> Model
    /// </summary>
    public class SubmitMessageValidator : AbstractValidator<SubmitMessage>, IValidator<SubmitMessage>
    {
        private readonly ILogger _logger;

        public SubmitMessageValidator()
        {
            this._logger = LogManager.GetCurrentClassLogger();

            RuleFor(s => s.Collaboration).NotNull();
            RuleFor(s => s.Collaboration.AgreementRef).NotNull();
            RuleFor(s => s.Collaboration.AgreementRef.PModeId).NotEmpty();

            RuleFor(s => s.Payloads).SetCollectionValidator(new PayloadValidator());
        }

        /// <summary>
        /// Validate the given <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool IValidator<SubmitMessage>.Validate(SubmitMessage model)
        {
            ValidationResult validationResult = base.Validate(model);

            if (validationResult.IsValid) return true;
            throw ThrowInvalidSubmitMessageException(model, validationResult);
        }

        private AS4Exception ThrowInvalidSubmitMessageException(SubmitMessage submitMessage, ValidationResult result)
        {
            foreach (var e in result.Errors)
            {
                this._logger.Error($"Submit Message Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }

            string description = $"Submit Message {submitMessage.MessageInfo.MessageId} was invalid, see logging";
            this._logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(submitMessage.MessageInfo.MessageId)
                .Build();
        }
    }

    internal class PayloadValidator : AbstractValidator<Payload>
    {
        public PayloadValidator()
        {
            RuleFor(p => p.Location).NotEmpty();
        }
    }
}