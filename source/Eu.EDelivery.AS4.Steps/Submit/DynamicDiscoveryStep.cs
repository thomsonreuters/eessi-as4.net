using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep" /> implementation
    /// to dynamically complete the <see cref="SendingProcessingMode"/>
    /// </summary>    
    [Info("Perform Dynamic Discovery if required")]
    [Description(
        "Contacts an SMP server and executes the configured SMP Profile if dynamic discovery is enabled. \n\r" +
        "The information returned from the SMP server is used to complete the sending PMode.")]
    public class DynamicDiscoveryStep : IStep
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.SendingPMode == null ||
                messagingContext.SendingPMode.DynamicDiscoverySpecified == false)
            {
                Logger.Debug($"Dynamic Discovery in SendingPMode {messagingContext.SendingPMode?.Id} is not configured");
                return StepResult.Success(messagingContext);
            }

            string smpProfile = messagingContext.SendingPMode.DynamicDiscovery.SmpProfile;
            Logger.Info($"{messagingContext.LogTag} DynamicDiscovery is enabled in SendingPMode - using {smpProfile}");

            var clonedPMode = (SendingProcessingMode)messagingContext.SendingPMode.Clone();
            clonedPMode.Id = $"{clonedPMode.Id}_SMP";
            IDynamicDiscoveryProfile profile = ResolveDynamicDiscoveryProfile(smpProfile);
            XmlDocument smpMetaData = await RetrieveSmpMetaData(profile, clonedPMode);

            SendingProcessingMode sendingPMode = profile.DecoratePModeWithSmpMetaData(clonedPMode, smpMetaData);
            Logger.Info($"{messagingContext.LogTag} SendingPMode {sendingPMode.Id} completed with SMP metadata");
            ValidatePMode(sendingPMode);

            messagingContext.SendingPMode = sendingPMode;
            if (messagingContext.SubmitMessage != null)
            {
                messagingContext.SubmitMessage.PMode = sendingPMode;
            }

            return StepResult.Success(messagingContext);
        }

        private static IDynamicDiscoveryProfile ResolveDynamicDiscoveryProfile(string smpProfile)
        {
            if (string.IsNullOrEmpty(smpProfile))
            {
                return new LocalDynamicDiscoveryProfile();
            }

            return GenericTypeBuilder.FromType(smpProfile).Build<IDynamicDiscoveryProfile>();
        }

        private static async Task<XmlDocument> RetrieveSmpMetaData(
            IDynamicDiscoveryProfile profile, 
            SendingProcessingMode pmode)
        {
            if (pmode.DynamicDiscovery == null)
            {
                throw new ConfigurationErrorsException(
                    $@"Cannot retrieve SMP metadata: SendingPMode {pmode.Id} requires a <DynamicDiscovery/> element");
            }

            if (pmode.MessagePackaging?.PartyInfo?.ToParty?.PartyIds.Any() != true)
            {
                throw new ConfigurationErrorsException(
                    $"Cannot retrieve SMP metadata: SendingPMode {pmode.Id} " + 
                    "must contain at lease one <ToPartyId/> element in the MessagePackaging.PartyInfo.ToParty element");
            }

            Party toParty = pmode.MessagePackaging.PartyInfo.ToParty;
            Dictionary<string, string> customProperties = 
                pmode.DynamicDiscovery.Settings?.ToDictionary(s => s.Key, s => s.Value) 
                    ?? new Dictionary<string, string>();

            return await profile.RetrieveSmpMetaData(
                party: toParty, 
                properties: customProperties);
        }

        private static void ValidatePMode(SendingProcessingMode pmode)
        {
            SendingProcessingModeValidator.Instance.Validate(pmode).Result(
                onValidationSuccess: result => Logger.Debug($"Dynamically completed PMode {pmode.Id} is valid"),
                onValidationFailed: result =>
                {
                    string errorMessage = 
                        result.AppendValidationErrorsToErrorMessage(
                            $"(Submit) Dynamically completed PMode {pmode.Id} was invalid:");

                    Logger.Error(errorMessage);

                    throw new ConfigurationErrorsException(errorMessage);
                });
        }
    }
}
