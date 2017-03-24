using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers.Pull;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// <see cref="IReceiver"/> implementation to pull exponentially for Pull Requests.
    /// </summary>
    public class PullRequestReceiver : ExponentialIntervalReceiver<PModePullRequest>
    {
        private readonly IConfig _configuration;

        private Func<PModePullRequest, Task<InternalMessage>> _messageCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReceiver"/> class.
        /// </summary>
        public PullRequestReceiver() : this(Config.Instance) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReceiver"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IConfig"/> implementation to collection PModes.</param>
        public PullRequestReceiver(IConfig configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public override void Configure(IEnumerable<Setting> settings)
        {
            foreach (Setting setting in settings)
            {
                if (!_configuration.ContainsSendingPMode(setting.Key)) continue;

                SendingProcessingMode pmode = _configuration.GetSendingPMode(setting.Key);
                var pullRequest = new PModePullRequest(pmode, setting["tmin"].AsTimeSpan(), setting["tmax"].AsTimeSpan());

                base.AddIntervalRequest(pullRequest);
            }
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public override void StartReceiving(
            Func<ReceivedMessage, CancellationToken, Task<InternalMessage>> messageCallback,
            CancellationToken cancellationToken)
        {
            _messageCallback = message =>
            {
                var receivedMessage = new ReceivedMessage(AS4XmlSerializer.ToStream(message.PMode));
                return messageCallback(receivedMessage, cancellationToken);
            };

            base.StartInterval();
        }

        /// <summary>
        /// <paramref name="intervalPullRequest"/> is received.
        /// </summary>
        /// <param name="intervalPullRequest"></param>
        /// <returns></returns>
        protected override async Task<bool> OnRequestReceived(PModePullRequest intervalPullRequest)
        {
            InternalMessage resultedMessage = await _messageCallback(intervalPullRequest);

            return resultedMessage.AS4Message.IsUserMessage;
        }
    }
}