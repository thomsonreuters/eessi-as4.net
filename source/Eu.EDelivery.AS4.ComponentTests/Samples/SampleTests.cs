using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Samples
{
    public class SampleTests : SampleTestTemplate
    {
        private const string DeliverPath = @".\messages\in",
                             NotifyReceiptPath = @".\messages\receipts",
                             NotifyErrorPath = @".\messages\errors",
                             GeneratedIdPattern = "*@*.xml",
                             SenderSettings = "sample_console_settings.xml",
                             ReceiverSettings = "sample_service_settings.xml";

        public SampleTests(WindowsServiceFixture fixture) : base(fixture) { }

        [Theory]
        [InlineData("01-sample-message.xml", SenderSettings, ReceiverSettings, "earth.jpg")]
        [InlineData("02-sample-message.xml", SenderSettings, ReceiverSettings, "earth.jpg", "xml-sample.xml")]
        [InlineData("03-sample-message.xml", SenderSettings, ReceiverSettings, "earth.jpg", "xml-sample.xml")]
        [InlineData("03-sample-message.xml", ReceiverSettings, SenderSettings, "earth.jpg", "xml-sample.xml")]
        public void Sending_Sample_Result_In_Delivered_And_Notified_Files(
            string file, 
            string consoleSettings,
            string serviceSettings,
            params string[] payloads)
        {
            // Arrange
            StartSenderMsh(consoleSettings);
            StartReceiverMsh(serviceSettings);

            // Act
            PutSample(file);

            // Wait some time till component has processed the sample
            WaitUntil(
                () => Directory.EnumerateFiles(DeliverPath, GeneratedIdPattern).Any()
                      && Directory.EnumerateFiles(NotifyReceiptPath).Any(),
                retryCount: 100,
                retryInterval: TimeSpan.FromSeconds(1));

            // TearDown
            SenderMsh.Dispose();

            // Assert
            Assert.All(payloads, m =>
            {
                AssertFiles(DeliverPath, m);

                string expected = GetOriginalPayload(m);
                string actual = Directory.EnumerateFiles(DeliverPath, m).First();

                Console.WriteLine(@"Expect send and delivered payload to have the same hash");
                Assert.Equal(MD5Hash(expected), MD5Hash(actual));
            });

            AssertFiles(DeliverPath, GeneratedIdPattern);
            AssertSignalMessages(NotifyReceiptPath, 1);
            AssertSignalMessages(NotifyErrorPath, 0);
        }

        private static void WaitUntil(Func<bool> predicate, int retryCount, TimeSpan retryInterval)
        {
            var count = 0;
            while (!predicate())
            {
                if (++count < retryCount)
                {
                    Console.WriteLine($@"No deliver/notify message found, wait {retryInterval:g}...");
                    Thread.Sleep(retryInterval);
                }
                else
                {
                    throw new TimeoutException("Test has been timed-out!");
                }
            }
        }

        private static void AssertFiles(string path, string searchPattern)
        {
            Console.WriteLine($@"Expect deliver dir: {path} to have files with extension: {searchPattern}");
            Assert.NotEmpty(Directory.EnumerateFiles(path, searchPattern));
        }

        private static void AssertSignalMessages(string path, int numberOfExpectedMessages)
        {
            Console.WriteLine($@"Expect notify dir: {path} to have {numberOfExpectedMessages} files");
            Assert.Equal(numberOfExpectedMessages, Directory.EnumerateFiles(path, GeneratedIdPattern).Count());
        }

        private static string GetOriginalPayload(string name)
        {
            return Directory.EnumerateFiles(@".\messages\attachments").First(a => Path.GetFileName(a) == name);
        }

        private static string MD5Hash(string fileName)
        {
            using (MD5 algorithm = MD5.Create())
            using (FileStream file = File.OpenRead(fileName))
            {
                return Encoding.Default.GetString(algorithm.ComputeHash(file));
            }
        }
    }
}