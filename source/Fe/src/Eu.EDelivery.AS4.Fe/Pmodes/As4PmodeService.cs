using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public class As4PmodeService : IAs4PmodeService
    {
        private readonly IAs4PmodeSource source;

        public As4PmodeService(IAs4PmodeSource source)
        {
            this.source = source;
        }

        public async Task<IEnumerable<string>> GetReceivingNames()
        {
            return await source.GetReceivingNames();
        }

        public async Task<ReceivingPmode> GetReceivingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetReceivingByName(name);
        }

        public async Task<IEnumerable<string>> GetSendingNames()
        {
            return await source.GetSendingNames();
        }

        public async Task<SendingPmode> GetSendingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetSendingByName(name);
        }

        public async Task CreateReceiving(ReceivingPmode pmode)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            var exists = await source.GetReceivingByName(pmode.Name);
            if (exists != null) throw new Exception($"Pmode with name {pmode.Name} already exists.");
            await source.CreateReceiving(pmode);
        }

        public async Task CreateSending(SendingPmode pmode)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            var exists = await source.GetSendingByName(pmode.Name);
            if (exists != null) throw new Exception($"Pmode with name {pmode.Name} already exists.");
            await source.CreateSending(pmode);
        }

        public async Task DeleteReceiving(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var exists = await source.GetReceivingByName(name);
            if (exists == null) throw new Exception($"Pmode with name {name} doesn't exist");
            await source.DeleteReceiving(name);
        }

        public async Task DeleteSending(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var exists = await source.GetSendingByName(name);
            if (exists == null) throw new Exception($"Pmode with name {name} doesn't exist");
            await source.DeleteSending(name);
        }

        public async Task UpdateSending(SendingPmode pmode, string originalName)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (pmode.Name != originalName)
            {
                var newExists = await GetSendingByName(pmode.Name);
                if (newExists != null) throw new Exception($"Pmode with {originalName} already exists");
            }

            await source.UpdateSending(pmode, originalName);
        }

        public async Task UpdateReceiving(ReceivingPmode pmode, string originalName)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (pmode.Name != originalName)
            {
                var newExists = await GetReceivingByName(pmode.Name);
                if (newExists != null) throw new Exception($"Pmode with {originalName} already exists");
            }

            await source.UpdateReceiving(pmode, originalName);
        }
    }
}