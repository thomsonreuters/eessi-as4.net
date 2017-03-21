using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExponentialIntervalReceiver
{
    public class ExponentialIntervalReceiver
    {
        private readonly Dictionary<string, RunInfo> _pmodes;

        private readonly Dictionary<DateTime, List<PModeInformation>> _runSchedule = new Dictionary<DateTime, List<PModeInformation>>();

        private readonly System.Timers.Timer _timer;

        private Action<PModeInformation> _onPModeReceived;

        private bool _running;

        private class RunInfo
        {
            public PModeInformation PMode { get; }
            public TimeSpan CurrentInterval { get; private set; }

            private readonly TimeSpan _initialInterval;
            private readonly TimeSpan _maxInterval;
            private int _runs;

            private const double Factor = 1.75;

            public void CalculateNewInterval()
            {
                if (CurrentInterval >= _maxInterval)
                {
                    return;
                }

                long ticks = (long)(_initialInterval.Ticks * Math.Pow(Factor, _runs));

                CurrentInterval = TimeSpan.FromTicks(ticks);

                if (CurrentInterval > _maxInterval)
                {
                    CurrentInterval = _maxInterval;
                }

                _runs++;
            }

            public void Reset()
            {
                _runs = 0;
            }

            public RunInfo(PModeInformation pmode, TimeSpan minInterval, TimeSpan maxInterval)
            {
                PMode = pmode;                                
                _runs = 0;

                if (maxInterval < TimeSpan.FromSeconds(0))
                {
                    _maxInterval = TimeSpan.FromSeconds(1);
                }
                else
                {
                    _maxInterval = maxInterval;
                }

                if (minInterval < TimeSpan.FromSeconds(1))
                {                    
                    _initialInterval = TimeSpan.FromSeconds(1);
                }
                else
                {
                    _initialInterval = minInterval;                 
                }

                CurrentInterval = _initialInterval;              
            }
        }

        public ExponentialIntervalReceiver(IEnumerable<PModeInformation> pmodes)
        {
            _pmodes = pmodes.ToDictionary(p => p.Name, x => new RunInfo(x, x.MinInterval, x.MaxInterval));
            _timer = new System.Timers.Timer();
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            GetPModes(e.SignalTime);
        }    

        public void Start(Action<PModeInformation> onReceived)
        {
            _onPModeReceived = onReceived;
            _running = true;
            DetermineNextRuns(_pmodes.Values.Select(x => x.PMode));
        }

        public void Stop()
        {
            _running = false;
            _timer.Dispose();
        }

        private void GetPModes(DateTime date)
        {
            if (!_running)
            {
                return;
            }

            _timer.Stop();

            // Get all PModes whose rundate is less or equal then the given one.
            DateTime given = date;

            var retrievedPModes = _runSchedule.Where(k => k.Key <= given).SelectMany(p => p.Value).ToList();

            // Remove all the items from runDate that are being run now
            // This is to prevent that we retrieve messages for these pmodes while our current request is 
            // not finished yet, and to prevent that other pmodes (with slower intervals) suffer from starvation.
            var keys = _runSchedule.Keys.Where(k => k <= given).ToList();
            keys.ForEach(k => _runSchedule.Remove(k));

            var tasks = new List<Task>();

            // Right now, we can already calculate the next rundate for the given pmodes, we assume no results will be returned and the interval increases.
            foreach (var pmode in retrievedPModes)
            {                
                _pmodes[pmode.Name].CalculateNewInterval();
             
                Console.WriteLine(pmode);
                tasks.Add(Task.Run(() => _onPModeReceived(pmode)));
            }

            Task.WaitAll(tasks.ToArray());

            // Wait on all tasks to be finished

            DetermineNextRuns(retrievedPModes);

        }

        public void Reconfigure(ExecuteResult executeResult)
        {

            if (executeResult.Results)
            {
                // Poll this target immediatly again.
                _pmodes[executeResult.Name].Reset();                
            }

            Console.WriteLine($"Interval for {executeResult.Name} set to {_pmodes[executeResult.Name].CurrentInterval}");

        }

        private void DetermineNextRuns(IEnumerable<PModeInformation> pmodes)
        {
            var now = DateTime.Now;

            foreach (var pm in pmodes)
            {
                AddRunInformation(now + _pmodes[pm.Name].CurrentInterval, pm);
            }

            if (_runSchedule.Any())
            {
                var firstRunDate = this._runSchedule.Min(t => t.Key);

                Console.WriteLine($"Next runDate = {firstRunDate} - current date {DateTime.Now}");

                var triggerTime = firstRunDate - now;

                _timer.Interval = triggerTime.TotalMilliseconds <= 0 ? 1 : triggerTime.TotalMilliseconds;
                _timer.Start();
            }
        }

        private void AddRunInformation(DateTime dateTime, PModeInformation pmode)
        {
            // don't keep track of milliseconds.
            dateTime= new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

            if (_runSchedule.ContainsKey(dateTime) == false)
            {
                _runSchedule.Add(dateTime, new List<PModeInformation>());
            }
            _runSchedule[dateTime].Add(pmode);
        }
    }
}