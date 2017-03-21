using System;

namespace ExponentialIntervalReceiver
{
    public class PModeInformation
    {
        public string Name { get; }
        public TimeSpan MinInterval { get; }
        public TimeSpan MaxInterval { get; }

        public PModeInformation(string name, TimeSpan minInterval, TimeSpan maxInterval)
        {
            Name = name;
            MinInterval = minInterval;
            MaxInterval = maxInterval;
        }

        public override string ToString()
        {
            return $"{Name} - min: {MinInterval} - max: {MaxInterval}";
        }
    }
}