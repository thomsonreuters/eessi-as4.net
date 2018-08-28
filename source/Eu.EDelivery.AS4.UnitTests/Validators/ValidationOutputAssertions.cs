using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    public static class ValidationOutputAssertions
    {
        public static bool SpecifiedMethod(Method m)
        {
            bool specifiedType = !String.IsNullOrWhiteSpace(m?.Type);
            bool specifiedParams =
                m?.Parameters?.All(p => !String.IsNullOrWhiteSpace(p?.Name)
                                        && !String.IsNullOrWhiteSpace(p?.Value))
                ?? false;

            return specifiedType && specifiedParams;
        }
    }
}
