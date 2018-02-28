using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.UnitTests.Model;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests
{
    public class CustomProperty : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> class.
        /// </summary>
        public CustomProperty()
        {
            Arbitrary = new[] {typeof(Gens)};
        }
    }

    public static class Gens
    {
        public static Arbitrary<SignalMessage> Signals()
        {
            return Gen.Elements<SignalMessage>(new Receipt(), new FilledNRRReceipt(), new Error())
                      .ToArbitrary();
        }
    }
}
