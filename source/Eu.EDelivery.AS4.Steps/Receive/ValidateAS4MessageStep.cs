using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context)
        {
            if (context?.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ValidateAS4MessageStep)} requires an AS4Message to validate but no AS4Message is present in the MessagingContext");
            }

            Logger.Trace("Validating the received AS4Message ...");
            if (SoapBodyIsNotEmpty(context.AS4Message))
            {                
                context.ErrorResult = SoapBodyAttachmentsNotSupported();
                return ValidationFailure(context);
            }

            IEnumerable<PartInfo> invalidPartInfos =
               context.AS4Message.UserMessages.SelectMany(
                   message => message.PayloadInfo.Where(payload => payload.Href.StartsWith("cid:") == false));

            if (invalidPartInfos.Any())
            {
                context.ErrorResult = ExternalPayloadError();
                return ValidationFailure(context);
            }

            if (context.AS4Message.IsUserMessage)
            {
                AS4Message message = context.AS4Message;

                bool noAttachmentCanBeFoundForEachPartInfo =
                    message.FirstUserMessage.PayloadInfo?.Count(
                        p => message.Attachments.FirstOrDefault(a => a.Matches(p)) == null) > 0;

                if (noAttachmentCanBeFoundForEachPartInfo)
                {
                    context.ErrorResult = AttachmentNotFoundInvalidHeaderError();
                    return ValidationFailure(context);
                }

                if (message.FirstUserMessage.PayloadInfo?.GroupBy(p => p.Href).All(g => g.Count() == 1) == false)
                {
                    context.ErrorResult = DuplicateAttachmentInvalidHeaderError();
                    return ValidationFailure(context);
                }
            }

            Logger.Info($"{context.LogTag} Received AS4Message is valid");
            return await StepResult.SuccessAsync(context);
        }

        private static bool SoapBodyIsNotEmpty(AS4Message message)
        {
            XmlNode bodyNode = message.EnvelopeDocument.SelectSingleNode("/soap12:Envelope/soap12:Body", Namespaces);

            return bodyNode != null && String.IsNullOrWhiteSpace(bodyNode.InnerText) == false;
        }

        private static ErrorResult SoapBodyAttachmentsNotSupported()
        {
            return new ErrorResult(
                "Attachments in the SOAP body are not supported", 
                ErrorAlias.FeatureNotSupported);
        }

        private static ErrorResult ExternalPayloadError()
        {
            return new ErrorResult(
                "Attachments must be embedded in the MIME message and must be referred " + 
                "to in the PayloadInfo section using a PartyInfo with a cid href reference",
                ErrorAlias.ExternalPayloadError);
        }

        private static ErrorResult AttachmentNotFoundInvalidHeaderError()
        {
            return new ErrorResult(
                "No Attachment can be found for each UserMessage PartInfo",
                ErrorAlias.InvalidHeader);
        }

        private static ErrorResult DuplicateAttachmentInvalidHeaderError()
        {
            return new ErrorResult(
                "AS4 Message is not allowed because it contains payloads that have the same PayloadId",
                ErrorAlias.InvalidHeader);
        }

        private static StepResult ValidationFailure(MessagingContext context)
        {
            Logger.Error($"{context.LogTag} AS4Message is not valid: {context.ErrorResult.Description}");
            return StepResult.Failed(context);
        }
    }
}
