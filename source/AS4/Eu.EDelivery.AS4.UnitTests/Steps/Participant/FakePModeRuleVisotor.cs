using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using Eu.EDelivery.AS4.Steps.Receive.Rules;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Participant
{
    /// <summary>
    /// Fake implementation fo the <see cref="IPModeRuleVisitor"/>.
    /// </summary>
    internal class FakePModeRuleVisotor : IPModeRuleVisitor
    {
        private readonly ICollection<IPModeRule> _rules;
        public FakePModeRuleVisotor()
        {
            _rules = new Collection<IPModeRule>
            {
                new PModeIdRule(),
                new PModePartyInfoRule(),
                new PModeUndefinedPartyInfoRule(),
                new PModeAgreementRefRule(),
                new PModeServiceActionRule()
            };
        }

        public void Visit(PModeParticipant participant)
        {
            foreach (IPModeRule rule in _rules)
            {
                participant.Points += rule.DeterminePoints(participant.PMode, participant.UserMessage);
            }
        }
    }
}