using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public interface IMonitorService
    {
        Task<MessageResult<ExceptionMessage>> GetOutExceptions(OutExceptionFilter filter);
        Task<MessageResult<Message>> GetInMessages(InMessageFilter filter);
        Task<MessageResult<ExceptionMessage>> GetInExceptions(InExceptionFilter filter);
        Task<MessageResult<Message>> GetOutMessages(OutMessageFilter filter);
        string GetPmodeNumber(string pmode);
    }
}