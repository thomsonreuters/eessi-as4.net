using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
  public interface IMonitorService
  {
    Task<MessageResult<ExceptionMessage>> GetExceptions(ExceptionFilter filter);
    string GetPmodeNumber(string pmode);
    Task<MessageResult<Message>> GetRelatedMessages(Direction direction, string messageId);
    Task<MessageResult<Message>> GetMessages(MessageFilter filter);
  }
}