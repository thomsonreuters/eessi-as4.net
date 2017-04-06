using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Infrastructure;
using Eu.EDelivery.AS4.PayloadService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Eu.EDelivery.AS4.PayloadService.Controllers
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

            var reader = new MultipartReader(
                boundary: MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), lengthLimit: 100),
                stream: Request.Body);

            // In this concept, only one file-content in the Request content is supported.
            Guid id = Guid.NewGuid();
            MultipartSection section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        using (Stream targetStream = CreateTarget(id, contentDisposition.FileName))
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

        private Stream CreateTarget(Guid id, string fileName)
        {
            string contentDestinationPath = Path.Combine(_persistenceLocation, $"{id}");
            string metaDestinationPath = Path.Combine(_persistenceLocation, $"{id}.meta");

            // TODO: persistence should be configurable; filesystem or database.

            using (FileStream metaFile = System.IO.File.Create(metaDestinationPath))
            {
                using (var streamWriter = new StreamWriter(metaFile))
                {
                    streamWriter.WriteLine("originalfilename:" + Path.GetFileName(fileName.Trim('\"')));
                }
            }

            return System.IO.File.Create(contentDestinationPath);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(StreamedFileResult), (int)HttpStatusCode.OK)]
        public IActionResult Download([FromRoute] string id)
        {
            string filePath = Path.Combine(_persistenceLocation, $"{id}");
            string metaFilePath = Path.Combine(_persistenceLocation, $"{id}.meta");

            if (System.IO.File.Exists(filePath) == false)
            {
                return NotFound();
            }

            string filedownloadName = $"{id}.download";

            if (System.IO.File.Exists(metaFilePath))
            {
                PayloadMeta meta = MetaFileParser.Parse(metaFilePath);

                if (!string.IsNullOrWhiteSpace(meta?.OriginalFileName))
                {
                    filedownloadName = meta.OriginalFileName;
                }
            }

            return new StreamedFileResult(System.IO.File.OpenRead(filePath), filedownloadName, "application/octet-stream");
        }
    }
}