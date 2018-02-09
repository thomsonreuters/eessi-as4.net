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
                             GeneratedIdPattern = "*@*.xml";

        [Theory]
        [InlineData("01-sample-message.xml", "earth.jpg")]
        [InlineData("02-sample-message.xml", "earth.jpg", "xml-sample.xml")]
        [InlineData("03-sample-message.xml", "earth.jpg", "xml-sample.xml")]
        public void Sending_Sample_Result_In_Delivered_And_Notified_Files(string file, params string[] payloads)
        {
            PutSample(file);

            // Wait some time till component has processed the sample
            Thread.Sleep(TimeSpan.FromSeconds(15));

            Assert.All(payloads, m =>
            {
                AssertFiles(DeliverPath, m);

                string expected = GetOriginalPayload(m);
                string actual = Directory.EnumerateFiles(DeliverPath, m).First();
                Assert.Equal(MD5Hash(expected), MD5Hash(actual));
            });

            AssertFiles(DeliverPath, GeneratedIdPattern);
            AssertFiles(NotifyReceiptPath, GeneratedIdPattern);
        }

        private static void AssertFiles(string path, string searchPattern)
        {
            Assert.NotEmpty(Directory.EnumerateFiles(path, searchPattern));
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