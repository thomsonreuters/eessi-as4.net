using System;
using System.Xml;

namespace Eu.EDelivery.AS4.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="XmlAttribute"/> instance.
    /// </summary>
    public static class XmlAttributeExtensions
    {
        /// <summary>
        /// Convert the given <paramref name="xmlAttribute"/> to a valid <see cref="TimeSpan"/> instance.
        /// </summary>
        /// <param name="xmlAttribute"><see cref="XmlAttribute"/> to convert.</param>
        /// <returns></returns>
        public static TimeSpan AsTimeSpan(this XmlAttribute xmlAttribute)
        {
            TimeSpan resultedTimeSpan = default(TimeSpan);

            if (xmlAttribute != null)
            {
                string timeSpanValue = xmlAttribute.Value;
                TimeSpan.TryParse(timeSpanValue, out resultedTimeSpan);
            }

            return resultedTimeSpan;
        }
    }
}
