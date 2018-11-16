using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Submit;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validate the <see cref="SubmitMessage"/> Model
    /// </summary>
    internal class SubmitMessageValidator : AbstractValidator<SubmitMessage>
    {
        public static SubmitMessageValidator Instance = new SubmitMessageValidator();

        private SubmitMessageValidator()
        {
            RuleFor(s => s.PartyInfo.FromParty)
                .Must(p => p.Role != null)
                .When(s => s.PartyInfo?.FromParty != null)
                .WithMessage("SubmitMessage.FromParty.Role must be present when an <FromParty/> element is present");

            RuleFor(s => s.PartyInfo.ToParty)
                .Must(p => p.Role != null)
                .When(p => p.PartyInfo?.ToParty != null)
                .WithMessage("SubmitMessage.ToParty.Role must be present when an ToParty.ToPartyId element is present");

            RuleFor(s => s.Payloads)
                .Must(ps => ps.All(p => p?.PayloadProperties != null && p.PayloadProperties.All(x => x != null && !String.IsNullOrEmpty(x?.Name))))
                .When(s => s.HasPayloads
                           && s.Payloads.Any(p => p?.PayloadProperties != null && p.PayloadProperties.Any()))
                .WithMessage("SubmitMessage.Payloads[].PayloadProperties must all have a Name");

            RuleFor(s => s.Payloads)
                .Must(ps => ps.All(p => p?.Schemas != null && p.Schemas.All(x => x != null && x.Location != null)))
                .When(s => s.HasPayloads && s.Payloads.Any(p => p?.Schemas != null && p.Schemas.Any()))
                .WithMessage("SubmitMessage.Payloads[].Schemas must all have a Location");

            RuleFor(s => s.MessageProperties)
                .Must(ps => ps.All(p => !String.IsNullOrEmpty(p?.Value) && !String.IsNullOrEmpty(p?.Name)))
                .When(ps => ps.MessageProperties != null && ps.MessageProperties.Any())
                .WithMessage("SubmitMessage.MessageProperties must each have a 'Name' and 'Value'");

            RuleFor(s => s.Payloads)
                .Must(ps => ps.GroupBy(p => p.Id)
                              .All(p => p.Count() == 1))
                .When(s => s.Payloads != null)
                .WithMessage(
                    "SubmitMessage doesn't have unique <Payload/> elements, please make each payload unique by giving each a unique <Id/> element");
        }
    }
}