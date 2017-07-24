using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    /// As4 PMode source
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IModular" />
    public interface IAs4PmodeSource : IModular
    {
        /// <summary>
        /// Gets the receiving names.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetReceivingNames();
        /// <summary>
        /// Gets the name of the receiving by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Task<ReceivingBasePmode> GetReceivingByName(string name);
        /// <summary>
        /// Gets the sending names.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetSendingNames();
        /// <summary>
        /// Gets the name of the sending by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Task<SendingBasePmode> GetSendingByName(string name);
        /// <summary>
        /// Creates the receiving.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <returns></returns>
        Task CreateReceiving(ReceivingBasePmode basePmode);
        /// <summary>
        /// Deletes the receiving.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Task DeleteReceiving(string name);
        /// <summary>
        /// Deletes the sending.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Task DeleteSending(string name);
        /// <summary>
        /// Creates the sending.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <returns></returns>
        Task CreateSending(SendingBasePmode basePmode);
        /// <summary>
        /// Updates the sending.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        Task UpdateSending(SendingBasePmode basePmode, string originalName);
        /// <summary>
        /// Updates the receiving.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName);
        /// <summary>
        /// Gets the pmode number.
        /// </summary>
        /// <param name="pmodeString">The pmode string.</param>
        /// <returns></returns>
        string GetPmodeNumber(string pmodeString);
    }
}