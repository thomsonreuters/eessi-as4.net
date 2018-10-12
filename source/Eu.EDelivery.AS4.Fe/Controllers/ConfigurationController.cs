using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EnsureThat;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Runtime;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;
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
        private readonly IRuntimeLoader runtimeLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationController" /> class.
        /// </summary>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="logging">The logging.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="portalSettingsService">The portal settings service.</param>
        /// <param name="runtimeLoader">The runtime loader.</param>
        public ConfigurationController(
            IAs4SettingsService settingsService, 
            ILogging logging, 
            IOptions<PortalSettings> portalSettings, 
            IPortalSettingsService portalSettingsService,
            IRuntimeLoader runtimeLoader)
        {
            this.settingsService = settingsService;
            this.portalSettings = portalSettings;
            this.portalSettingsService = portalSettingsService;
            this.runtimeLoader = runtimeLoader;
        }

        /// <summary>
        /// Returns if the portal is in setup state
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("setup")]
        [AllowAnonymous]
        public async Task<IActionResult> IsSetup()
        {
            return new OkObjectResult(await portalSettingsService.IsSetup());
        }

        /// <summary>
        /// Saves the setup.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("setup")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveSetup([FromBody] Setup setup)
        {
            await portalSettingsService.SaveSetup(setup);
            return new OkResult();
        }

        /// <summary>
        /// Posts the authorization map.
        /// </summary>
        /// <param name="authorizationEntries">The authorization entries.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("authorizationmap")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public IActionResult PostAuthorizationMap([FromBody] IEnumerable<PullRequestAuthorizationEntry> authorizationEntries)
        {
            Registry.Instance.PullRequestAuthorizationMapProvider.SavePullRequestAuthorizationEntries(authorizationEntries);
            return new OkResult();
        }

        /// <summary>
        /// Gets the authorization map.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("authorizationmap")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(IEnumerable<PullRequestAuthorizationEntry>))]
        public IActionResult GetAuthorizationMap()
        {
            return new OkObjectResult(Registry.Instance.PullRequestAuthorizationMapProvider.GetPullRequestAuthorizationEntryOverview());
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
        /// Save pull send settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>OkResult</returns>
        [HttpPost]
        [Route("pullsendsettings")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OkResult))]
        public async Task<IActionResult> SavePullSendSettings([FromBody] SettingsPullSend settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            await settingsService.SavePullSendSettings(settings);
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
        /// Gets the default agent receiver.
        /// </summary>
        /// <param name="agentType">Type of the agent</param>
        /// <returns>Receiver for the requested agent type</returns>
        [HttpGet]
        [Route("defaultagentreceiver/{agentType}")]
        public IActionResult GetDefaultAgentReceiver(AgentType agentType)
        {
            return new OkObjectResult(AgentProvider.GetDefaultReceiverForAgentType(agentType));
        }

        /// <summary>
        /// Gets the default agent steps.
        /// </summary>
        /// <param name="agentType">Type of the agent.</param>
        /// <returns>StepConfiguration for the requested agent type</returns>
        [HttpGet]
        [Route("defaultagentsteps/{agentType}")]
        public IActionResult GetDefaultAgentSteps(AgentType agentType)
        {
            var steps = AgentProvider.GetDefaultStepConfigurationForAgentType(agentType);
            IEnumerable<ItemType> FilterStepsFor(IEnumerable<Step> xs) 
                => xs.Select(x => runtimeLoader.Steps.First(s => s.TechnicalName == x.Type));

            return new OkObjectResult(new
            {
                NormalPipeline = FilterStepsFor(steps.NormalPipeline),
                ErrorPipeline = FilterStepsFor(steps.ErrorPipeline ?? Enumerable.Empty<Step>())
            });
        }

        /// <summary>
        /// Gets the default agent transformer.
        /// </summary>
        /// <param name="agentType">Type of the agent.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("defaultagenttransformer/{agentType}")]
        public IActionResult GetDefaultAgentTransformer(AgentType agentType)
        {
            var transformerEntry = AgentProvider.GetDefaultTransformerForAgentType(agentType);
            var availableTransformers = transformerEntry.OtherTransformers.Concat(new[] {transformerEntry.DefaultTransformer});
            var types = runtimeLoader.Transformers.Where(t => availableTransformers.Any(x => x.Type == t.TechnicalName));

            return new OkObjectResult(new
            {
                DefaultTransformer = types.First(t => t.TechnicalName == transformerEntry.DefaultTransformer.Type),
                OtherTransformers = types.Where(t => t.TechnicalName != transformerEntry.DefaultTransformer.Type)
            });
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
        /// Creates the forward agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("forwardagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int) HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateForwardAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.ForwardAgents, (settings, agents) => settings.ForwardAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the forward agent.
        /// </summary>
        /// <param name="settingsAgent"></param>
        /// <param name="originalName"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("forwardagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int) HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int) HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateForwardAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.ForwardAgents, (settings, agents) => settings.ForwardAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the forward agent.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("forwardagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int) HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> DeleteForwardAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.ForwardAgents, (settings, agents) => settings.ForwardAgents = agents);
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
        /// Creates the notify agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("notifyagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        public async Task<IActionResult> CreateNotifyConsumerAgent([FromBody] AgentSettings settingsAgent)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            await settingsService.CreateAgent(settingsAgent, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Deletes the notify agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("notifyagents")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> DeleteNotifyConsumerAgent(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            await settingsService.DeleteAgent(name, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
            return new OkResult();
        }

        /// <summary>
        /// Updates the notify agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("notifyagents/{originalName}")]
        [Authorize(Roles = Roles.Admin)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(OkResult))]
        [SwaggerResponse((int)HttpStatusCode.Conflict, typeof(ErrorModel), "Indicates that another entity already exists")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, typeof(ErrorModel), "Returned when the requested submit agent doesn't exist")]
        public async Task<IActionResult> UpdateNotifyConsumerAgent([FromBody] AgentSettings settingsAgent, string originalName)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));
            await settingsService.UpdateAgent(settingsAgent, originalName, agents => agents.NotifyAgents, (settings, agents) => settings.NotifyAgents = agents);
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