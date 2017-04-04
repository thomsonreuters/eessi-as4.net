using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using PayloadService.Infrastructure;
using PayloadService.Models;

namespace PayloadService.Controllers
{
    [Route("api/[controller]")]
    public class PayloadController : Controller
    {
        private readonly string _persistenceLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadController"/> class.
        /// </summary>
        public PayloadController(IHostingEnvironment environment)
        {
            _persistenceLocation = Path.Combine(environment.ContentRootPath, "Payloads");

            if (Directory.Exists(_persistenceLocation) == false)
            {
                Directory.CreateDirectory(_persistenceLocation);
            }
        }

        [HttpPost]
        [Route("Upload")]
        [ProducesResponseType(typeof(UploadResult), (int)HttpStatusCode.OK)]
        [Produces("application/xml", "application/json")]

        public async Task<IActionResult> Upload()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var reader = new MultipartReader(MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), 100),
                                             Request.Body);

            // In this concept, only one file-content in the Request content is supported.
            Guid id = Guid.NewGuid();

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var contentDestinationPath = Path.Combine(_persistenceLocation, $"{id}");
                        var metaDestinationPath = Path.Combine(_persistenceLocation, $"{id}.meta");

                        // TODO: persistence should be configurable; filesystem or database.

                        using (var metaFile = System.IO.File.Create(metaDestinationPath))
                        {
                            using (var sw = new StreamWriter(metaFile))
                            {
                                sw.WriteLine("originalfilename:" + Path.GetFileName(contentDisposition.FileName.Trim('\"')));
                            }
                        }

                        using (var targetStream = System.IO.File.Create(contentDestinationPath))
                        {
                            await section.Body.CopyToAsync(targetStream);
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return new OkObjectResult(new UploadResult(id.ToString()));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(StreamedFileResult), (int)HttpStatusCode.OK)]
        public IActionResult Download([FromRoute]string id)
        {
            // Below is a naive implementation which does not fully make use of streaming, resulting
            // in OutOfMemoryExceptions when retrieving larger payloads.

            ////var filePath = Path.Combine(_persistenceLocation, $"{id}");
            ////var metaFilePath = Path.Combine(_persistenceLocation, $"{id}.meta");

            ////if (System.IO.File.Exists(filePath) == false)
            ////{
            ////    return NotFound();
            ////}

            ////var result = new FileStreamResult(System.IO.File.OpenRead(filePath), "application/octet-stream");


            ////if (System.IO.File.Exists(metaFilePath))
            ////{
            ////    var meta = MetaFileParser.Parse(metaFilePath);

            ////    if (meta != null && !String.IsNullOrWhiteSpace(meta.OriginalFileName))
            ////    {
            ////        result.FileDownloadName = meta.OriginalFileName;
            ////    }
            ////}

            ////return result;

            // The solution below uses a custom made FileResult implementation which does not buffer the contents 
            // in memory, allowing larger payloads to be transmitted via streaming.
            // The implementation of the StreamedFileResult class is almost identical to the FileStreamResult class,
            // except that the StreamedFileResult flushes each time bytes are written to the target-stream.

            var filePath = Path.Combine(_persistenceLocation, $"{id}");
            var metaFilePath = Path.Combine(_persistenceLocation, $"{id}.meta");

            if (System.IO.File.Exists(filePath) == false)
            {
                return NotFound();
            }

            string filedownloadName = $"{id}.download";

            if (System.IO.File.Exists(metaFilePath))
            {
                var meta = MetaFileParser.Parse(metaFilePath);

                if (!String.IsNullOrWhiteSpace(meta?.OriginalFileName))
                {
                    filedownloadName = meta.OriginalFileName;
                }
            }

            var result = new StreamedFileResult(System.IO.File.OpenRead(filePath), filedownloadName, "application/octet-stream");

            return result;
        }
    }
}