using System;
using System.Linq;
using Castle.Core.Internal;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to validate the state of a <see cref="DeliverMessage"/>
    /// </summary>
    public class DeliverMessageValidator : AbstractValidator<DeliverMessage>, IValidator<DeliverMessage>
    {
        private readonly ILogger _logger;

        public DeliverMessageValidator()
        {
            this._logger = LogManager.GetCurrentClassLogger();

            RulesForMessageInfo();
            RulesForPartyInfo();
            RulesForCollaborationInfo();
            RulesForPayloads();
        }

        /// <summary>
        /// Validate the given <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool IValidator<DeliverMessage>.Validate(DeliverMessage model)
        {
            ValidationResult validationResult = TryValidateDeliverMessage(model);
            if (validationResult == null) return false;

            if (validationResult.IsValid) return true;
                throw ThrowInvalidDeliverMessage(model, validationResult);
        }

        private ValidationResult TryValidateDeliverMessage(DeliverMessage model)
        {
            try
            {
                return base.Validate(model);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private AS4Exception ThrowInvalidDeliverMessage(DeliverMessage deliverMessage, ValidationResult result)
        {
            foreach (var e in result.Errors)
            {
                _logger.Error($"Deliver Message Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }
            
            string description = $"Deliver Message {deliverMessage.MessageInfo.MessageId} was invalid, see logging";
            this._logger.Error(description);
            
            return new AS4ExceptionBuilder().WithDescription(description).Build();
        }

        private void RulesForMessageInfo()
        {
            RuleFor(i => i.MessageInfo.MessageId).NotNull();
            RuleFor(i => i.MessageInfo.Mpc).NotNull();
        }

        private void RulesForPartyInfo()
        {
            RuleFor(i => i.PartyInfo.FromParty.PartyIds).NotNull();
            RuleForEach(i => i.PartyInfo.FromParty.PartyIds)
                .Must(i => i.Id != null && i.Type != null);

            RuleFor(i => i.PartyInfo.ToParty.PartyIds).NotNull();
            RuleForEach(i => i.PartyInfo.ToParty.PartyIds)
                .Must(i => i.Id != null && i.Type != null);
        }

        private void RulesForCollaborationInfo()
        {
            RuleFor(i => i.CollaborationInfo.AgreementRef.PModeId).NotNull();
            RuleFor(i => RuleForService(i)).NotNull();
            RuleFor(i => i.CollaborationInfo.Action).NotNull();
            RuleFor(i => i.CollaborationInfo.ConversationId).NotNull();
        }

        private string RuleForService(DeliverMessage deliverMessage)
        {
            return deliverMessage.CollaborationInfo.Service?.Value;
        }

        private void RulesForPayloads()
        {
            RuleForEach(p => p.Payloads)
                .Must(RuleForPayload);
        }

        private static bool RuleForPayload(Payload payload)
        {
            return RuleForPayloadDirectProperties(payload) && RuleForPayloadSchemas(payload);
        }

        private static bool RuleForPayloadDirectProperties(Payload i)
        {
            return i.Id != null && i.Location != null /*&& i.MimeType != null*/;
        }

        private static bool RuleForPayloadSchemas(Payload i)
        {
            if (i.Schemas == null || i.Schemas.Length <= 0)
            {
                return true;
            }

            return i.Schemas.All(s => !String.IsNullOrEmpty(s.Location));            
        }
    }
}