using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
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
            _config = config;
        }

        /// <summary>
        /// Configure the step with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            _properties = properties;
        }

        /// <summary>
        /// Start creating a <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            AddDefaultAS4Message(messagingContext);
            Logger.Info($"{messagingContext.Prefix} Default AS4 Message is created");

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void AddDefaultAS4Message(MessagingContext messagingContext)
        {
            SendingProcessingMode pmode = GetDefaultPMode();

            UserMessage userMessage = UserMessageFactory.Instance.Create(pmode);
            messagingContext.AS4Message.UserMessages.Add(userMessage);
            messagingContext.SendingPMode = pmode;
            AddPartInfos(messagingContext.AS4Message);
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
            as4Message.PrimaryUserMessage.PayloadInfo.Add(partInfo);
        }

        private static PartInfo CreateAttachmentPartInfo(Attachment attachment)
        {
            return new PartInfo("cid:" + attachment.Id)
            {
                Properties = new Dictionary<string, string> {["MimeType"] = attachment.ContentType}
            };
        }
    }
}