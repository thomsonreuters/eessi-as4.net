using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive.Participant
{
    /// <summary>
    /// PMode Participant Class act as a possible match for the Determine Receiving PMode Step 
    /// </summary>
    internal class PModeParticipant : IComparable<PModeParticipant>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public UserMessage UserMessage { get; set; }
        public ReceivingProcessingMode PMode { get; set; }
        public int Points { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeParticipant"/> class. 
        /// Create new Participant with two needed items to participate
        /// </summary>
        /// <param name="pmode">
        /// </param>
        /// <param name="userMessage">
        /// </param>
        public PModeParticipant(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            if (pmode == null) throw new ArgumentNullException(nameof(pmode));
            if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));

            this.PMode = pmode;
            this.UserMessage = userMessage;
        }

        /// <summary>
        /// Accept visitor to visit this Participant
        /// </summary>
        /// <param name="visitor"></param>
        public void Accept(IPModeRuleVisitor visitor)
        {            
            visitor.Visit(this);
            Logger.Debug($"Receiving PMode: {PMode.Id} has {Points} Points");
        }

        /// <summary>
        /// Use the Points as approach to compare two Participants
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(PModeParticipant other)
        {
            if (other.Points > this.Points) return -1;
            return other.Points == this.Points ? 0 : 1;
        }
    }
}