using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep" /> implementation
    /// to create a default configured <see cref="AS4Message" />
    /// </summary>
    [NotConfigurable]
    public class CreateDefaultAS4MessageStep : IConfigStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfig _config;

        private IDictionary<string, string> _properties;

        [Info("Default pmode", type: "pmode")]
        [Description("The default pmode to be used to create a message.")]
        private string DefaultPmode => _properties?.ReadOptionalProperty("default-pmode");

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDefaultAS4MessageStep" /> class.
        /// </summary>
        public CreateDefaultAS4MessageStep() : this(Config.Instance) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDefaultAS4MessageStep" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public CreateDefaultAS4MessageStep(IConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        /// <summary>
        /// Configure the step with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            _properties = properties;
        }

        /// <summary>
        /// Start creating a <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateDefaultAS4MessageStep)} requires an AS4Message to assign the default UserMessage to but no AS4Message is present in the MessagingContext");
            }

            SendingProcessingMode pmode = GetDefaultPMode();

            UserMessage userMessage = UserMessageFactory.Instance.Create(pmode);
            messagingContext.AS4Message.AddMessageUnit(userMessage);
            messagingContext.SendingPMode = pmode;
            AddPartInfos(messagingContext.AS4Message);

            Logger.Info($"{messagingContext.LogTag} Default AS4Message is created using SendingPMode {pmode.Id}");
            return await StepResult.SuccessAsync(messagingContext);
        }

        private SendingProcessingMode GetDefaultPMode()
        {
            return _config.GetSendingPMode(DefaultPmode);
        }

        private static void AddPartInfos(AS4Message as4Message)
        {
            foreach (Attachment attachment in as4Message.Attachments)
            {
                AddPartInfo(as4Message, attachment);
            }
        }

        private static void AddPartInfo(AS4Message as4Message, Attachment attachment)
        {
            PartInfo partInfo = CreateAttachmentPartInfo(attachment);
            as4Message.FirstUserMessage.AddPartInfo(partInfo);
        }

        private static PartInfo CreateAttachmentPartInfo(Attachment attachment)
        {
            return new PartInfo(
                href: "cid:" + attachment.Id, 
                properties: new Dictionary<string, string> { ["MimeType"] = attachment.ContentType }, 
                schemas: new Schema[0]);
        }
    }
}