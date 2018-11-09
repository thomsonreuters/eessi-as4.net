using System;

namespace Eu.EDelivery.AS4.Extensions
{
    internal static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="dateTime"/> to a offset representation.
        /// </summary>
        /// <param name="dateTime">The date to convert.</param>
        internal static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                ? DateTimeOffset.Now
                : new DateTimeOffset(dateTime);
        }
    }
}
