using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using FluentValidation.Results;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    /// Interface to implement a pmode service
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IModular" />
    public interface IPmodeService : IModular
    {
        /// <summary>
        /// Gets the receiving names.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetReceivingNames();

        /// <summary>
        /// Get a list of receiving pmodes
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        Task<ReceivingBasePmode> GetReceivingByName(string name);

        /// <summary>
        /// Get a list of sending pmodes
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetSendingNames();

        /// <summary>
        /// Get a sending pmode by name
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        Task<SendingBasePmode> GetSendingByName(string name);

        /// <summary>
        /// Create a receiving pmode
        /// </summary>
        /// <param name="basePmode">The pmode to create</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a pmode with the supplied name already exists</exception>
        Task CreateReceiving(ReceivingBasePmode basePmode);

        /// <summary>
        /// Create sending pmode
        /// </summary>
        /// <param name="basePmode">The pmode to create.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a pmode with the supplied name already exists</exception>
        Task CreateSending(SendingBasePmode basePmode);

        /// <summary>
        /// Delete a receiving pmode
        /// </summary>
        /// <param name="name">The name of the pmode to delete.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException">Exception thrown when the pmode doesn't exist</exception>
        Task DeleteReceiving(string name);

        /// <summary>
        /// Delete a sending pmode
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException">Exception thrown when the pmode doesn't exist</exception>
        Task DeleteSending(string name);

        /// <summary>
        /// Update sending pmode
        /// </summary>
        /// <param name="basePmode">Date to update the sending pmode with</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a sending pmode with the supplied name already exists</exception>
        Task UpdateSending(SendingBasePmode basePmode, string originalName);

        /// <summary>
        /// Update receiving pmode
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a pmode with the supplied name already exists.</exception>
        Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName);
    }
}