using System.ComponentModel;

namespace Eu.EDelivery.AS4.Fe.UnitTests.TestData
{
    [Description("TestReceiverWithOnlyDescription")]
    public class TestReceiverWithOnlyDescription : ITestReceiver
    {
        [Description("Name")]
        [Info("Name")]
        public string Name { get; set; }

        [Info("Test", attributes: new [] { "testattribute"})]
        public string Test { get; set; }
    }
}