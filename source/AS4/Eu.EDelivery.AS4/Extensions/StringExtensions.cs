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

        public static T ToEnum<T>(this string x, T defaultValue = default(T)) where T : struct, IConvertible
        {
            return x != null 
                   && Enum.TryParse(x, ignoreCase: true, result: out T output)
                ? output
                : defaultValue;
        }
    }
}
