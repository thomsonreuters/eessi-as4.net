using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
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
        private Func<ReceivedMessage, CancellationToken, Task<InternalMessage>> _onPModeReceived;
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

            PModeRequests = new List<PModeRequest>();
        }

        public ICollection<PModeRequest> PModeRequests { get; set; }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            foreach (Setting setting in settings)
            {
                SendingProcessingMode pmode = _configuration.GetSendingPMode(setting.Key);
                PModeRequests.Add(new PModeRequest(pmode, setting["tmin"], setting["tmax"]));
            }

            _timer.Elapsed += TimerEnlapsed;
        }

        private void TimerEnlapsed(object sender, ElapsedEventArgs eventArgs)
        {
            _timer.Stop();

            IEnumerable<PModeRequest> pmodeRequests = _runSchedulePModes
                .Where(s => s.Key <= eventArgs.SignalTime)
                .SelectMany(p => p.Value);

            List<DateTime> keys = _runSchedulePModes.Keys.Where(k => k <= eventArgs.SignalTime).ToList();
            keys.ForEach(k => _runSchedulePModes.Remove(k));

            var tasks = new List<Task>();

            foreach (PModeRequest pmodeRequest in pmodeRequests)
            {
                pmodeRequest.CalculateNewInterval();

                Console.WriteLine(pmodeRequest);
                tasks.Add(Task.Run(() => _onPModeReceived(new ReceivedPullMessage(Stream.Null, Constants.ContentTypes.Soap, pmodeRequest.PMode), CancellationToken.None)));
            }

            Task.WaitAll(tasks.ToArray());

            DetermineNextRuns(pmodeRequests);
        }

        private void DetermineNextRuns(IEnumerable<PModeRequest> pmodeRequests)
        {
            if (!_running)
            {
                return;
            }

            DateTime now = DateTime.Now;

            foreach (PModeRequest pmodeRequest in pmodeRequests)
            {
                AddPModeRequest(now + pmodeRequest.CurrentInterval, pmodeRequest);
            }

            if (!_runSchedulePModes.Any()) return;

            DateTime firstRunDate = this._runSchedulePModes.Min(t => t.Key);
            Console.WriteLine($"Next runDate = {firstRunDate} - current date {DateTime.Now}");

            TimeSpan triggerTime = firstRunDate - now;
            _timer.Interval = triggerTime.TotalMilliseconds <= 0 ? 1 : triggerTime.TotalMilliseconds;
            _timer.Start();
        }

        private void AddPModeRequest(DateTime dateTime, PModeRequest pmodeRequest)
        {
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

            if (_runSchedulePModes.ContainsKey(dateTime) == false)
            {
                _runSchedulePModes.Add(dateTime, new List<PModeRequest>());
            }

            _runSchedulePModes[dateTime].Add(pmodeRequest);
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
            _onPModeReceived = messageCallback;
            _running = true;
            DetermineNextRuns(PModeRequests);
        }

        public void StopReceiving()
        {
            _running = false;
            _timer.Dispose();
        }

        public class PModeRequest
        {
            private const double Factor = 1.75;

            private readonly TimeSpan _minInterval;
            private readonly TimeSpan _maxInterval;
           
            private int _runs;

            public TimeSpan CurrentInterval { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PModeRequest"/> class.
            /// </summary>
            /// <param name="pmode">The pmode.</param>
            /// <param name="minInterval">The min Interval.</param>
            /// <param name="maxInterval">The max Interval.</param>
            public PModeRequest(SendingProcessingMode pmode, XmlAttribute minInterval, XmlAttribute maxInterval)
            {
                PMode = pmode;
                _minInterval = TimeSpan.Parse(minInterval.Value);
                _maxInterval = TimeSpan.Parse(maxInterval.Value);
            }

            public SendingProcessingMode PMode { get; }

            /// <summary>
            /// Reset the PMode Request.
            /// </summary>
            public void Reset()
            {
                _runs = 0;
            }

            /// <summary>
            /// Recalculate when the Request must be resend.
            /// </summary>
            public void CalculateNewInterval()
            {
                if (CurrentInterval >= _maxInterval)
                {
                    return;
                }

                var ticks = (long)(_minInterval.Ticks * Math.Pow(Factor, _runs));

                CurrentInterval = TimeSpan.FromTicks(ticks);

                if (CurrentInterval > _maxInterval)
                {
                    CurrentInterval = _maxInterval;
                }

                _runs++;
            }
        }
    }
}