using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Eu.EDelivery.AS4.PayloadService.Infrastructure
{
    /// <summary>
    /// Represents an ActionResult that when executed will write a stream to the response.
    /// </summary>
    /// <remarks>This class is almost identical to the FileStreamResult class, except that this implementation
    /// flushes the output-stream each time bytes are written to it.  This prevents possible OutOfMemoryException when
    /// larger files are being sent.</remarks>
    public class StreamedFileResult : FileResult
    {
        // Should the buffersize be made configurable (via ctor argument?) or should this class determine the
        // ideal buffersize itself (taking the length of the source-stream into consideration) and a certain max size ?
        private const int BufferSize = 4096;

        private readonly string _downloadFilename;
        private readonly Stream _stream;

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.AspNetCore.Mvc.FileResult" /> instance with
        /// the provided <paramref name="contentType" />.
        /// </summary>
        /// <param name="stream">The source stream which must be written to the Response body.</param>        
        /// <param name="downloadFilename">The filename that must be used when the consumer downloads the file.</param>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public StreamedFileResult(Stream stream, string downloadFilename, string contentType) : base(contentType)
        {
            _stream = stream;
            _downloadFilename = downloadFilename;
        }

        // TODO: override the ExecuteResult async method to allow async execution as well ?           

        public override void ExecuteResult(ActionContext context)
        {
            AssignHeadersTo(context.HttpContext.Response.Headers);

            // get chunks of data and write to the output stream
            WriteInChunksTo(context.HttpContext.Response.Body);
        }

        private void AssignHeadersTo(IHeaderDictionary headers)
        {
            var contentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = _downloadFilename
            };

            headers.Add("Content-Length", _stream.Length.ToString());
            headers.Add("Content-Disposition", contentDisposition.ToString());
        }

        private void WriteInChunksTo(Stream outputStream)
        {
            var buffer = new byte[BufferSize];

            try
            {
                while (true)
                {
                    int bytesRead = _stream.Read(buffer, 0, BufferSize);

                    if (bytesRead == 0)
                    {
                        // no more data that must be transmitted so stop here.
                        break;
                    }

                    outputStream.Write(buffer, 0, bytesRead);
                    outputStream.Flush();
                }
            }
            finally
            {
                _stream.Dispose();
            }
        }
    }
}