using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace PayloadService.Infrastructure
{
    public class StreamedFileResult : FileResult
    {
        private const int BufferSize = 10000;

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.AspNetCore.Mvc.FileResult" /> instance with
        /// the provided <paramref name="contentType" />.
        /// </summary>
        /// <param name="contentType">The Content-Type header of the response.</param>
        public StreamedFileResult(Stream fileStream, string fileName, string contentType) : base(contentType)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            FileStream = fileStream;
            _fileName = fileName;
        }

        public Stream FileStream { get; }

        private readonly string _fileName;

        // TODO: override the ExecuteResult async method to allow async execution as well.            

        public override void ExecuteResult(ActionContext context)
        {            
            ContentDispositionHeaderValue contentDisposition = new ContentDispositionHeaderValue("attachment");
            contentDisposition.FileName = _fileName;

            context.HttpContext.Response.Headers.Add("Content-Length", FileStream.Length.ToString());
            context.HttpContext.Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

            // get chunks of data and write to the output stream
            Stream outputStream = context.HttpContext.Response.Body;
            using (FileStream)
            {
                byte[] buffer = new byte[BufferSize];

                while (true)
                {
                    int bytesRead = FileStream.Read(buffer, 0, BufferSize);

                    if (bytesRead == 0)
                    {
                        // no more data that must be transmitted so stop here.
                        break;
                    }

                    outputStream.Write(buffer, 0, bytesRead);
                    outputStream.Flush();
                }
            }
        }
    }
}