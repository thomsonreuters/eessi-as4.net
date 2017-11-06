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
            const int bufferSize = 81920;
            const int ioCount = 40960;

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
        }
    }
}
