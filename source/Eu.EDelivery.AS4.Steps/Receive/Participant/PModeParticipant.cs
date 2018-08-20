using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Participant
{
    /// <summary>
    /// PMode Participant Class act as a possible match for the Determine Receiving PMode Step 
    /// </summary>
    internal class PModeParticipant : IComparable<PModeParticipant>
    {
        public UserMessage UserMessage { get; }

        public ReceivingProcessingMode PMode { get; }

        public int Points { get; internal set; }

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
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            if (userMessage == null)
            {
                throw new ArgumentNullException(nameof(userMessage));
            }

            PMode = pmode;
            UserMessage = userMessage;
        }

        /// <summary>
        /// Use the Points as approach to compare two Participants
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(PModeParticipant other)
        {
            if (other.Points > Points)
            {
                return -1;
            }

            return other.Points == Points ? 0 : 1;
        }
    }
}