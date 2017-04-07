using System;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Infrastructure;
using Eu.EDelivery.AS4.PayloadService.Models;
using Eu.EDelivery.AS4.PayloadService.Persistance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.PayloadService.Controllers
{
    [Route("api/[controller]")]
    public class PayloadController : Controller
    {
        private readonly FilePayloadPersister _payloadPersistor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadController"/> class.
        /// </summary>
        public PayloadController(IHostingEnvironment environment)
        {
            _payloadPersistor = new FilePayloadPersister(environment);
        }

        /// <summary>
        /// Upload a given Multipart payload.
        /// </summary>
        /// <remarks>In this concept, only one file-content in the Request content is supported.</remarks>
        /// <returns></returns>
        [HttpPost]
        [Route("Upload")]
        [ProducesResponseType(typeof(UploadResult), (int) HttpStatusCode.OK)]
        [Produces("application/xml", "application/json")]
        public async Task<IActionResult> Upload()
        {
            (bool success, MultipartPayloadReader reader) result = MultipartPayloadReader.TryCreate(Request.Body, Request.ContentType);

            if (!result.success)
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            string payloadId = await UploadPayloadsWith(result.reader);
            return new OkObjectResult(new UploadResult(payloadId));
        }

        private async Task<string> UploadPayloadsWith(MultipartPayloadReader reader)
        {
            string payloadId = Guid.NewGuid().ToString();

            await reader.StartReading(
                async payload => payloadId = await _payloadPersistor.SavePayload(payload));

            return payloadId;
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(StreamedFileResult), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> Download([FromRoute] string id)
        {
            Payload payload = await _payloadPersistor.LoadPayload(id);

            if (payload.Equals(Payload.Null))
            {
                return NotFound();
            }

            return new StreamedFileResult(payload.Content, payload.Meta.OriginalFileName, "application/octet-stream");
        }
    }
}