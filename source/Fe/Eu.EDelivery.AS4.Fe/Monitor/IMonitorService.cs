using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Monitor.Model;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public interface IMonitorService
    {
        Task<MessageResult<ExceptionMessage>> GetExceptions(ExceptionFilter filter);
        string GetPmodeNumber(string pmode);
        Task<MessageResult<Message>> GetRelatedMessages(Direction direction, string messageId);
        Task<MessageResult<Message>> GetMessages(MessageFilter filter);
        Task<Stream> DownloadMessageBody(Direction direction, string messageId);
        Task<byte[]> DownloadExceptionBody(Direction direction, string messageId);
    }
}