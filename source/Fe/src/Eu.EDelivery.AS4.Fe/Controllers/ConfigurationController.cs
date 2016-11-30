using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Services;
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
            await settingsService.SaveBaseSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("customsettings")]
        public async Task<OkResult> SaveCustomSettings([FromBody] CustomSettings settings)
        {
            await settingsService.SaveCustomSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("databasesettings")]
        public async Task<OkResult> SaveDatabaseSettings([FromBody] SettingsDatabase settings)
        {
            await settingsService.SaveDatabaseSettings(settings);
            return Ok();
        }

        [HttpPost]
        [Route("submitagents")]
        public async Task<OkResult> UpdateOrCreateSubmitAgent([FromBody] SettingsAgent settingsAgent)
        {
            await settingsService.UpdateOrCreateAgent(settingsAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("submitagents")]
        public async Task<OkResult> DeleteSubmitAgent(string name)
        {
            await settingsService.DeleteAgent(name, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("receiveagents")]
        public async Task<OkResult> UpdateOrCreateReceiveAgent([FromBody] SettingsAgent settingsAgent)
        {
            await settingsService.UpdateOrCreateAgent(settingsAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("receiveagents")]
        public async Task<OkResult> DeleteReceiveAgent(string name)
        {
            await settingsService.DeleteAgent(name, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("sendagents")]
        public async Task<OkResult> UpdateOrCreateSendAgent([FromBody] SettingsAgent settingsAgent)
        {
            await settingsService.UpdateOrCreateAgent(settingsAgent, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("sendagents")]
        public async Task<OkResult> DeleteSendAgent(string name)
        {
            await settingsService.DeleteAgent(name, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("deliveragents")]
        public async Task<OkResult> UpdateOrCreateDeliverAgent([FromBody] SettingsAgent settingsAgent)
        {
            await settingsService.UpdateOrCreateAgent(settingsAgent, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("deliveragents")]
        public async Task<OkResult> DeleteDeliverAgent(string name)
        {
            await settingsService.DeleteAgent(name, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return Ok();
        }

        [HttpPost]
        [Route("notifyagents")]
        public async Task<OkResult> UpdateOrCreateNotifyAgent([FromBody] SettingsAgent settingsAgent)
        {
            await settingsService.UpdateOrCreateAgent(settingsAgent, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return Ok();
        }

        [HttpDelete]
        [Route("notifyagents")]
        public async Task<OkResult> DeleteNotifyAgent(string name)
        {
            await settingsService.DeleteAgent(name, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return Ok();
        }
    }
}