using System;
using System.IO;
using MimeKit.IO;

namespace Eu.EDelivery.AS4.Streaming
{
    public static class StreamUtilities
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

            Stream streamToWorkOn = GetStreamToWorkOn(stream);

            if (streamToWorkOn.CanSeek == false)
            {
                throw new InvalidOperationException("Unable to reset the Stream Position.  Stream is not seekable.");
            }

            if (streamToWorkOn.Position != 0)
            {
                streamToWorkOn.Position = 0;
            }
        }

        /// <summary>
        /// Tries to determine the length of the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <remarks>If the specified <paramref name="stream"/> is not seekable, -1 is returned.</remarks>
        /// <returns></returns>
        public static long GetStreamSize(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream streamToWorkOn = GetStreamToWorkOn(stream);

            if (streamToWorkOn.CanSeek == false)
            {
                return -1;
            }

            return streamToWorkOn.Length;
        }

        public static long DetermineOriginalSizeOfCompressedStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream streamToWorkOn = GetStreamToWorkOn(stream);

            if (streamToWorkOn.CanSeek == false)
            {
                return -1;
            }

            long originalPosition = streamToWorkOn.Position;

            try
            {
                byte[] fh = new byte[3];
                streamToWorkOn.Read(fh, 0, 3);
                if (fh[0] == 31 && fh[1] == 139 && fh[2] == 8) //If magic numbers are 31 and 139 and the deflation id is 8 then...
                {
                    byte[] ba = new byte[4];
                    streamToWorkOn.Seek(-4, SeekOrigin.End);
                    streamToWorkOn.Read(ba, 0, 4);
                    return BitConverter.ToInt32(ba, 0);
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
                return -1;
            }
            finally
            {
                streamToWorkOn.Position = originalPosition;
            }
        }

        private static Stream GetStreamToWorkOn(Stream stream)
        {
            Stream streamToWorkOn = stream;

            if (stream is NonCloseableStream ncs)
            {
                streamToWorkOn = ncs.InnerStream;
            }
            else if (stream is FilteredStream fs)
            {
                streamToWorkOn = fs.Source;
            }
            return streamToWorkOn;
        }
    }
}
