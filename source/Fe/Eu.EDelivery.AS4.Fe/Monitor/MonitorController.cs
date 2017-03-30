using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Monitor.Model;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    [Route("api/[controller]")]
    public class MonitorController : Controller
    {
        private readonly IMonitorService monitorService;

        public MonitorController(IMonitorService monitorService)
        {
            this.monitorService = monitorService;
        }

        [HttpGet]
        [Route("exceptions")]
        public async Task<IActionResult> GetInExceptions(ExceptionFilter filter)
        {
            return new OkObjectResult(await monitorService.GetExceptions(filter));
        }

        [HttpGet]
        [Route("messages")]
        public async Task<IActionResult> GetMessages(MessageFilter filter)
        {
            return new OkObjectResult(await monitorService.GetMessages(filter));
        }

        [HttpGet]
        [Route("relatedmessages")]
        public async Task<IActionResult> GetRelatedMessages(Direction direction, string messageId)
        {
            return new OkObjectResult(await monitorService.GetRelatedMessages(direction, messageId));
        }

        [HttpGet]
        [Route("messagebody")]
        public async Task<FileContentResult> GetMessageBody(Direction direction, string messageId)
        {
            return File(await monitorService.DownloadMessageBody(direction, messageId), "application/txt");
        }

        [HttpGet]
        [Route("exceptionbody")]
        public async Task<FileContentResult> GetExceptionBody(Direction direction, string messageId)
        {
            return File(await monitorService.DownloadExceptionBody(direction, messageId), "application/txt");
        }
    }
}