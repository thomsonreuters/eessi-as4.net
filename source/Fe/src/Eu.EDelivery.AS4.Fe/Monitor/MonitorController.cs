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
  }
}