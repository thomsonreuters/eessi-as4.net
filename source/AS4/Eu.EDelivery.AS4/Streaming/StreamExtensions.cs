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

        public static async Task CopyToFastAsync(this Stream source, Stream target)
        {
            await CopyToFastAsync(source, target, 81_920);
        }

        /// <summary>
        /// Copies the source stream to the target stream asynchronously
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="bufferSize">The size of the buffer that must be applied when reading or writing during copy.</param>
        /// <remarks>Reading from the source-stream and writing to the target stream occurs in parallel.</remarks>
        /// <returns></returns>
        public static async Task CopyToFastAsync(this Stream source, Stream target, int bufferSize)
        {
#if CONCURRENT_IO

            // We have a buffer of bufferSize.
            // The ioCount constant is exactly the half of bufferSize.
            // We're reading ioCount bytes from the source-stream inside one half of the buffer
            // while the writeTask is writing the other half of the buffer to the target-stream.

            int ioBufferSize = bufferSize * 2;
            int actionBufferSize = bufferSize;

            byte[] buffer = new byte[ioBufferSize];
            int curoff = 0;

            Task<int> readTask = source.ReadAsync(buffer, curoff, actionBufferSize);
            Task writeTask = Task.CompletedTask;
            int len;

            while ((len = await readTask.ConfigureAwait(false)) != 0)
            {
                await writeTask.ConfigureAwait(false);
                writeTask = target.WriteAsync(buffer, curoff, len);

                curoff ^= actionBufferSize;
                readTask = source.ReadAsync(buffer, curoff, actionBufferSize);
            }

            await writeTask.ConfigureAwait(false);
#else
            await source.CopyToAsync(target).ConfigureAwait(false);
#endif
        }
    }
}
