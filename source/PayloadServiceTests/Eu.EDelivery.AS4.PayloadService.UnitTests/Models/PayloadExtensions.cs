using System.IO;
using Eu.EDelivery.AS4.PayloadService.Models;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Models
{
    /// <summary>
    /// Extensions made on the <see cref="Payload"/> class.
    /// </summary>
    internal static class PayloadExtensions
    {
        /// <summary>
        /// Deserialize the given <paramref name="payload"/>'s content.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static string DeserializeContent(this Payload payload)
        {
            using (var streamReader = new StreamReader(payload.Content))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
