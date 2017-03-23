using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Fe.Monitor;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
  public interface IAs4PmodeService : IModular
  {
    Task<IEnumerable<string>> GetReceivingNames();
    Task<ReceivingBasePmode> GetReceivingByName(string name);
    Task<IEnumerable<string>> GetSendingNames();
    Task<SendingBasePmode> GetSendingByName(string name);
    Task CreateReceiving(ReceivingBasePmode basePmode);
    Task CreateSending(SendingBasePmode basePmode);
    Task DeleteReceiving(string name);
    Task DeleteSending(string name);
    Task UpdateSending(SendingBasePmode basePmode, string originalName);
    Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName);
  }
}