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
using SubmitPartyId = Eu.EDelivery.AS4.Model.Common.PartyId;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;
using PModePartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;
using AS4Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep" /> implementation to dynamically complete the <see cref="SendingProcessingMode"/>.
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

            if (messagingContext.SendingPMode == null 
                || messagingContext.SendingPMode.DynamicDiscoverySpecified == false)
            {
                Logger.Debug($"Dynamic Discovery in SendingPMode {messagingContext.SendingPMode?.Id} is not configured");
                return StepResult.Success(messagingContext);
            }

            var clonedPMode = (SendingProcessingMode) messagingContext.SendingPMode.Clone();
            clonedPMode.Id = $"{clonedPMode.Id}_SMP";

            string smpProfile = messagingContext.SendingPMode.DynamicDiscovery.SmpProfile;
            IDynamicDiscoveryProfile profile = _resolveDynamicDiscoveryProfile(smpProfile);
            Logger.Info($"{messagingContext.LogTag} DynamicDiscovery is enabled in SendingPMode - using {profile.GetType().Name}");

            AS4Party toParty = 
                messagingContext.Mode == MessagingContextMode.Forward
                ? ResolveAS4ReceiverParty(messagingContext.AS4Message)
                : ResolveSubmitOrPModeReceiverParty(
                    messagingContext.SubmitMessage?.PartyInfo?.ToParty,
                    messagingContext.SendingPMode?.MessagePackaging?.PartyInfo?.ToParty,
                    messagingContext.SendingPMode?.AllowOverride == true);

            SendingProcessingMode sendingPMode = await DynamicDiscoverSendingPModeAsync(messagingContext, profile, toParty);
            Logger.Info($"{messagingContext.LogTag} SendingPMode {sendingPMode.Id} completed with SMP metadata");

            messagingContext.SendingPMode = sendingPMode;
            if (messagingContext.SubmitMessage != null)
            {
                messagingContext.SubmitMessage.PMode = sendingPMode;
            }

            return StepResult.Success(messagingContext);
        }

        private static IDynamicDiscoveryProfile ResolveDynamicDiscoveryProfile(string smpProfile)
        {
            if (smpProfile == null)
            {
                Logger.Debug($"SendingPMode doesn't specify DynamicDiscovery.SmpProfile element, using default: {nameof(LocalDynamicDiscoveryProfile)}");
                return new LocalDynamicDiscoveryProfile();
            }

            if (!GenericTypeBuilder.CanResolveTypeImplementedBy<IDynamicDiscoveryProfile>(smpProfile))
            {
                throw new InvalidOperationException(
                    "SendingPMode.DynamicDiscovery.SmpProfile element doesn't have a fully-qualified assembly name "
                    + $"that can be used to resolve a instance that implements the {nameof(IDynamicDiscoveryProfile)} interface");
            }

            Logger.Debug($"SendingPMode specifies a DynamicDiscovery.SmpProfile element, resolve using: {smpProfile}");
            return GenericTypeBuilder
                .FromType(smpProfile)
                .Build<IDynamicDiscoveryProfile>();
        }

        private static async Task<SendingProcessingMode> DynamicDiscoverSendingPModeAsync(
            MessagingContext messagingContext,
            IDynamicDiscoveryProfile profile,
            AS4Party toParty)
        {
            var clonedPMode = (SendingProcessingMode)messagingContext.SendingPMode.Clone();
            clonedPMode.Id = $"{clonedPMode.Id}_SMP";

            XmlDocument smpMetaData = await RetrieveSmpMetaDataAsync(profile, clonedPMode.DynamicDiscovery, toParty);
            if (smpMetaData == null)
            {
                throw new ArgumentNullException(
                    nameof(smpMetaData),
                    $@"No SMP medata data document was retrieved by the Dynamic Discovery profile: {profile.GetType().Name}");
            }

            SendingProcessingMode sendingPMode = profile.DecoratePModeWithSmpMetaData(clonedPMode, smpMetaData);
            if (sendingPMode == null)
            {
                throw new ArgumentNullException(
                    nameof(sendingPMode),
                    $@"No decorated SendingPMode was returned by the Dynamic Discovery profile: {profile.GetType().Name}");
            }


            ValidatePMode(sendingPMode);
            return sendingPMode;
        }

        private static async Task<XmlDocument> RetrieveSmpMetaDataAsync(
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
                (dynamicDiscovery.Settings ?? new DynamicDiscoverySetting[0])
                ?.ToDictionary(s => s?.Key, s => s?.Value);

            return await profile.RetrieveSmpMetaData(
                party: toParty, 
                properties: customProperties);
        }

        private static void ValidatePMode(SendingProcessingMode pmode)
        {
            SendingProcessingModeValidator
                .Instance
                .Validate(pmode)
                .Result(
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
                Logger.Debug(
                    "Resolve ToParty in a Forwarding scenario from AS4Message's "
                    + $"primary messsage unit {m.MessageId} {{MessageType=UserMessage}}");

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
            if (allowOverride == false
                && submitParty != null 
                && pmodeParty != null 
                && submitParty.Equals(pmodeParty) == false)
            {
                throw new NotSupportedException(
                    "SubmitMessage is not allowed by the SendingPMode to override ToParty");
            }

            if (submitParty == null && pmodeParty == null)
            {
                throw new InvalidOperationException(
                    "Either the SubmitMessage or the SendingPMode is required to have a " + 
                    "ToParty configured for dynamic discovery in a non-Forwarding scenario");
            }

            if (submitParty != null)
            {
                Logger.Debug(
                    "Resolve ToParty in non-Forwarding scenario from SubmitMessage "
                    + "because SendingPMode allows overriding {AllowOverride = true}");

                return CreateToPartyFrom(
                    "SubmitMessage", 
                    submitParty.Role, 
                    (submitParty.PartyIds ?? Enumerable.Empty<SubmitPartyId>())
                        .Select(p => new PartyId(p?.Id, p?.Type.AsMaybe())));
            }

            if (pmodeParty?.PartyIds.Any() == false)
            {
                
                throw new ConfigurationErrorsException(
                    "Cannot retrieve SMP metadata: SendingPMode must contain at lease one "
                    + "<ToPartyId/> element in the MessagePackaging.PartyInfo.ToParty element");
            }

            Logger.Debug("Resolve ToParty in non-Forwarding scenario from SendingPMode because SubmitMessage has none");
            return CreateToPartyFrom(
                "SendingPMode", 
                pmodeParty.Role, 
                (pmodeParty.PartyIds ?? Enumerable.Empty<PModePartyId>())
                    .Select(p => new PartyId(p?.Id, p?.Type.AsMaybe())));
        }

        private static AS4Party CreateToPartyFrom(string log, string role, IEnumerable<PartyId> ids)
        {
            try
            {
                return new AS4Party(role, ids);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidDataException($"{log} has an incomplete ToParty: {ex.Message}");
            }
        }
    }
}
