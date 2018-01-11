using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AS4.ParserService.Infrastructure;
using AS4.ParserService.Models;
using AS4.ParserService.Services;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Swashbuckle.Swagger.Annotations;

namespace AS4.ParserService.Controllers
{
    /// <summary>
    /// Provides functionality to create an AS4 Message
    /// </summary>
    public class EncodeController : ApiController
    {
        /// <summary>
        /// Verify if the Encode service is up.
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get()
        {
            return Ok("AS4.NET Encode");
        }

        /// <summary>
        /// Encode the given payloads into an AS4 Message using the specified Processing Mode.
        /// </summary>       
        /// <param name="encodeInformation">An <see cref="EncodeMessageInfo"/> that contains all information that is required to create an AS4 Message.</param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, description: "When the Encode process succeeded, an EncodeResult that contains the AS4 Message is returned", type: typeof(EncodeResult))]
        [SwaggerResponse(HttpStatusCode.BadRequest, description: "When the given EncodeMessageInfo object does not contain a Sending PMode, a Bad Request is returned.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, description: "Something went wrong while creating the requested AS4 Message", type: typeof(Exception))]
        public async Task<IHttpActionResult> Post([FromBody] EncodeMessageInfo encodeInformation)
        {
            if (encodeInformation == null)
            {
                return BadRequest();
            }

            if (encodeInformation.SendingPMode == null)
            {
                return BadRequest();
            }

            encodeInformation.SigningCertificatePassword = CertificateInfoRetriever.RetrieveCertificatePassword(this.Request).SigningPassword;

            var service = new EncodeService();

            var result = await service.CreateAS4Message(encodeInformation);

            if (result == null)
            {
                return BadRequest();
            }

            if (result.Exception != null)
            {
                return InternalServerError(result.Exception);
            }

            return Ok(CreateEncodeResultFromContext(result));
        }

        private static EncodeResult CreateEncodeResultFromContext(MessagingContext context)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = SerializerProvider.Default.Get(context.AS4Message.ContentType);

                serializer.Serialize(context.AS4Message, stream, CancellationToken.None);

                var result = new EncodeResult
                {
                    SendToUrl = context.SendingPMode.PushConfiguration.Protocol.Url,
                    AS4Message = stream.ToArray(),
                    ContentType = context.AS4Message.ContentType.Replace("\"utf-8\"", "utf-8"),
                    EbmsMessageId = context.AS4Message.GetPrimaryMessageId()
                };

                return result;
            }
        }



    }


}
