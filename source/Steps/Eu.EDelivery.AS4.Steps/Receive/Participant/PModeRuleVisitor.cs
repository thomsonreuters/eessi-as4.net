using System.Collections.Generic;
using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive.Participant
{
    /// <summary>
    /// Interface used for testing
    /// </summary>
    internal interface IPModeRuleVisitor
    {
        void Visit(PModeParticipant participant);
    }

    /// <summary>
    /// Class to Provide <see cref="IPModeRule"/> implementations
    /// </summary>
    internal class PModeRuleVisitor : IPModeRuleVisitor
    {
        private readonly ILogger _logger;
        private readonly ICollection<IPModeRule> _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeRuleVisitor"/> class. 
        /// Create a new Visitor for the <see cref="PModeParticipant"/>
        /// </summary>
        public PModeRuleVisitor()
        {
            this._logger = LogManager.GetCurrentClassLogger();
            this._rules = new Collection<IPModeRule>
            {
                new PModeIdRule(),
                new PModePartyInfoRule(),
                new PModeUndefinedPartyInfoRule(),
                new PModeAgreementRefRule(),
                new PModeServiceActionRule(),
            };
        }

        /// <summary>
        /// Visits the <see cref="PModeParticipant"/>:
        /// apply Rules on the Participant
        /// </summary>
        /// <param name="participant"></param>
        public void Visit(PModeParticipant participant)
        {
            foreach (IPModeRule rule in this._rules)
            {
                int points = rule.DeterminePoints(participant.PMode, participant.UserMessage);
                this._logger.Debug(
                    $"PMode {participant.PMode.Id}: {points} Points determined for the {rule.GetType().Name}");

                participant.Points += points;
            }
        }
    }
}