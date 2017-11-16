#define CONCURRENT_IO
using System.IO;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Streaming
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Serialize to series of bytes.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public static byte[] ToBytes(this Stream contents)
        {
            StreamPositionMover.MovePositionToStreamStart(contents);

            VirtualStream virtualStream =
                VirtualStream.CreateVirtualStream(contents.CanSeek ? contents.Length : VirtualStream.ThresholdMax);

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
            // Set the length of the targetstream upfront if we can do that.
            // This can improve performance significantly.
            if (source.CanSeek && target.CanSeek)
            {
                target.SetLength(target.Length + (source.Length - source.Position));
            }

#if CONCURRENT_IO

            // We're reading bufferSize bytes from the source-stream inside one half of the buffer
            // while the writeTask is writing the other half of the buffer to the target-stream.
            int bufferSize = DetermineOptimalBufferSize(source);
            int ioBufferSize = bufferSize * 2;

            byte[] buffer = new byte[ioBufferSize];
            int curoff = 0;

            Task<int> readTask = source.ReadAsync(buffer, curoff, bufferSize);
            Task writeTask = Task.CompletedTask;
            int len;

            while ((len = await readTask.ConfigureAwait(false)) != 0)
            {
                await writeTask.ConfigureAwait(false);
                writeTask = target.WriteAsync(buffer, curoff, len);

                curoff ^= bufferSize;
                readTask = source.ReadAsync(buffer, curoff, bufferSize);
            }

            await writeTask.ConfigureAwait(false);
#else
            await source.CopyToAsync(target).ConfigureAwait(false);
#endif
        }

        private static int DetermineOptimalBufferSize(Stream sourceStream)
        {
            if (sourceStream.CanSeek == false)
            {
                return DefaultCopyToFastBufferSize;
            }

            if (sourceStream.Length < DefaultCopyToFastBufferSize)
            {
                int factor = (int)sourceStream.Length / (4096 * 2);

                if (factor == 0)
                {
                    return 4096;
                }

                return 4096 * factor;
            }

            return DefaultCopyToFastBufferSize;
        }
    }
}
