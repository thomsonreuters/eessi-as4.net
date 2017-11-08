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

        /// <summary>
        /// Copies the source stream to the target stream asynchronously
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <remarks>Reading from the source-stream and writing to the target stream occurs in parallel.</remarks>
        /// <returns></returns>
        public static async Task CopyToFastAsync(this Stream source, Stream target)
        {
#if CONCURRENT_IO

            // We have a buffer of bufferSize.
            // The ioCount constant is exactly the half of bufferSize.
            // We're reading ioCount bytes from the source-stream inside one half of the buffer
            // while the writeTask is writing the other half of the buffer to the target-stream.

            const int bufferSize = 163_840;
            const int ioCount = 81_920;

            byte[] buffer = new byte[bufferSize];
            int curoff = 0;

            Task<int> readTask = source.ReadAsync(buffer, curoff, ioCount);
            Task writeTask = Task.CompletedTask;
            int len;

            while ((len = await readTask.ConfigureAwait(false)) != 0)
            {
                await writeTask.ConfigureAwait(false);
                writeTask = target.WriteAsync(buffer, curoff, len);

                curoff ^= ioCount;
                readTask = source.ReadAsync(buffer, curoff, ioCount);
            }

            await writeTask.ConfigureAwait(false);
#else
            await source.CopyToAsync(target).ConfigureAwait(false);
#endif
        }
    }
}
