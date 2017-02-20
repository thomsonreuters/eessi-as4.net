using System.Threading.Tasks;
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
        [Route("inexceptions")]
        public async Task<IActionResult> GetInExceptions(string messageId)
        {
            return new OkObjectResult(await monitorService.GetInExceptions(messageId));
        }

        [HttpGet]
        [Route("outexceptions")]
        public async Task<IActionResult> GetOutExceptions(string messageId)
        {
            return new OkObjectResult(await monitorService.GetOutExceptions(messageId));
        }

        [HttpGet]
        [Route("inmessages")]
        public async Task<IActionResult> GetInMessages(InMessageFilter filter)
        {
            return new OkObjectResult(await monitorService.GetInMessages(filter));
        }

        [HttpGet]
        [Route("outmessages")]
        public async Task<IActionResult> GetOutMessages(OutMessageFilter filter)
        {
            return new OkObjectResult(await monitorService.GetOutMessages(filter));
        }
    }
}