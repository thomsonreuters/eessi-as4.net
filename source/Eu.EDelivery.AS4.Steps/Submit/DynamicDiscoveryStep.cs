using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.Validators;
using NLog;
using SubmitParty = Eu.EDelivery.AS4.Model.Common.Party;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;
using AS4Party = Eu.EDelivery.AS4.Model.Core.Party;
using Exception = System.Exception;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

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
        private readonly Func<string, IDynamicDiscoveryProfile> _resolveDynamicDiscoveryProfile;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDiscoveryStep"/> class.
        /// </summary>
        public DynamicDiscoveryStep() : this(ResolveDynamicDiscoveryProfile) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDiscoveryStep"/> class.
        /// </summary>
        internal DynamicDiscoveryStep(Func<string, IDynamicDiscoveryProfile> resolveDynamicDiscoveryProfile)
        {
            if (resolveDynamicDiscoveryProfile == null)
            {
                throw new ArgumentNullException(nameof(resolveDynamicDiscoveryProfile));
            }

            _resolveDynamicDiscoveryProfile = resolveDynamicDiscoveryProfile;
        }

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

            if (messagingContext.AS4Message == null 
                && messagingContext.Mode == MessagingContextMode.Forward)
            {
                throw new InvalidOperationException(
                    $"{nameof(DynamicDiscoveryStep)} requires an AS4Message when used in a Forward Agent, "
                    + "please make sure that the ReceivedMessage is deserialized before executing this step."
                    + "Possibly this failure happend because the Transformer of the Forward Agent is still using "
                    + "the ForwardMessageTransformer instead of the AS4MessageTransformer");
            }

            if (messagingContext.SendingPMode == null ||
                messagingContext.SendingPMode.DynamicDiscoverySpecified == false)
            {
                Logger.Debug($"Dynamic Discovery in SendingPMode {messagingContext.SendingPMode?.Id} is not configured");
                return StepResult.Success(messagingContext);
            }

            string smpProfile = messagingContext.SendingPMode.DynamicDiscovery.SmpProfile;
            Logger.Info($"{messagingContext.LogTag} DynamicDiscovery is enabled in SendingPMode - using {smpProfile}");

            var clonedPMode = (SendingProcessingMode) messagingContext.SendingPMode.Clone();
            clonedPMode.Id = $"{clonedPMode.Id}_SMP";

            IDynamicDiscoveryProfile profile = _resolveDynamicDiscoveryProfile(smpProfile);
            AS4Party toParty = 
                messagingContext.Mode == MessagingContextMode.Forward
                ? ResolveAS4ReceiverParty(messagingContext.AS4Message)
                : ResolveSubmitOrPModeReceiverParty(
                    messagingContext.SubmitMessage?.PartyInfo?.ToParty,
                    messagingContext.SendingPMode?.MessagePackaging?.PartyInfo?.ToParty,
                    messagingContext.SendingPMode?.AllowOverride == true);
            
            XmlDocument smpMetaData = await RetrieveSmpMetaData(profile, clonedPMode.DynamicDiscovery, toParty);

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
            DynamicDiscoveryConfiguration dynamicDiscovery,
            AS4Party toParty)
        {
            if (dynamicDiscovery == null)
            {
                throw new ConfigurationErrorsException(
                    @"Cannot retrieve SMP metadata: SendingPMode requires a <DynamicDiscovery/> element");
            }

            Dictionary<string, string> customProperties = 
                dynamicDiscovery.Settings?.ToDictionary(s => s.Key, s => s.Value) 
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

        private static AS4Party ResolveAS4ReceiverParty(AS4Message msg)
        {
            if (msg?.PrimaryMessageUnit is UserMessage m)
            {
                return m.Receiver;
            }

            throw new InvalidOperationException(
                "AS4Message is not an UserMessage so can't dynamically discover the SendingPMode with the ToParty from it");
        }

        private static AS4Party ResolveSubmitOrPModeReceiverParty(
            SubmitParty submitParty,
            PModeParty pmodeParty, 
            bool allowOverride)
        {
            if (submitParty != null && allowOverride == false)
            {
                if (pmodeParty != null && submitParty.Equals(pmodeParty) == false)
                {
                    throw new NotSupportedException(
                        "SubmitMessage is not allowed by the SendingPMode to override ToParty");
                }
            }

            if (submitParty == null && pmodeParty == null)
            {
                throw new InvalidOperationException(
                    "Either the SubmitMessage or the SendingPMode is required to have a ToParty configured for dynamic discovery");
            }

            if (submitParty != null)
            {
                return CreateToPartyFrom(
                    "SubmitMessage", 
                    submitParty.Role, 
                    submitParty.PartyIds?.Select(p => new PartyId(p.Id, p.Type.AsMaybe())));
            }

            if (pmodeParty?.PartyIds.Any() != true)
            {
                throw new ConfigurationErrorsException(
                    "Cannot retrieve SMP metadata: SendingPMode must contain at lease one <ToPartyId/> element in the MessagePackaging.PartyInfo.ToParty element");
            }

            return CreateToPartyFrom(
                "SendingPMode", 
                pmodeParty.Role, 
                pmodeParty.PartyIds?.Select(p => new PartyId(p.Id, p.Type.AsMaybe())));
        }

        private static AS4Party CreateToPartyFrom(string log, string role, IEnumerable<PartyId> ids)
        {
            try
            {
                return new AS4Party(role, ids);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidDataException(log + " has an incomplete ToParty: " + ex.Message);
            }
        }
    }
}
