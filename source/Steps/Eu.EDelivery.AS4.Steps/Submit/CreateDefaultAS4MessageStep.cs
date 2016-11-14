using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep"/> implementation 
    /// to create a default configured <see cref="AS4Message"/>
    /// </summary>
    public class CreateDefaultAS4MessageStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDefaultAS4MessageStep"/>
        /// </summary>
        public CreateDefaultAS4MessageStep()
        {
            this._config = Config.Instance;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start creating a <see cref="AS4Message"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            AddDefaultAS4Message(internalMessage);
            this._logger.Info($"{internalMessage.Prefix} Default AS4 Message is created");

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AddDefaultAS4Message(InternalMessage internalMessage)
        {
            SendingProcessingMode pmode = this._config.GetSendingPMode("default-pmode");

            UserMessage userMessage = UserMessageFactory.Instance.Create(pmode);
            internalMessage.AS4Message.UserMessages.Add(userMessage);
            internalMessage.AS4Message.SendingPMode = pmode;
            AddPartInfos(internalMessage.AS4Message);
        }

        private void AddPartInfos(AS4Message as4Message)
        {
            as4Message.PrimaryUserMessage.PayloadInfo = new List<PartInfo>();
            foreach (Attachment attachment in as4Message.Attachments)
                AddPartInfo(as4Message, attachment);
        }

        private void AddPartInfo(AS4Message as4Message, Attachment attachment)
        {
            PartInfo partInfo = CreateAttachmentPartInfo(attachment);
            as4Message.PrimaryUserMessage.PayloadInfo.Add(partInfo);
        }

        private PartInfo CreateAttachmentPartInfo(Attachment attachment)
        {
            return new PartInfo("cid:" + attachment.Id)
            {
                Properties = new Dictionary<string, string> {["MimeType"] = attachment.ContentType}
            };
        }
    }
}