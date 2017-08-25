using System;

namespace Eu.EDelivery.AS4.Receivers
{
    public sealed class FileReceiverSettings
    {
        public string FilePath { get; }
        public string FileMask { get; }
        public int BatchSize { get; }
        public TimeSpan PollingInterval { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReceiverSettings"/> class.
        /// </summary>
        public FileReceiverSettings(string filePath, string fileMask, int batchSize, TimeSpan pollingInterval)
        {
            FilePath = filePath;
            FileMask = fileMask;
            BatchSize = batchSize;
            PollingInterval = pollingInterval;
        }
    }
}