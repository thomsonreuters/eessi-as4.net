using System;
using System.IO;

namespace Eu.EDelivery.AS4.Repositories
{
    internal class AS4MessageBodyFileRetriever : IAS4MessageBodyRetriever
    {
        /// <summary>
        /// Loads a <see cref="Stream"/> at a given stored <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream"/> is stored.</param>
        /// <returns></returns>
        public Stream LoadAS4MessageStream(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            string absoluteLocation = SubstringWithoutFileUri(location);

            if (!File.Exists(absoluteLocation))
            {
                return null;
            }

            return File.OpenRead(absoluteLocation);
        }

        private static string SubstringWithoutFileUri(string location)
        {
            if (location.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                return location.Substring("file:///".Length);
            }

            return location;
        }
    }

    public interface IAS4MessageBodyRetriever
    {
        /// <summary>
        /// Loads a <see cref="Stream"/> at a given stored <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream"/> is stored.</param>
        /// <returns></returns>
        Stream LoadAS4MessageStream(string location);
    }
}