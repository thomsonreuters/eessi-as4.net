using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers.Pull;
using Eu.EDelivery.AS4.Serialization;
using Timer = System.Timers.Timer;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// <see cref="IReceiver"/> implementation to pull exponentially for Pull Requests.
    /// </summary>
    public class ExponentialIntervalReceiver : IReceiver
    {
        private readonly IConfig _configuration;
        private readonly Timer _timer;
        private readonly IDictionary<DateTime, List<PModeRequest>> _runSchedulePModes;
        private readonly ICollection<PModeRequest> _pmodeRequests;

        private Func<PModeRequest, Task<InternalMessage>> _messageCallback;
        private bool _running;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialIntervalReceiver"/> class.
        /// </summary>
        public ExponentialIntervalReceiver() : this(Config.Instance) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialIntervalReceiver"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IConfig"/> implementation to collection PModes.</param>
        public ExponentialIntervalReceiver(IConfig configuration)
        {
            _configuration = configuration;
            _timer = new Timer();
            _runSchedulePModes = new ConcurrentDictionary<DateTime, List<PModeRequest>>();
            _pmodeRequests = new List<PModeRequest>();
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            foreach (Setting setting in settings)
            {
                SendingProcessingMode pmode = _configuration.GetSendingPMode(setting.Key);
                _pmodeRequests.Add(new PModeRequest(pmode, setting["tmin"], setting["tmax"]));
            }

            _timer.Elapsed += TimerEnlapsed;
        }

        private void TimerEnlapsed(object sender, ElapsedEventArgs eventArgs)
        {
            _timer.Stop();

            if (!_running)
            {
                return;
            }

            List<PModeRequest> pmodeRequests = SelectAllPModeRequestsForThisEvent(eventArgs);
            RemoveAllSelectedPModeRequestsForThisEvent(eventArgs);

            WaitForAllRequests(pmodeRequests);
            DetermineNextRuns(pmodeRequests);
        }

        private List<PModeRequest> SelectAllPModeRequestsForThisEvent(ElapsedEventArgs eventArgs)
        {
            return _runSchedulePModes.Where(s => s.Key <= eventArgs.SignalTime).SelectMany(p => p.Value).ToList();
        }

        private void RemoveAllSelectedPModeRequestsForThisEvent(ElapsedEventArgs eventArgs)
        {
            List<DateTime> keys = _runSchedulePModes.Keys.Where(k => k <= eventArgs.SignalTime).ToList();
            keys.ForEach(k => _runSchedulePModes.Remove(k));
        }

        private void WaitForAllRequests(IEnumerable<PModeRequest> pmodeRequests)
        {
            var tasks = new List<Task>();

            foreach (PModeRequest pmodeRequest in pmodeRequests)
            {
                pmodeRequest.CalculateNewInterval();

                tasks.Add(Task.Run(() => OnPModeReceived(pmodeRequest)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private async Task OnPModeReceived(PModeRequest pmodeRequest)
        {
            InternalMessage resultedMessage = await _messageCallback(pmodeRequest);

            if (resultedMessage.AS4Message.IsUserMessage)
            {
                pmodeRequest.ResetInterval();
            }
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(
            Func<ReceivedMessage, CancellationToken, Task<InternalMessage>> messageCallback,
            CancellationToken cancellationToken)
        {
            _messageCallback = message =>
            {
                var receivedMessage = new ReceivedMessage(AS4XmlSerializer.ToStream(message.PMode));
                return messageCallback(receivedMessage, cancellationToken);
            };

            _running = true;

            DetermineNextRuns(_pmodeRequests);
        }

        private void DetermineNextRuns(IEnumerable<PModeRequest> pmodeRequests)
        {
            DateTime currentTime = DateTime.Now;

            foreach (PModeRequest pmodeRequest in pmodeRequests)
            {
                AddPModeRequest(currentTime + pmodeRequest.CurrentInterval, pmodeRequest);
            }

            if (!_runSchedulePModes.Any()) return;

            _timer.Interval = CalculateNextTriggerInterval(currentTime);
            _timer.Start();
        }

        private void AddPModeRequest(DateTime dateTime, PModeRequest pmodeRequest)
        {
            var timeTrimmedOnSeconds = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second);

            if (_runSchedulePModes.ContainsKey(timeTrimmedOnSeconds) == false)
            {
                _runSchedulePModes.Add(timeTrimmedOnSeconds, new List<PModeRequest>());
            }

            _runSchedulePModes[timeTrimmedOnSeconds].Add(pmodeRequest);
        }

        private double CalculateNextTriggerInterval(DateTime now)
        {
            DateTime firstRunDate = _runSchedulePModes.Min(t => t.Key);
            TimeSpan triggerTime = firstRunDate - now;

            return triggerTime.TotalMilliseconds <= 0 ? 1 : triggerTime.TotalMilliseconds;
        }

        /// <summary>
        /// Stop the given receiver from pulling Pull Requests.
        /// </summary>
        public void StopReceiving()
        {
            _running = false;
            _timer.Dispose();
        }
    }
}