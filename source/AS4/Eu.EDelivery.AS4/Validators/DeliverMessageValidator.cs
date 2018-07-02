using System.Linq;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to validate the state of a <see cref="DeliverMessage"/>
    /// </summary>
    public class DeliverMessageValidator : AbstractValidator<DeliverMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverMessageValidator"/> class.
        /// </summary>
        public DeliverMessageValidator()
        {
            RulesForMessageInfo();
            RulesForPartyInfo();
            RulesForCollaborationInfo();
            RulesForPayloads();
        }

        private void RulesForMessageInfo()
        {
            RuleFor(i => i.MessageInfo.MessageId).NotNull();
            RuleFor(i => i.MessageInfo.Mpc).NotNull();
        }

        private void RulesForPartyInfo()
        {
            RuleFor(i => i.PartyInfo.FromParty.PartyIds).NotNull();
            RuleForEach(i => i.PartyInfo.FromParty.PartyIds).Must(i => i.Id != null);

            RuleFor(i => i.PartyInfo.ToParty.PartyIds).NotNull();
            RuleForEach(i => i.PartyInfo.ToParty.PartyIds).Must(i => i.Id != null);
        }

        private void RulesForCollaborationInfo()
        {
            RuleFor(i => i.CollaborationInfo.AgreementRef.PModeId).NotNull();
            RuleFor(i => RuleForService(i)).NotNull();
            RuleFor(i => i.CollaborationInfo.Action).NotNull();
            RuleFor(i => i.CollaborationInfo.ConversationId).NotNull();
        }

        private static string RuleForService(DeliverMessage deliverMessage)
        {
            return deliverMessage.CollaborationInfo.Service?.Value;
        }

        private void RulesForPayloads()
        {
            RuleForEach(p => p.Payloads).Must(RuleForPayload);
        }

        private static bool RuleForPayload(Payload payload)
        {
            return RuleForPayloadDirectProperties(payload) && RuleForPayloadSchemas(payload);
        }

        private static bool RuleForPayloadDirectProperties(Payload i)
        {
            return i.Id != null && i.Location != null;
        }

        private static bool RuleForPayloadSchemas(Payload i)
        {
            if (i.Schemas == null || i.Schemas.Length <= 0)
            {
                return true;
            }

            return i.Schemas.All(s => !string.IsNullOrEmpty(s.Location));
        }
    }
}