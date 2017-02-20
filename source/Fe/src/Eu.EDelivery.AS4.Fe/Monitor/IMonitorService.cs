using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public interface IMonitorService
    {
        Task<MessageResult<Message>> GetInMessages(InMessageFilter filter);
        Task<MessageResult<Message>> GetOutMessages(OutMessageFilter filter);
        Task<MessageResult<ExceptionMessage>> GetInExceptions(string inMessageId);
        Task<MessageResult<ExceptionMessage>> GetOutExceptions(string outMessageId);
        string GetPmodeNumber(string pmode);
    }
}