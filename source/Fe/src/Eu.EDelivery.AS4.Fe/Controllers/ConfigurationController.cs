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
    public class PmodeController : Controller
    {
        public PmodeController()
        {
            
        }        
    }

    [Route("api/[controller]")]
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly IAs4SettingsService settingsService;

        public ConfigurationController(IAs4SettingsService settingsService, ILogging logging)
        {
            this.settingsService = settingsService;
        }

        [HttpGet]
        public async Task<AS4Model.Settings> Get()
        {
            return await settingsService.GetSettings();
        }

        [HttpPost]
        [Route("basesettings")]
        public async Task<OkResult> SaveBaseSettings([FromBody] BaseSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveBaseSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("customsettings")]
        public async Task<OkResult> SaveCustomSettings([FromBody] CustomSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveCustomSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("databasesettings")]
        public async Task<OkResult> SaveDatabaseSettings([FromBody] SettingsDatabase settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveDatabaseSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("submitagents")]
        public async Task<OkResult> CreateSubmitAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("submitagents")]
        public async Task<OkResult> DeleteSubmitAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return Ok();
        }

        [HttpPut]
        [Route("submitagents/{originalName}")]
        public async Task<OkResult> UpdateSubmitAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("receiveagents")]
        public async Task<OkResult> CreateReceiveAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("receiveagents")]
        public async Task<OkResult> DeleteReceiveAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return Ok();
        }

        [HttpPut]
        [Route("receiveagents/{originalName}")]
        public async Task<OkResult> UpdateReceiveAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("sendagents")]
        public async Task<OkResult> CreateSendAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("sendagents")]
        public async Task<OkResult> DeleteSendAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return Ok();
        }

        [HttpPut]
        [Route("sendagents/{originalName}")]
        public async Task<OkResult> UpdateSendAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("deliveragents")]
        public async Task<OkResult> CreateDeliverAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("deliveragents")]
        public async Task<OkResult> DeleteDeliverAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return Ok();
        }

        [HttpPut]
        [Route("deliveragents/{originalName}")]
        public async Task<OkResult> UpdateDeliverAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("notifyagents")]
        public async Task<OkResult> CreateNotifyAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("notifyagents")]
        public async Task<OkResult> DeleteNotifyAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return Ok();
        }

        [HttpPut]
        [Route("notifyagents/{originalName}")]
        public async Task<OkResult> UpdateNotifyAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("receptionawarenessagent")]
        public async Task<OkResult> CreateReceptionAwarenessAgent([FromBody] SettingsAgent settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceptionAwarenessAgent == null ? new SettingsAgent[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
            return Ok();
        }

        [HttpDelete]
        [Route("receptionawarenessagent")]
        public async Task<OkResult> DeleteReceptionAwarenessAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceptionAwarenessAgent == null ? new SettingsAgent[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents.FirstOrDefault());
            return Ok();
        }

        [HttpPut]
        [Route("receptionawarenessagent/{originalName}")]
        public async Task<OkResult> UpdateReceptionAwarenessAgent([FromBody] SettingsAgent settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceptionAwarenessAgent == null ? new SettingsAgent[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
            return Ok();
        }
    }
}