using System.IO;
using System.Text;

namespace Eu.EDelivery.AS4.Streaming
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Serialize to series of bytes.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream contents)
        {
            contents.Position = 0;

            using (var streamReader = new StreamReader(contents))
            {
                return Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
            }
        }
    }
}
