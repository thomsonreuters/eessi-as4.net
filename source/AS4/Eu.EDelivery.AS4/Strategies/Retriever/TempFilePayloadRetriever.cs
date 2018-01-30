using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Temporary <see cref="IPayloadRetriever"/> implementation that removes the file after retrieving.
    /// </summary>
    /// <seealso cref="IPayloadRetriever" />
    public class TempFilePayloadRetriever : IPayloadRetriever
    {
        public const string Key = "temp:///";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public async Task<Stream> RetrievePayloadAsync(string location)
        {
            VirtualStream virtualStr = VirtualStream.CreateVirtualStream();
            string absolutePath = location.Replace(Key, string.Empty);

            await CopyTempFileToStream(absolutePath, virtualStr);
            virtualStr.Position = 0;

            DeleteTempFile(absolutePath);

            return virtualStr;
        }

        private static async Task CopyTempFileToStream(string absolutePath, Stream str)
        {
            using (var fileStr = new FileStream(
                absolutePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read))
            {
                await fileStr.CopyToFastAsync(str);
            }
        }

        private static void DeleteTempFile(string absolutePath)
        {
            try
            {
                File.Delete(absolutePath);
                Logger.Debug("Removing temporary file at location: " + absolutePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
