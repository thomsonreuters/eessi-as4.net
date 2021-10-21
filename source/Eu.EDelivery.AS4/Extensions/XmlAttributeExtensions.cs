using System;
using System.Xml;
using log4net;

namespace Eu.EDelivery.AS4.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="XmlAttribute"/> instance.
    /// </summary>
    public static class XmlAttributeExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
                bool isParsedCorrectly = TimeSpan.TryParse(timeSpanValue, out resultedTimeSpan);

                if (!isParsedCorrectly)
                {
                    Logger.Warn($"The given Attribute value: '{xmlAttribute.Value}' isn't parsed correctly to a TimeSpan");
                }
            }

            return resultedTimeSpan;
        }
    }
}
