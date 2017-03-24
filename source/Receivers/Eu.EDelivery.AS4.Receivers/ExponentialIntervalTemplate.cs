using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    /// Receive in a exponential interval <see cref="IntervalRequest"/> instances.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ExponentialIntervalTemplate<T> where T : IntervalRequest
    {
        private readonly IDictionary<DateTime, List<T>> _runSchedulePModes;
        private readonly List<T> _intervalRequests;
        private readonly Timer _timer;
        private bool _running;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialIntervalTemplate{T}"/> class.
        /// </summary>
        protected ExponentialIntervalTemplate()
        {
            _runSchedulePModes = new ConcurrentDictionary<DateTime, List<T>>();
            _intervalRequests = new List<T>();
            _timer = new Timer();
            _timer.Elapsed += TimerEnlapsed;
        }

        private void TimerEnlapsed(object sender, ElapsedEventArgs eventArgs)
        {
            _timer.Stop();

            if (!_running)
            {
                return;
            }

            List<T> intervalRequests = SelectAllRequestsForThisEvent(eventArgs);
            RemoveAllSelectedRequestsForThisEvent(eventArgs);

            WaitForAllRequests(intervalRequests);
            DetermineNextRuns(intervalRequests);
        }

        private List<T> SelectAllRequestsForThisEvent(ElapsedEventArgs eventArgs)
        {
            return _runSchedulePModes.Where(s => s.Key <= eventArgs.SignalTime).SelectMany(p => p.Value).ToList();
        }

        private void RemoveAllSelectedRequestsForThisEvent(ElapsedEventArgs eventArgs)
        {
            List<DateTime> keys = _runSchedulePModes.Keys.Where(k => k <= eventArgs.SignalTime).ToList();
            keys.ForEach(k => _runSchedulePModes.Remove(k));
        }

        private void WaitForAllRequests(IEnumerable<T> pmodeRequests)
        {
            var tasks = new List<Task>();

            foreach (T intervalRequest in pmodeRequests)
            {
                intervalRequest.CalculateNewInterval();

                tasks.Add(Task.Run(() => OnRequestReceived(intervalRequest)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void DetermineNextRuns(IEnumerable<T> intervalRequests)
        {
            DateTime currentTime = DateTime.Now;

            foreach (T intervalRequest in intervalRequests)
            {
                AddIntervalRequest(currentTime + intervalRequest.CurrentInterval, intervalRequest);
            }

            if (!_runSchedulePModes.Any()) return;

            _timer.Interval = CalculateNextTriggerInterval(currentTime);
            _timer.Start();
        }

        private void AddIntervalRequest(DateTime dateTime, T intervalRequest)
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
                _runSchedulePModes.Add(timeTrimmedOnSeconds, new List<T>());
            }

            _runSchedulePModes[timeTrimmedOnSeconds].Add(intervalRequest);
        }

        private double CalculateNextTriggerInterval(DateTime now)
        {
            DateTime firstRunDate = _runSchedulePModes.Min(t => t.Key);
            TimeSpan triggerTime = firstRunDate - now;

            return triggerTime.TotalMilliseconds <= 0 ? 1 : triggerTime.TotalMilliseconds;
        }

        /// <summary>
        /// <paramref name="intervalRequest"/> is received.
        /// </summary>
        /// <param name="intervalRequest"></param>
        /// <returns></returns>
        protected abstract Task OnRequestReceived(T intervalRequest);

        /// <summary>
        /// Add a interval request to the schedule.
        /// </summary>
        /// <param name="intervalRequest"></param>
        protected void AddIntervalRequest(T intervalRequest)
        {
            _intervalRequests.Add(intervalRequest);
        }

        protected void StartInterval()
        {
            _running = true;
            DetermineNextRuns(_intervalRequests);
        }

        /// <summary>
        /// Stop the 
        /// </summary>
        protected void StopInterval()
        {
            _running = false;
            _timer.Dispose();
        }
    }

    public class TestReceiver : ExponentialIntervalTemplate<PModeRequest>, IReceiver
    {
        private readonly IConfig _configuration;
        private Func<PModeRequest, Task<InternalMessage>> _messageCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReceiver"/> class.
        /// </summary>
        /// <param name="configuration">Configuration used to retrieve the pmodes.</param>
        public TestReceiver(IConfig configuration)
        {
            _configuration = configuration;
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
                base.AddIntervalRequest(new PModeRequest(pmode, setting["tmin"], setting["tmax"]));
            }
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Func<ReceivedMessage, CancellationToken, Task<InternalMessage>> messageCallback, CancellationToken cancellationToken)
        {
            _messageCallback = message =>
            {
                var receivedMessage = new ReceivedMessage(AS4XmlSerializer.ToStream(message.PMode));
                return messageCallback(receivedMessage, cancellationToken);
            };

            base.StartInterval();
        }

        /// <summary>
        /// Stop the receiver from pulling.
        /// </summary>
        public void StopReceiving()
        {
            base.StopInterval();
        }

        /// <summary>
        /// <paramref name="intervalRequest"/> is received.
        /// </summary>
        /// <param name="intervalRequest"></param>
        /// <returns></returns>
        protected override async Task OnRequestReceived(PModeRequest intervalRequest)
        {
            InternalMessage resultedMessage = await _messageCallback(intervalRequest);

            if (resultedMessage.AS4Message.IsUserMessage)
            {
                intervalRequest.ResetInterval();
            }
        }
    }
}
