using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using Eu.EDelivery.AS4.Steps.Receive.Rules;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Participant
{
    /// <summary>
    /// 
    /// </summary>
    internal class StubPModeRuleVisitor : IPModeRuleVisitor
    {
        private readonly ICollection<IPModeRule> _rules;
        private Action<PModeParticipant> _assertion;

        public StubPModeRuleVisitor()
        {
            this._rules = new Collection<IPModeRule>
            {
                new PModeIdRule(),
                new PModePartyInfoRule(),
                new PModeUndefindPartyInfoRule(),
                new PModeAgreementRefRule(),
                new PModeServiceActionRule()
            };
        }

        public void Visit(PModeParticipant participant)
        {
            foreach (IPModeRule rule in this._rules)
                    participant.Points += rule.DeterminePoints(participant.PMode, participant.UserMessage);

            this._assertion?.Invoke(participant);
        }

        public void SetAssert(Action<PModeParticipant> action)
        {
            this._assertion = action;
        }
    }
}
