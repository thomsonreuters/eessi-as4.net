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
using Xunit.Abstractions;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    public class GivenFileReceiverFacts : IDisposable
    {
        private readonly ITestOutputHelper _testLogger;
        private readonly string _watchedDirectory;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenFileReceiverFacts" /> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public GivenFileReceiverFacts(ITestOutputHelper outputHelper)
        {
            _testLogger = outputHelper;
            _watchedDirectory = Path.Combine(Path.GetTempPath(), "FileReceiverTest");

            if (Directory.Exists(_watchedDirectory) == false)
            {
                Directory.CreateDirectory(_watchedDirectory);
            }

            FileSystemUtils.ClearDirectory(_watchedDirectory);
        }

        [Fact]
        public void BlocksFileReceiverWhenFolderIsLocked()
        {
            CreateFileInDirectory("testfile.dat", _watchedDirectory);
            CreateFileInDirectory("file.lock", _watchedDirectory);
            var receiver = CreateFileReceiver();

            Assert.Empty(StartReceiving(receiver, TimeSpan.FromSeconds(10)));
            receiver.StopReceiving();
        }

        [Fact]
        public void DoesReceiveNonSystemFileTypes()
        {
            CreateFileInDirectory("testfile.dat", _watchedDirectory);

            var receiver = CreateFileReceiver();
            var receivedFiles = StartReceiving(receiver, TimeSpan.FromSeconds(10));
            receiver.StopReceiving();

            Assert.Single(receivedFiles);
            Assert.Equal("testfile", Path.GetFileNameWithoutExtension(receivedFiles.First()));
        }

        [Theory]
        [InlineData(".processing")]
        [InlineData(".exception")]
        [InlineData(".accepted")]
        [InlineData(".pending")]
        [InlineData(".lock")]
        public void DoesNotReceiveSystemFileTypes(string extension)
        {
            CreateFileInDirectory($"unwanted_testfile{extension}", _watchedDirectory);

            var receiver = CreateFileReceiver();
            var receivedFiles = StartReceiving(receiver, TimeSpan.FromSeconds(1));
            receiver.StopReceiving();

            Assert.Empty(receivedFiles);
        }

        private static IEnumerable<string> StartReceiving(FileReceiver receiver, TimeSpan timeout)
        {
            var signal = new ManualResetEvent(false);

            var receiveProcessor = new FileReceivedProcessor(signal);

            var tokenSource = new CancellationTokenSource();
            tokenSource.Token.Register(receiver.StopReceiving);
            tokenSource.CancelAfter(timeout);

            Task.Factory.StartNew(() => receiver.StartReceiving((m, c) => receiveProcessor.OnFileReceived(m, c), tokenSource.Token), tokenSource.Token);
            signal.WaitOne(timeout);

            return receiveProcessor.ReceivedFiles.ToList();
        }

        private static void CreateFileInDirectory(string fileName, string directory)
        {
            var fullPath = Path.Combine(directory, fileName);

            File.WriteAllText(fullPath, string.Empty);
        }

        private FileReceiver CreateFileReceiver()
        {
            var settings = new FileReceiverSettings(_watchedDirectory, "*.*", 20, TimeSpan.FromMilliseconds(100));

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
                using (var fs = (FileStream) m.UnderlyingStream)
                {
                    _receivedFiles.Add(fs.Name);
                }

                _waitSignal.Set();

                return Task.FromResult(new MessagingContext(m, MessagingContextMode.Receive));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            FileSystemUtils.ClearDirectory(_watchedDirectory);

            try
            {
                Directory.Delete(_watchedDirectory, true);
            }
            catch (Exception ex)
            {
                _testLogger.WriteLine(ex.ToString());
            }
        }
    }
}
