using System;

namespace Eu.EDelivery.AS4.Extensions
{
    public static class StringExtensions
    {
        public static TimeSpan AsTimeSpan(this string source)
        {
            return AsTimeSpan(source, default(TimeSpan));
        }

        public static TimeSpan AsTimeSpan(this string source, TimeSpan defaulTimeSpan)
        {            
            if (!String.IsNullOrWhiteSpace(source))
            {                
                bool isParsedCorrectly = TimeSpan.TryParse(source, out var resultedTimeSpan);

                if (isParsedCorrectly)
                {
                    return resultedTimeSpan;
                }
            }

            return defaulTimeSpan;
        }

        public static T ToEnum<T>(this string x) where T : struct, IConvertible
        {
            return (T) Enum.Parse(typeof(T), x, ignoreCase: true);
        }
    }
}
