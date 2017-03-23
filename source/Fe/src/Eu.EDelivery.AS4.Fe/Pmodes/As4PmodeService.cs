using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Monitor;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
  public class As4PmodeService : IAs4PmodeService
  {
    private readonly IAs4PmodeSource source;
    private readonly IMonitorService monitorService;

    public As4PmodeService(IAs4PmodeSource source, IMonitorService monitorService)
    {
      this.source = source;
      this.monitorService = monitorService;
    }

    public async Task<IEnumerable<string>> GetReceivingNames()
    {
      return await source.GetReceivingNames();
    }

    public async Task<ReceivingBasePmode> GetReceivingByName(string name)
    {
      EnsureArg.IsNotNullOrEmpty(name, nameof(name));
      return await source.GetReceivingByName(name);
    }

    public async Task<IEnumerable<string>> GetSendingNames()
    {
      return await source.GetSendingNames();
    }

    public async Task<SendingBasePmode> GetSendingByName(string name)
    {
      EnsureArg.IsNotNullOrEmpty(name, nameof(name));
      return await source.GetSendingByName(name);
    }

    public async Task CreateReceiving(ReceivingBasePmode basePmode)
    {
      EnsureArg.IsNotNull(basePmode, nameof(basePmode));
      var exists = await source.GetReceivingByName(basePmode.Name);
      if (exists != null) throw new Exception($"BasePmode with name {basePmode.Name} already exists.");
      await source.CreateReceiving(basePmode);
    }

    public async Task CreateSending(SendingBasePmode basePmode)
    {
      EnsureArg.IsNotNull(basePmode, nameof(basePmode));
      var exists = await source.GetSendingByName(basePmode.Name);
      if (exists != null) throw new Exception($"BasePmode with name {basePmode.Name} already exists.");
      await source.CreateSending(basePmode);
    }

    public async Task DeleteReceiving(string name)
    {
      EnsureArg.IsNotNullOrEmpty(name, nameof(name));
      var exists = await source.GetReceivingByName(name);
      if (exists == null) throw new Exception($"BasePmode with name {name} doesn't exist");
      await source.DeleteReceiving(name);
    }

    public async Task DeleteSending(string name)
    {
      EnsureArg.IsNotNullOrEmpty(name, nameof(name));
      var exists = await source.GetSendingByName(name);
      if (exists == null) throw new Exception($"BasePmode with name {name} doesn't exist");
      await source.DeleteSending(name);
    }

    public async Task UpdateSending(SendingBasePmode basePmode, string originalName)
    {
      EnsureArg.IsNotNull(basePmode, nameof(basePmode));
      EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

      if (basePmode.Name != originalName)
      {
        var newExists = await GetSendingByName(basePmode.Name);
        if (newExists != null) throw new Exception($"BasePmode with {originalName} already exists");
      }

      await source.UpdateSending(basePmode, originalName);
    }

    public async Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName)
    {
      EnsureArg.IsNotNull(basePmode, nameof(basePmode));
      EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

      if (basePmode.Name != originalName)
      {
        var newExists = await GetReceivingByName(basePmode.Name);
        if (newExists != null) throw new Exception($"BasePmode with {originalName} already exists");
      }

      await source.UpdateReceiving(basePmode, originalName);
    }

    public Task<bool> ValidateSendingPmodes(string messageId, string destinationPmodeName)
    {
      return Task.FromResult(false);
    }
  }
}