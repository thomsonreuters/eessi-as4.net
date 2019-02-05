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
using InvalidOperationException = System.InvalidOperationException;
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
                Logger.Error(
                    $"{nameof(DynamicDiscoveryStep)} requires an AS4Message when used in a Forward Agent, "
                    + "please make sure that the ReceivedMessage is deserialized before executing this step."
                    + $"{Environment.NewLine} Possibly this failure happened because the Transformer of the Forward Agent is still using "
                    + "the ForwardMessageTransformer instead of the AS4MessageTransformer");

                throw new InvalidOperationException(
                    "Dynamic Discovery process cannot be used in a Forwarding scenario for messages that are not AS4Messages");
            }

            if (messagingContext.SendingPMode == null 
                || messagingContext.SendingPMode.DynamicDiscoverySpecified == false)
            {
                Logger.Trace($"Skip Dynamic Discovery because SendingPMode {messagingContext.SendingPMode?.Id} is not configured for Dynamic Discovery");
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

            DynamicDiscoveryResult result = await DynamicDiscoverSendingPModeAsync(messagingContext.SendingPMode, profile, toParty);
            Logger.Debug($"SendingPMode {result.CompletedSendingPMode.Id} completed with SMP metadata");

            messagingContext.SendingPMode = result.CompletedSendingPMode;
            if (messagingContext.SubmitMessage != null && result.OverrideToParty)
            {
                if (messagingContext.SubmitMessage?.PartyInfo != null)
                {
                    messagingContext.SubmitMessage.PartyInfo.ToParty = null;
                }

                messagingContext.SubmitMessage.PMode = result.CompletedSendingPMode;
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

            if (!GenericTypeBuilder.CanResolveTypeThatImplements<IDynamicDiscoveryProfile>(smpProfile))
            {
                Logger.Error(
                    "SendingPMode.DynamicDiscovery.SmpProfile element doesn't have a fully-qualified assembly name "
                    + $"that can be used to resolve a instance that implements the {nameof(IDynamicDiscoveryProfile)} interface");

                throw new InvalidOperationException(
                    "Dynamic Discovery process was not correctly configured");
            }

            Logger.Debug($"SendingPMode specifies a DynamicDiscovery.SmpProfile element, resolve using: {smpProfile}");
            return GenericTypeBuilder
                .FromType(smpProfile)
                .Build<IDynamicDiscoveryProfile>();
        }

        private static async Task<DynamicDiscoveryResult> DynamicDiscoverSendingPModeAsync(
            SendingProcessingMode sendingPMode,
            IDynamicDiscoveryProfile profile,
            AS4Party toParty)
        {
            try
            {
                var clonedPMode = (SendingProcessingMode)sendingPMode.Clone();
                clonedPMode.Id = $"{clonedPMode.Id}_SMP";

                XmlDocument smpMetaData = await RetrieveSmpMetaDataAsync(profile, clonedPMode.DynamicDiscovery, toParty);
                if (smpMetaData == null)
                {
                    Logger.Error($"No SMP meta-data document was retrieved by the Dynamic Discovery profile: {profile.GetType().Name}");
                    throw new InvalidDataException(
                        "No SMP meta-data document was retrieved during the Dynamic Discovery process");
                }

                DynamicDiscoveryResult result = profile.DecoratePModeWithSmpMetaData(clonedPMode, smpMetaData);
                if (result == null)
                {
                    Logger.Error($@"No decorated SendingPMode was returned by the Dynamic Discovery profile: {profile.GetType().Name}");
                    throw new InvalidDataException(
                        "No decorated SendingPMode was returned during the Dynamic Discovery");
                }

                ValidatePMode(result.CompletedSendingPMode);
                return result;
            }
            catch (Exception ex) 
            {
                Logger.Error(
                    $"An exception occured during the Dynamic Discovery process of the profile: {profile.GetType().Name} "
                    + $"with the message having ToParty={toParty} for SendingPMode {sendingPMode.Id}");

                throw new DynamicDiscoveryException(
                    "An exception occured during the Dynamic Discovery process", ex);
            }
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
                ?.ToDictionary(s => s?.Key, s => s?.Value, StringComparer.OrdinalIgnoreCase);

            return await profile.RetrieveSmpMetaDataAsync(
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
                "Only AS4Message with an UserMessage as primary message unit can be used dynamically discover the SendingPMode");
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
                Logger.Error(
                    "Cannot retrieve SMP metadata because SendingPMode must contain at lease one "
                    + "<ToPartyId/> element in the MessagePackaging.PartyInfo.ToParty element");

                throw new ConfigurationErrorsException(
                    "Cannot retrieve SMP metadata because the message is referencing an incomplete SendingPMode");
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

    /// <summary>
    /// Represents a exception that occurs during the dynamic discovery process.
    /// </summary>
    [Serializable]
    public class DynamicDiscoveryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDiscoveryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the exception.</param>
        public DynamicDiscoveryException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDiscoveryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DynamicDiscoveryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
