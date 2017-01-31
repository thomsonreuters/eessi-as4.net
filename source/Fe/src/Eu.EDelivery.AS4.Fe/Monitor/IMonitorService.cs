using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public interface IMonitorService
    {
        Task<MessageResult<InException>> GetExceptions(InExceptionFilter filter);
        Task<MessageResult<Message>> GetInMessages(InMessageFilter filter);
        Task<MessageResult<Message>> GetOutMessages(OutMessageFilter filter);
    }
}