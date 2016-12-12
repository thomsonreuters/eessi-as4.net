using System;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly IAs4SettingsService settingsService;

        public ConfigurationController(IAs4SettingsService settingsService, ILogging logging)
        {
            this.settingsService = settingsService;
            throw new Exception("test");
        }

        [HttpGet]
        public async Task<AS4Model.Settings> Get()
        {
            return await settingsService.GetSettings();
        }

        [HttpPost]
        [Route("basesettings")]
        public async Task SaveBaseSettings([FromBody] BaseSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveBaseSettings(settings);
        }

        [HttpPost]
        [Route("customsettings")]
        public async Task SaveCustomSettings([FromBody] CustomSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveCustomSettings(settings);
        }

        [HttpPost]
        [Route("databasesettings")]
        public async Task SaveDatabaseSettings([FromBody] SettingsDatabase settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveDatabaseSettings(settings);
        }

        [HttpPost]
        [Route("submitagents")]
        public async Task CreateSubmitAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
        }

        [HttpDelete]
        [Route("submitagents")]
        public async Task DeleteSubmitAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
        }

        [HttpPut]
        [Route("submitagents/{originalName}")]
        public async Task UpdateSubmitAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
        }

        [HttpPost]
        [Route("receiveagents")]
        public async Task CreateReceiveAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
        }

        [HttpDelete]
        [Route("receiveagents")]
        public async Task DeleteReceiveAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
        }

        [HttpPut]
        [Route("receiveagents/{originalName}")]
        public async Task UpdateReceiveAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
        }

        [HttpPost]
        [Route("sendagents")]
        public async Task CreateSendAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
        }

        [HttpDelete]
        [Route("sendagents")]
        public async Task DeleteSendAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
        }

        [HttpPut]
        [Route("sendagents/{originalName}")]
        public async Task UpdateSendAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
        }

        [HttpPost]
        [Route("deliveragents")]
        public async Task CreateDeliverAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        [HttpDelete]
        [Route("deliveragents")]
        public async Task DeleteDeliverAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        [HttpPut]
        [Route("deliveragents/{originalName}")]
        public async Task UpdateDeliverAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        [HttpPost]
        [Route("notifyagents")]
        public async Task CreateNotifyAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
        }

        [HttpDelete]
        [Route("notifyagents")]
        public async Task DeleteNotifyAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
        }

        [HttpPut]
        [Route("notifyagents/{originalName}")]
        public async Task UpdateNotifyAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
        }

        [HttpPost]
        [Route("receptionawarenessagent")]
        public async Task CreateReceptionAwarenessAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceptionAwarenessAgent == null ? new SettingsAgent[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
        }

        [HttpDelete]
        [Route("receptionawarenessagent")]
        public async Task DeleteReceptionAwarenessAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceptionAwarenessAgent == null ? new SettingsAgent[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents.FirstOrDefault());
        }

        [HttpPut]
        [Route("receptionawarenessagent/{originalName}")]
        public async Task UpdateReceptionAwarenessAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceptionAwarenessAgent == null ? new SettingsAgent[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
        }
    }
}