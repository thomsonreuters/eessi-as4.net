#define CONCURRENT_IO
using log4net;
using System.IO;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Streaming
{
    public static class StreamExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Serialize to series of bytes.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public static byte[] ToBytes(this Stream contents)
        {
            StreamUtilities.MovePositionToStreamStart(contents);

            VirtualStream virtualStream =
                VirtualStream.Create(contents.CanSeek ? contents.Length : VirtualStream.ThresholdMax);

            if (contents.CanRead)
            {
                contents.CopyTo(virtualStream);
                return virtualStream.ToArray();
            }

            return new byte[0];
        }

        public const int DefaultCopyToFastBufferSize = 81_920;

        /// <summary>
        /// Copies the source stream to the target stream asynchronously
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <remarks>Reading from the source-stream and writing to the target stream occurs in parallel.</remarks>
        /// <returns></returns>
        public static async Task CopyToFastAsync(this Stream source, Stream target)
        {
            Logger.Info($"CopyToFastAsync: Start.");
            // Set the length of the targetstream upfront if we can do that.
            // This can improve performance significantly.
            if (source.CanSeek && target.CanSeek)
            {
                target.SetLength(target.Length + (source.Length - source.Position));
            }

#if CONCURRENT_IO

            // We're reading bufferSize bytes from the source-stream inside one half of the buffer
            // while the writeTask is writing the other half of the buffer to the target-stream.
            int bufferSize = DefaultCopyToFastBufferSize; 
            int ioBufferSize = bufferSize * 2;

            byte[] buffer = new byte[ioBufferSize];
            int curoff = 0;

            Logger.Info($"CopyToFastAsync: Read async from source : Start.");
            Task<int> readTask = source.ReadAsync(buffer, curoff, bufferSize);
            Logger.Info($"CopyToFastAsync: Read async from source : End.");
            Task writeTask = Task.CompletedTask;
            int len;

            Logger.Info($"CopyToFastAsync: while loop : Start.");
            while ((len = await readTask.ConfigureAwait(false)) != 0)
            {
                await writeTask.ConfigureAwait(false);
                writeTask = target.WriteAsync(buffer, curoff, len);

                curoff ^= bufferSize;
                readTask = source.ReadAsync(buffer, curoff, bufferSize);
            }
            Logger.Info($"CopyToFastAsync: while loop : End.");

            await writeTask.ConfigureAwait(false);
            Logger.Info($"CopyToFastAsync: writeTask ConfigureAwait : End.");
#else
            await source.CopyToAsync(target).ConfigureAwait(false);
#endif
            Logger.Info($"CopyToFastAsync: End.");
        }

    }
}
