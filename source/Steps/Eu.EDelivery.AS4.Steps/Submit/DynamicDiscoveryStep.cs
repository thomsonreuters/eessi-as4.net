using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep" /> implementation
    /// to dynamically complete the <see cref="SendingProcessingMode"/>
    /// </summary>
    public class DynamicDiscoveryStep : IConfigStep
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDynamicDiscoveryProfile _profile;

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.SendingPMode.DynamicDiscoverySpecified == false)
            {
                return StepResult.Success(messagingContext);
            }

            Logger.Info("DynamicDiscovery is enabled in Sending PMode");

            var clonedPMode = (SendingProcessingMode)messagingContext.SendingPMode.Clone();
            clonedPMode.Id = $"{clonedPMode.Id}_SMP";

            var smpServerUri = BuildSmpServerUri(clonedPMode);

            XmlDocument smpMetaData = await RetrieveSmpMetaData(smpServerUri);

            var sendingPMode = _profile.DecoratePModeWithSmpMetaData(clonedPMode, smpMetaData);

            Logger.Info("Sending PMode completed with SMP metadata");

            messagingContext.SendingPMode = sendingPMode;
            messagingContext.SubmitMessage.PMode = sendingPMode;
            
            return StepResult.Success(messagingContext);
        }

        private Uri BuildSmpServerUri(SendingProcessingMode clonedPMode)
        {
            if (clonedPMode.DynamicDiscovery == null)
            {
                throw new ConfigurationErrorsException(@"The Sending PMode requires a DynamicDiscovery element");
            }

            if (clonedPMode.MessagePackaging?.PartyInfo?.ToParty?.PartyIds.Any() == false)
            {
                throw new ConfigurationErrorsException("The Sending PMode must contain a ToParty Id");
            }

            var toPartyId = clonedPMode.MessagePackaging.PartyInfo.ToParty.PartyIds.First().Id;
            var dynamicDiscoveryConfig = clonedPMode.DynamicDiscovery;

            return _profile.CreateSmpServerUri(toPartyId, dynamicDiscoveryConfig);
        }

        private static async Task<XmlDocument> RetrieveSmpMetaData(Uri smpServerUri)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(smpServerUri);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpException((int)response.StatusCode, "Unexpected result returned from SMP Service");
            }

            if (response.Content.Headers.ContentType.MediaType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) == -1)
            {
                throw new NotSupportedException($"An XML response was expected from the SMP server instead of {response.Content.Headers.ContentType.MediaType}");
            }

            var result = new XmlDocument();
            result.Load(await response.Content.ReadAsStreamAsync());

            return result;
        }

        /// <summary>
        /// Configure the step with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            string typeName = properties.ReadMandatoryProperty("SmpProfile");
            _profile = GenericTypeBuilder.FromType(typeName).Build<IDynamicDiscoveryProfile>();
        }
    }
}
