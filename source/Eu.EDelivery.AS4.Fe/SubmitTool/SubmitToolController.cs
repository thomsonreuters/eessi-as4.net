using System;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using HttpMultipartParser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    ///     Controller for the submit tool
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class SubmitToolController : Controller
    {
        private readonly ISubmitMessageCreator submitMessageCreator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SubmitToolController" /> class.
        /// </summary>
        /// <param name="submitMessageCreator">The submit message creator.</param>
        public SubmitToolController(ISubmitMessageCreator submitMessageCreator)
        {
            this.submitMessageCreator = submitMessageCreator;
        }

        /// <summary>
        ///     Post method to submit a message
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post()
        {
            var parser = new MultipartFormDataParser(Request.Body);
            int.TryParse(parser.GetParameterValue("messages"), out var messages);

            var sendingPmode = parser.GetParameterValue("pmode");
            if (sendingPmode == null) throw new ArgumentNullException(nameof(sendingPmode), @"SendingPmode parameter is required!");

            await submitMessageCreator.CreateSubmitMessages(new MessagePayload
            {
                Files = parser.Files,
                SendingPmode = sendingPmode,
                NumberOfSubmitMessages = messages == 0 ? 1 : messages
            });

            return Ok();
        }
    }
}