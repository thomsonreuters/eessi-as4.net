using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    ///     Manage pmodes
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Pmodes.IPmodeService" />
    public class PmodeService : IPmodeService
    {
        private readonly IAs4PmodeSource source;
        private readonly bool disableValidation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PmodeService" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="disableValidation">if set to <c>true</c> [disable validation].</param>
        public PmodeService(IAs4PmodeSource source, bool disableValidation = false)
        {
            this.source = source;
            this.disableValidation = disableValidation;
        }

        /// <summary>
        ///     Gets the receiving names.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetReceivingNames()
        {
            return await source.GetReceivingNames();
        }

        /// <summary>
        ///     Get a list of receiving pmodes
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        public async Task<ReceivingBasePmode> GetReceivingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetReceivingByName(name);
        }

        /// <summary>
        ///     Get a list of sending pmodes
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetSendingNames()
        {
            return await source.GetSendingNames();
        }

        /// <summary>
        ///     Get a sending pmode by name
        /// </summary>
        /// <param name="name">The name of the pmode.</param>
        /// <returns></returns>
        public async Task<SendingBasePmode> GetSendingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetSendingByName(name);
        }

        /// <summary>
        ///     Create a receiving pmode
        /// </summary>
        /// <param name="basePmode">The pmode to create</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">
        ///     Exception thrown when a pmode with the supplied name
        ///     already exists
        /// </exception>
        public async Task CreateReceiving(ReceivingBasePmode basePmode)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            var exists = await source.GetReceivingByName(basePmode.Name);
            if (exists != null) throw new AlreadyExistsException($"BasePmode with name {basePmode.Name} already exists.");
            ValidateReceivingPmode(basePmode);
            await source.CreateReceiving(basePmode);
        }

        /// <summary>
        ///     Create sending pmode
        /// </summary>
        /// <param name="basePmode">The pmode to create.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">
        ///     Exception thrown when a pmode with the supplied name
        ///     already exists
        /// </exception>
        public async Task CreateSending(SendingBasePmode basePmode)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            var exists = await source.GetSendingByName(basePmode.Name);
            if (exists != null) throw new AlreadyExistsException($"BasePmode with name {basePmode.Name} already exists.");
            ValidateSendingPmode(basePmode);
            await source.CreateSending(basePmode);
        }

        /// <summary>
        ///     Delete a receiving pmode
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
        ///     Delete a sending pmode
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
        ///     Update sending pmode
        /// </summary>
        /// <param name="basePmode">Date to update the sending pmode with</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">
        ///     Exception thrown when a sending pmode with the supplied
        ///     name already exists
        /// </exception>
        public async Task UpdateSending(SendingBasePmode basePmode, string originalName)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (basePmode.Name != originalName)
            {
                var newExists = await GetSendingByName(basePmode.Name);
                if (newExists != null) throw new AlreadyExistsException($"BasePmode with {originalName} already exists");
            }

            ValidateSendingPmode(basePmode);
            await source.UpdateSending(basePmode, originalName);
        }

        /// <summary>
        ///     Update receiving pmode
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">
        ///     Exception thrown when a pmode with the supplied name
        ///     already exists.
        /// </exception>
        public async Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName)
        {
            EnsureArg.IsNotNull(basePmode, nameof(basePmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (basePmode.Name != originalName)
            {
                var newExists = await GetReceivingByName(basePmode.Name);
                if (newExists != null) throw new AlreadyExistsException($"BasePmode with {originalName} already exists");
            }

            ValidateReceivingPmode(basePmode);
            await source.UpdateReceiving(basePmode, originalName);
        }

        /// <summary>
        /// Validates the sending pmode.
        /// </summary>
        /// <param name="sendingPmode">The sending pmode.</param>
        /// <exception cref="InvalidPModeException">Invalid PMode</exception>
        private void ValidateSendingPmode(SendingBasePmode sendingPmode)
        {
            if (disableValidation)
            {
                return;
            }
            
            var result = SendingProcessingModeValidator.Instance.Validate(sendingPmode.Pmode);

            if (!result.IsValid)
            {
                throw new InvalidPModeException("Invalid PMode", result);
            }
        }

        private void ValidateReceivingPmode(ReceivingBasePmode sendingPmode)
        {
            if (disableValidation)
            {
                return;
            }
            
            var result = ReceivingProcessingModeValidator.Instance.Validate(sendingPmode.Pmode);

            if (!result.IsValid)
            {
                throw new InvalidPModeException("Invalid PMode", result);
            }
        }
    }
}
