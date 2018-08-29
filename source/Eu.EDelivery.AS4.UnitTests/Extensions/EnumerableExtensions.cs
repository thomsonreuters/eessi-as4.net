using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> xs)
        {
            return xs ?? Enumerable.Empty<T>();
        }
    }
}
