using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class ValidateAS4MessageStep : IStep
    {
        private static readonly XmlNamespaceManager _namespaces = new XmlNamespaceManager(new NameTable());

        static ValidateAS4MessageStep()
        {
            _namespaces.AddNamespace("soap12", Constants.Namespaces.Soap12);
        }

        /// <summary>
        /// Execute the step for a given <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context, CancellationToken cancellationToken)
        {
            if (SoapBodyIsNotEmpty(context.AS4Message))
            {
                context.ErrorResult = FeatureNotSupportedError();
                return StepResult.Failed(context);
            }

            IEnumerable<PartInfo> invalidPartInfos =
               context.AS4Message.UserMessages.SelectMany(
                   message => message.PayloadInfo.Where(payload => payload.Href == null || payload.Href.StartsWith("cid:") == false));

            if (invalidPartInfos.Any())
            {
                context.ErrorResult = ExternalPayloadError(invalidPartInfos);
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
                    return StepResult.Failed(context);
                }
            }

            return await StepResult.SuccessAsync(context);
        }

        private static bool SoapBodyIsNotEmpty(AS4Message message)
        {
            var bodyNode = message.EnvelopeDocument.SelectSingleNode("/soap12:Envelope/soap12:Body", _namespaces);

            if (bodyNode != null && String.IsNullOrWhiteSpace(bodyNode.InnerText) == false)
            {
                return true;
            }
            return false;
        }

        private static ErrorResult FeatureNotSupportedError()
        {
            return new ErrorResult("Attachments in the soap body are not supported.", ErrorAlias.FeatureNotSupported);
        }

        private static ErrorResult ExternalPayloadError(IEnumerable<PartInfo> invalidPartInfos)
        {
            string hrefs = string.Join(",", invalidPartInfos.Select(i => $"'{i.Href}'"));

            return new ErrorResult(
                $"AS4Message only support embedded Payloads and: '{hrefs}' was given",
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
