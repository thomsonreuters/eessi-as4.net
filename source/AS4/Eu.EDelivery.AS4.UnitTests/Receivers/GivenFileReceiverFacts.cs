using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.TestUtils;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    public class GivenFileReceiverFacts : IDisposable
    {
        private readonly string _watchedDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenFileReceiverFacts"/> class.
        /// </summary>
        public GivenFileReceiverFacts()
        {
            _watchedDirectory = Path.Combine(Path.GetTempPath(), "FileReceiverTest");

            if (Directory.Exists(_watchedDirectory) == false)
            {
                Directory.CreateDirectory(_watchedDirectory);
            }
        }

        [Theory]
        [InlineData(".processing")]
        [InlineData(".exception")]
        [InlineData(".accepted")]
        [InlineData(".pending")]
        public void DoesNotReceiveCertainFileTypes(string extension)
        {
            var receiver = CreateFileReceiver();

            CreateFileInDirectory("testfile.dat", _watchedDirectory);
            var receivedFiles = StartReceiving(receiver);
            Assert.True(receivedFiles.Count() == 1);
            Assert.Equal("testfile", Path.GetFileNameWithoutExtension(receivedFiles.First()));

            FileSystemUtils.ClearDirectory(_watchedDirectory);

            CreateFileInDirectory($"unwanted_testfile{extension}", _watchedDirectory);
            receivedFiles = StartReceiving(receiver);
            Assert.False(receivedFiles.Any());
        }

        private static IEnumerable<string> StartReceiving(FileReceiver receiver)
        {
            var signal = new ManualResetEvent(false);

            var receiveProcessor = new FileReceivedProcessor(signal);

            Task.Factory.StartNew(() => receiver.StartReceiving((m, c) => receiveProcessor.OnFileReceived(m, c), CancellationToken.None));

            signal.WaitOne(TimeSpan.FromSeconds(1));
            receiver.StopReceiving();
            return receiveProcessor.ReceivedFiles;
        }

        private static void CreateFileInDirectory(string fileName, string directory)
        {
            var fullPath = Path.Combine(directory, fileName);

            var fs = File.Create(fullPath);
            fs.Close();
        }

        private FileReceiver CreateFileReceiver()
        {
            var settings = new FileReceiverSettings(_watchedDirectory, "*.*", 20, TimeSpan.FromSeconds(2));

            FileReceiver receiver = new FileReceiver();
            receiver.Configure(settings);

            return receiver;
        }

        private sealed class FileReceivedProcessor
        {
            private readonly List<string> _receivedFiles = new List<string>();
            private readonly ManualResetEvent _waitSignal;

            public IEnumerable<string> ReceivedFiles => _receivedFiles.ToArray();

            /// <summary>
            /// Initializes a new instance of the <see cref="FileReceivedProcessor"/> class.
            /// </summary>
            public FileReceivedProcessor(ManualResetEvent waitSignal)
            {
                _waitSignal = waitSignal;
            }

            public Task<MessagingContext> OnFileReceived(ReceivedMessage m, CancellationToken token)
            {
                using (var fileStream = m.UnderlyingStream as FileStream)
                {
                    _receivedFiles.Add(Path.GetFileName(fileStream?.Name));
                }

                _waitSignal.Set();

                return Task.FromResult(new MessagingContext(m, MessagingContextMode.Receive));
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            FileSystemUtils.ClearDirectory(_watchedDirectory);
            Directory.Delete(_watchedDirectory, true);
        }
    }
}
