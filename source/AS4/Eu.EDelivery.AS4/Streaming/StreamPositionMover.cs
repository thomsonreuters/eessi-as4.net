using System;
using System.IO;
using MimeKit.IO;

namespace Eu.EDelivery.AS4.Streaming
{
    public static class StreamPositionMover
    {
        /// <summary>
        /// Resets the Position of the given Stream to 0.
        /// </summary>
        /// <remarks>This method takes care of special streams like NonCloseableStream and FilteredStream instances
        /// which are not seekable and thus cannot simply reset their Position to 0.</remarks>
        /// <param name="stream"></param>
        public static void MovePositionToStreamStart(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream streamToWorkOn = stream;

            if (stream is NonCloseableStream ncs)
            {
                streamToWorkOn = ncs.InnerStream;
            }
            else if (stream is FilteredStream fs)
            {
                streamToWorkOn = fs.Source;
            }

            if (streamToWorkOn.CanSeek && streamToWorkOn.Position != 0)
            {
                streamToWorkOn.Position = 0;
            }
        }
    }
}
