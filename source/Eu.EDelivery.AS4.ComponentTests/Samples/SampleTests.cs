using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Samples
{
    public class SampleTests : SampleTestTemplate
    {
        [Theory]
        [InlineData("01-sample-message.xml", "earth.jpg", "*@*.xml")]
        [InlineData("02-sample-message.xml", "earth.jpg", "xml-sample.xml", "*@*.xml")]
        [InlineData("03-sample-message.xml", "earth.jpg", "xml-sample.xml", "*@*.xml")]
        public void Sending_Sample_Result_In_Delivered_And_Notified_Files(string file, params string[] deliverMessages)
        {
            PutSample(file);

            // Wait some time till component has processed the sample
            Thread.Sleep(TimeSpan.FromSeconds(15));

            Assert.All(deliverMessages, m => AssertFiles(@".\messages\in", m));
            AssertFiles(@".\messages\receipts", "*@*.xml");
        }

        private static void AssertFiles(string path, string searchPattern)
        {
            Assert.NotEmpty(Directory.EnumerateFiles(path, searchPattern));
        }
    }
}