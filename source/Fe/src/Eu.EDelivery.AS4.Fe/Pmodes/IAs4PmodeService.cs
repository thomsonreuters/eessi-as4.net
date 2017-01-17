using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public interface IAs4PmodeService : IModular
    {
        Task<IEnumerable<string>> GetReceivingNames();
        Task<ReceivingPmode> GetReceivingByName(string name);
        Task<IEnumerable<string>> GetSendingNames();
        Task<SendingPmode> GetSendingByName(string name);
        Task CreateReceiving(ReceivingPmode pmode);
        Task CreateSending(SendingPmode pmode);
        Task DeleteReceiving(string name);
        Task DeleteSending(string name);
        Task UpdateSending(SendingPmode pmode, string originalName);
        Task UpdateReceiving(ReceivingPmode pmode, string originalName);
    }
}