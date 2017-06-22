using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class ValidateAS4MessageStep : IStep
    {
        /// <summary>
        /// Execute the step for a given <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context, CancellationToken cancellationToken)
        {
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

        private static ErrorResult ExternalPayloadError(IEnumerable<PartInfo> invalidPartInfos)
        {
            string hrefs = string.Join(",", invalidPartInfos.Select(i => $"'{i.Href}'"));

            return new ErrorResult(
                $"AS4Message only support embedded Payloads and: '{hrefs}' was given",
                ErrorCode.Ebms0011,
                ErrorAlias.ExternalPayloadError);
        }

        private static ErrorResult InvalidHeaderError()
        {
            return new ErrorResult(
                "No Attachment can be found for each UserMessage PartInfo",
                ErrorCode.Ebms0009,
                ErrorAlias.InvalidHeader);
        }
    }
}
