using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    [Info("Validate received AS4 Message")]
    [Description("Verify if the received AS4 Message is valid for further processing")]
    public class ValidateAS4MessageStep : IStep
    {
        private static readonly XmlNamespaceManager Namespaces = new XmlNamespaceManager(new NameTable());
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static ValidateAS4MessageStep()
        {
            Namespaces.AddNamespace("soap12", Constants.Namespaces.Soap12);
        }

        /// <summary>
        /// Execute the step for a given <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context, CancellationToken cancellationToken)
        {
            Logger.Info("Validating the received AS4 Message ...");

            if (SoapBodyIsNotEmpty(context.AS4Message))
            {                
                context.ErrorResult = SoapBodyAttachmentsNotSupported();
                Logger.Error($"AS4 Message {context.AS4Message.GetPrimaryMessageId()} is not valid: {context.ErrorResult.Description}");
                return StepResult.Failed(context);
            }

            IEnumerable<PartInfo> invalidPartInfos =
               context.AS4Message.UserMessages.SelectMany(
                   message => message.PayloadInfo.Where(payload => payload.Href == null || payload.Href.StartsWith("cid:") == false));

            if (invalidPartInfos.Any())
            {
                context.ErrorResult = ExternalPayloadError(invalidPartInfos);
                Logger.Error($"AS4 Message {context.AS4Message.GetPrimaryMessageId()} is not valid: {context.ErrorResult.Description}");
                return StepResult.Failed(context);
            }

            if (context.AS4Message.IsUserMessage)
            {
                AS4Message message = context.AS4Message;

                bool noAttachmentCanBeFounForEachPartInfo =
                    message.PrimaryUserMessage.PayloadInfo?.Count(
                        p => message.Attachments.FirstOrDefault(a => a.Matches(p)) == null) > 0;

                if (noAttachmentCanBeFounForEachPartInfo)
                {
                    context.ErrorResult = InvalidHeaderError();
                    Logger.Error($"AS4 Message {context.AS4Message.GetPrimaryMessageId()} is not valid: {context.ErrorResult.Description}");
                    return StepResult.Failed(context);
                }
            }

            Logger.Info("Received AS4 Message is valid");

            return StepResult.Success(context);
        }

        private static bool SoapBodyIsNotEmpty(AS4Message message)
        {
            var bodyNode = message.EnvelopeDocument.SelectSingleNode("/soap12:Envelope/soap12:Body", Namespaces);

            if (bodyNode != null && String.IsNullOrWhiteSpace(bodyNode.InnerText) == false)
            {
                return true;
            }
            return false;
        }

        private static ErrorResult SoapBodyAttachmentsNotSupported()
        {
            return new ErrorResult("Attachments in the soap body are not supported.", ErrorAlias.FeatureNotSupported);
        }

        private static ErrorResult ExternalPayloadError(IEnumerable<PartInfo> invalidPartInfos)
        {
            return new ErrorResult(
                "Attachments must be embedded in the MIME message and must be referred to in the PayloadInfo section using a PartyInfo with a cid href reference.",
                ErrorAlias.ExternalPayloadError);
        }

        private static ErrorResult InvalidHeaderError()
        {
            return new ErrorResult(
                "No Attachment can be found for each UserMessage PartInfo",
                ErrorAlias.InvalidHeader);
        }
    }
}
