using System.ComponentModel;

namespace Eu.EDelivery.AS4.Fe.Tests.TestData
{
    [Description("TestReceiverWithOnlyDescription")]
    public class TestReceiverWithOnlyDescription : ITestReceiver
    {
        [Description("Name")]
        public string Name { get; set; }
    }
}