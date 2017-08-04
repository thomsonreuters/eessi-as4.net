using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Model.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    /// <summary>
    /// Controller to manage settings.xml
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {
        private readonly IAs4SettingsService settingsService;
        private readonly IOptions<PortalSettings> portalSettings;
        private readonly IPortalSettingsService portalSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationController" /> class.
        /// </summary>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="logging">The logging.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="portalSettingsService">The portal settings service.</param>
        public ConfigurationController(IAs4SettingsService settingsService, ILogging logging, IOptions<PortalSettings> portalSettings, IPortalSettingsService portalSettingsService)
        {
            this.settingsService = settingsService;
            this.portalSettings = portalSettings;
            this.portalSettingsService = portalSettingsService;
        }

        /// <summary>
        /// Get settings
        /// </summary>
        /// <returns>Settings object</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(Model.Internal.Settings))]
        public async Task<Model.Internal.Settings> Get()
        {
            return await settingsService.GetSettings();
        }

        /// <summary>
        /// Gets the portal settings.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("portal")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(PortalSettings))]
        public PortalSettings GetRuntimeSettings()
        {
            return portalSettings.Value;
        }

        /// <summary>
        /// Saves the portal settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("portal")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SavePortalSettings([FromBody] PortalSettings settings)
        {
            await portalSettingsService.Save(settings);
            return new OkResult();
        }

        /// <summary>
        /// Save basic settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>OkResult</returns>
        [HttpPost]
        [Route("basesettings")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> SaveBaseSettings([FromBody] BaseSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveBaseSettings(settings);
            return new OkResult();
        }

        /// <summary>
        /// Save custom settings
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>OkResult</returns>
        [HttpPost]
        [Route("customsettings")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> SaveCustomSettings([FromBody] CustomSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveCustomSettings(settings);
            return new OkResult();
        }

        /// <summary>
        /// Saves the database settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>OkResult</returns>
        [HttpPost]
        [Route("databasesettings")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> SaveDatabaseSettings([FromBody] SettingsDatabase settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SaveDatabaseSettings(settings);
            return new OkResult();
        }

        /// <summary>
        /// Create a submit agent
        /// </summary>
        /// <param name="settingsAgent">The submit agent agent.</param>
        /// <returns>OkResult</returns>
        [HttpPost]
        [Route("submitagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateSubmitAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Delete a submit agent
        /// </summary>
        /// <param name="name">The name of the submit agent.</param>
        /// <returns>OkResult</returns>
        [HttpDelete]
        [Route("submitagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteSubmitAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Update an existing submit agent
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("submitagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateSubmitAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the outbound processing agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("outboundprocessingagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateOutboundProcessingAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the outbound processing agent agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("outboundprocessingagent")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateOutboundProcessingAgentAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the outbound processing agent agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("outboundprocessingagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteOutboundProcessingAgentAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.OutboundProcessingAgents, (settings, agents) => settings.OutboundProcessingAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the send agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("sendagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateSendAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the send agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("sendagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> DeleteSendAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the send agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("sendagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateSendAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.SendAgents, (settings, agents) => settings.SendAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the receive agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("receiveagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateReceiveAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the receive agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("receiveagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteReceiveAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the receive agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("receiveagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateReceiveAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the deliver agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("deliveragents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateDeliverAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the deliver agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("deliveragents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task DeleteDeliverAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
        }

        /// <summary>
        /// Updates the deliver agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("deliveragents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateDeliverAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.DeliverAgents, (settings, agents) => settings.DeliverAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the notify consumer agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("notifyconsumeragents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateNotifyConsumerAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyConsumerAgents, (settings, agents) => settings.NotifyConsumerAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the notify consumer agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("notifyconsumeragents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteNotifyConsumerAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyConsumerAgents, (settings, agents) => settings.NotifyConsumerAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the notify consumer agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("notifyconsumeragents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateNotifyConsumerAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyConsumerAgents, (settings, agents) => settings.NotifyConsumerAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the notify producer agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("notifyproduceragents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateNotifyProducerAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyProducerAgents, (settings, agents) => settings.NotifyProducerAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the notify producer agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("notifyproduceragents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteNotifyProducerAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyProducerAgents, (settings, agents) => settings.NotifyProducerAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the notify producer agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("notifyproduceragents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateNotifyProducerAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyProducerAgents, (settings, agents) => settings.NotifyProducerAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the reception awareness agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("receptionawarenessagent")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateReceptionAwarenessAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ReceptionAwarenessAgent == null ? new AgentSettings[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the reception awareness agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("receptionawarenessagent")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteReceptionAwarenessAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ReceptionAwarenessAgent == null ? new AgentSettings[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents.FirstOrDefault());
            return new OkResult();
        }

        /// <summary>
        /// Updates the reception awareness agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("receptionawarenessagent/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateReceptionAwarenessAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ReceptionAwarenessAgent == null ? new AgentSettings[] { } : new[] { agents.ReceptionAwarenessAgent }, (settings, agents) => settings.ReceptionAwarenessAgent = agents[0]);
            return new OkResult();
        }

        /// <summary>
        /// Creates the pull receive agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("pullreceiveagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreatePullReceiveAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.PullReceiveAgents, (settings, agents) => settings.PullReceiveAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the pull receive agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("pullreceiveagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeletePullReceiveAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.PullReceiveAgents, (settings, agents) => settings.PullReceiveAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the pull receive agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("pullreceiveagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdatePullReceiveAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.PullReceiveAgents, (settings, agents) => settings.PullReceiveAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Creates the pull send agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("pullsendagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreatePullSendAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.PullSendAgents, (settings, agents) => settings.PullSendAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the pull send agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("pullsendagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task DeletePullSendAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.PullSendAgents, (settings, agents) => settings.PullSendAgents = agents);
        }

        /// <summary>
        /// Updates the pull send agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("pullsendagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task UpdatePullSendAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.PullSendAgents, (settings, agents) => settings.PullSendAgents = agents);
        }
    }
}