using System;
using System.Threading.Tasks;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        [Route("simulate")]
        public async Task<IActionResult> Simulate()
        {
            var parser = new MultipartFormDataParser(Request.Body);

            var sendingPmode = parser.GetParameterValue("pmode");
            if (sendingPmode == null) throw new ArgumentNullException(nameof(sendingPmode), @"SendingPmode parameter is required!");

            return new OkObjectResult(await submitMessageCreator.Simulate(new MessagePayload
            {
                Files = parser.Files,
                SendingPmode = sendingPmode
            }));
        }
    }
}