using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Model.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {
        private readonly IAs4SettingsService settingsService;

        public ConfigurationController(IAs4SettingsService settingsService, ILogging logging)
        {
            this.settingsService = settingsService;
        }

        [HttpGet]
        public async Task<Model.Internal.Settings> Get()
        {
            return await settingsService.GetSettings();
        }

        [HttpPost]
        [Route("basesettings")]
        [Authorize(Roles = Roles.Admin)]
        public async Task SaveBaseSettings([FromBody] BaseSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveBaseSettings(settings);
        }

        [HttpPost]
        [Route("customsettings")]
        [Authorize(Roles = Roles.Admin)]
        public async Task SaveCustomSettings([FromBody] CustomSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveCustomSettings(settings);
        }

        [HttpPost]
        [Route("databasesettings")]
        [Authorize(Roles = Roles.Admin)]
        public async Task SaveDatabaseSettings([FromBody] SettingsDatabase settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveDatabaseSettings(settings);
        }

        [HttpPost]
        [Route("submitagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateSubmitAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
        }

        [HttpDelete]
        [Route("submitagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteSubmitAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
        }

        [HttpPut]
        [Route("outboundprocessingagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateOutboundProcessingAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
        }

        [HttpPost]
        [Route("outboundprocessingagent")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateOutboundProcessingAgentAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
        }

        [HttpDelete]
        [Route("outboundprocessingagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteOutboundProcessingAgentAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
        }

        [HttpPut]
        [Route("outboundprocessingagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateOutboundProcessingAgentAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
        }

        [HttpPost]
        [Route("sendagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateSendAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
        }

        [HttpDelete]
        [Route("sendagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteSendAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
        }

        [HttpPut]
        [Route("sendagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateSendAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
        }

        [HttpPost]
        [Route("receiveagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateReceiveAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
        }

        [HttpDelete]
        [Route("receiveagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteReceiveAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
        }

        [HttpPut]
        [Route("receiveagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateReceiveAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
        }

        [HttpPost]
        [Route("deliveragents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateDeliverAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        [HttpDelete]
        [Route("deliveragents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteDeliverAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        [HttpPut]
        [Route("deliveragents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateDeliverAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        [HttpPost]
        [Route("notifyconsumeragents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateNotifyConsumerAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyConsumerAgents, (settings, agents) => settings.NotifyConsumerAgents = agents);
        }

        [HttpDelete]
        [Route("notifyconsumeragents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteNotifyConsumerAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyConsumerAgents, (settings, agents) => settings.NotifyConsumerAgents = agents);
        }

        [HttpPut]
        [Route("notifyconsumeragents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateNotifyConsumerAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyConsumerAgents, (settings, agents) => settings.NotifyConsumerAgents = agents);
        }

        [HttpPost]
        [Route("notifyproduceragents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateNotifyProducerAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyProducerAgents, (settings, agents) => settings.NotifyProducerAgents = agents);
        }

        [HttpDelete]
        [Route("notifyproduceragents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteNotifyProducerAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyProducerAgents, (settings, agents) => settings.NotifyProducerAgents = agents);
        }

        [HttpPut]
        [Route("notifyproduceragents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateNotifyProducerAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyProducerAgents, (settings, agents) => settings.NotifyProducerAgents = agents);
        }

        [HttpPost]
        [Route("receptionawarenessagent")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreateReceptionAwarenessAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceptionAwarenessAgent == null ? new AgentSettings[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
        }

        [HttpDelete]
        [Route("receptionawarenessagent")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeleteReceptionAwarenessAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceptionAwarenessAgent == null ? new AgentSettings[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents.FirstOrDefault());
        }

        [HttpPut]
        [Route("receptionawarenessagent/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdateReceptionAwarenessAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceptionAwarenessAgent == null ? new AgentSettings[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
        }

        [HttpPost]
        [Route("pullreceiveagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreatePullReceiveAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.PullReceiveAgents, (settings, agents) => settings.PullReceiveAgents = agents);
        }

        [HttpDelete]
        [Route("pullreceiveagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeletePullReceiveAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.PullReceiveAgents, (settings, agents) => settings.PullReceiveAgents = agents);
        }

        [HttpPut]
        [Route("pullreceiveagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdatePullReceiveAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.PullReceiveAgents, (settings, agents) => settings.PullReceiveAgents = agents);
        }

        [HttpPost]
        [Route("pullsendagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task CreatePullSendAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.PullSendAgents, (settings, agents) => settings.PullSendAgents = agents);
        }

        [HttpDelete]
        [Route("pullsendagents")]
        [Authorize(Roles = Roles.Admin)]
        public async Task DeletePullSendAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.PullSendAgents, (settings, agents) => settings.PullSendAgents = agents);
        }

        [HttpPut]
        [Route("pullsendagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task UpdatePullSendAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.PullSendAgents, (settings, agents) => settings.PullSendAgents = agents);
        }
    }
}