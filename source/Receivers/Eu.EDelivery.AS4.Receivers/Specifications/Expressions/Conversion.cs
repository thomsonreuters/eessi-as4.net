using System.ComponentModel;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal static class Conversion
    {
        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public static object Convert(object property, string value)
        {
            if (value?.Equals("NULL") == true)
            {
                return null;
            }

            return TypeDescriptor.GetConverter(property).ConvertFrom(value);
        }
    }
}