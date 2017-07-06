using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    /// Manage pmodes
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Pmodes.IPmodeService" />
    public class PmodeService : IPmodeService
    {
        private readonly IAs4PmodeSource source;

        /// <summary>
        /// Initializes a new instance of the <see cref="PmodeService"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public PmodeService(IAs4PmodeSource source)
        {
            this.source = source;
        }

        /// <summary>
        /// Gets the receiving names.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetReceivingNames()
        {
            return await source.GetReceivingNames();
        }

        /// <summary>
        /// Get a list of receiving pmodes
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        public async Task<ReceivingBasePmode> GetReceivingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetReceivingByName(name);
        }

        /// <summary>
        /// Get a list of sending pmodes
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetSendingNames()
        {
            return await source.GetSendingNames();
        }

        /// <summary>
        /// Get a sending pmode by name
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        public async Task<SendingBasePmode> GetSendingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetSendingByName(name);
        }

        /// <summary>
        /// Create a receiving pmode
        /// </summary>
        /// <param name="basePmode">The pmode to create</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a pmode with the supplied name already exists</exception>
        public async Task CreateReceiving(ReceivingBasePmode basePmode)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            var exists = await source.GetReceivingByName(basePmode.Name);
            if (exists != null) throw new AlreadyExistsException($"BasePmode with name {basePmode.Name} already exists.");
            await source.CreateReceiving(basePmode);
        }

        /// <summary>
        /// Create sending pmode
        /// </summary>
        /// <param name="basePmode">The pmode to create.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a pmode with the supplied name already exists</exception>
        public async Task CreateSending(SendingBasePmode basePmode)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            var exists = await source.GetSendingByName(basePmode.Name);
            if (exists != null) throw new AlreadyExistsException($"BasePmode with name {basePmode.Name} already exists.");
            await source.CreateSending(basePmode);
        }

        /// <summary>
        /// Delete a receiving pmode
        /// </summary>
        /// <param name="name">The name of the pmode to delete.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException">Exception thrown when the pmode doesn't exist</exception>
        public async Task DeleteReceiving(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var exists = await source.GetReceivingByName(name);
            if (exists == null) throw new NotFoundException($"BasePmode with name {name} doesn't exist");
            await source.DeleteReceiving(name);
        }

        /// <summary>
        /// Delete a sending pmode
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException">Exception thrown when the pmode doesn't exist</exception>
        public async Task DeleteSending(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var exists = await source.GetSendingByName(name);
            if (exists == null) throw new NotFoundException($"BasePmode with name {name} doesn't exist");
            await source.DeleteSending(name);
        }

        /// <summary>
        /// Update sending pmode
        /// </summary>
        /// <param name="basePmode">Date to update the sending pmode with</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a sending pmode with the supplied name already exists</exception>
        public async Task UpdateSending(SendingBasePmode basePmode, string originalName)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (basePmode.Name != originalName)
            {
                var newExists = await GetSendingByName(basePmode.Name);
                if (newExists != null) throw new AlreadyExistsException($"BasePmode with {originalName} already exists");
            }

            await source.UpdateSending(basePmode, originalName);
        }

        /// <summary>
        /// Update receiving pmode
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Exception thrown when a pmode with the supplied name already exists.</exception>
        public async Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (basePmode.Name != originalName)
            {
                var newExists = await GetReceivingByName(basePmode.Name);
                if (newExists != null) throw new AlreadyExistsException($"BasePmode with {originalName} already exists");
            }

            await source.UpdateReceiving(basePmode, originalName);
        }
    }
}