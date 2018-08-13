using System.IO;
using System.Text;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Serialization
{
    /// <summary>
    /// Extensions made on a 'string'.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Serialize a given string content to a stream.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static Stream AsStream(this string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
    }
}
