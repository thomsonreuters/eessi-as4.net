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
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(@"A file path must be specified", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(fileMask))
            {
                throw new ArgumentException(@"A file mask must be specified", nameof(fileMask));
            }

            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), @"A batch size must be specified that > 0");
            }

            FilePath = filePath;
            FileMask = fileMask;
            BatchSize = batchSize;
            PollingInterval = pollingInterval;
        }
    }
}