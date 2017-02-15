using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Minder <see cref="IStep"/> implementation to create a Receipt
    /// </summary>
    [Obsolete("This Minder specific step should no longer be used.")]
    public class MinderNotifyReceiptStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderNotifyReceiptStep"/> class
        /// </summary>
        public MinderNotifyReceiptStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start notify receipt step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Notify Receipt on corner 2");

            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;

            SwitchParties(userMessage);
            AssignMessageId(userMessage);
            AssignNotifyAction(userMessage);
            AssignMessageProperties(userMessage);
            RemovePayloadInfo(userMessage);

            return StepResult.SuccessAsync(internalMessage);
        }

        private void SwitchParties(UserMessage userMessage)
        {
            Party fromparty = userMessage.Sender;
            Party toParty = userMessage.Receiver;

            userMessage.Sender = toParty;
            userMessage.Receiver = fromparty;
        }

        private static void AssignMessageId(UserMessage userMessage)
        {
            userMessage.MessageId = "notification@corner2";
        }

        private static void AssignNotifyAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Notify";
        }

        private static void AssignMessageProperties(UserMessage userMessage)
        {
            userMessage.MessageProperties.Add(new MessageProperty("SignalType", "Receipt"));
            userMessage.MessageProperties.Add(new MessageProperty("RefToMessageId", userMessage.MessageId));            
        }

        private static void RemovePayloadInfo(UserMessage userMessage)
        {
            userMessage.PayloadInfo.Clear();
        }
    }
}
