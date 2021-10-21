using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Common;
using log4net;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    [Info("Validate received AS4 Message")]
    [Description("Verify if the received AS4 Message is valid for further processing")]
    public class ValidateAS4MessageStep : IStep
    {
        private static readonly XmlNamespaceManager Namespaces = new XmlNamespaceManager(new NameTable());
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.AS4Message == null)
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

            IEnumerable<PartInfo> notSupportedPartInfos =
               context.AS4Message.UserMessages.SelectMany(
                   message => message.PayloadInfo.Where(payload => payload.Href.StartsWith("cid:") == false));

            if (notSupportedPartInfos.Any())
            {
                context.ErrorResult = ExternalPayloadError(notSupportedPartInfos);
                return ValidationFailure(context);
            }

            if (context.AS4Message.IsUserMessage)
            {
                AS4Message message = context.AS4Message;

                IEnumerable<PartInfo> unreferencedPartInfos =
                    message.UserMessages
                           .SelectMany(u => u.PayloadInfo)
                           .Where(p => message.Attachments.FirstOrDefault(a => a.Matches(p)) == null);

                if (unreferencedPartInfos.Any())
                {
                    context.ErrorResult = AttachmentNotFoundInvalidHeaderError(unreferencedPartInfos);
                    return ValidationFailure(context);
                }

                IEnumerable<IGrouping<string, PartInfo>> duplicatePartInfos = 
                    message.UserMessages
                           .SelectMany(u => u.PayloadInfo)
                           .GroupBy(p => p.Href)
                           .Where(g => g.Count() != 1);

                if (duplicatePartInfos.Any())
                {
                    context.ErrorResult = DuplicateAttachmentInvalidHeaderError(duplicatePartInfos);
                    return ValidationFailure(context);
                }
            }

            Logger.Trace($"{Config.Encode(context.LogTag)} Received AS4Message is valid");
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
                "AS4Message is not supported because there exists attachments in the SOAP body", 
                ErrorAlias.FeatureNotSupported);
        }

        private static ErrorResult ExternalPayloadError(IEnumerable<PartInfo> notSupportedPartInfos)
        {
            return new ErrorResult(
                "Not all attachments are embedded in the MIME message and are referred "
                + $"in the PayloadInfo section using a PartInfo with a cid href reference: {String.Join(", ", notSupportedPartInfos.Select(p => p.Href))}",
                ErrorAlias.ExternalPayloadError);
        }

        private static ErrorResult AttachmentNotFoundInvalidHeaderError(IEnumerable<PartInfo> unreferencedPartInfos)
        {
            return new ErrorResult(
                $"No attachment can be found this/these PartInfo(s) in the UserMessage: {String.Join(", ", unreferencedPartInfos.Select(p => p.Href))}",
                ErrorAlias.InvalidHeader);
        }

        private static ErrorResult DuplicateAttachmentInvalidHeaderError(
            IEnumerable<IGrouping<string, PartInfo>> duplicatePartInfos)
        {
            return new ErrorResult(
                $"AS4Message is invalid because it contains duplicate PartInfo elements: {String.Join(", ", duplicatePartInfos.Select(g => g.Key))}",
                ErrorAlias.InvalidHeader);
        }

        private static StepResult ValidationFailure(MessagingContext context)
        {
            Logger.Error($"{Config.Encode(context.LogTag)} AS4Message is not valid: {Config.Encode(context.ErrorResult.Description)}");
            return StepResult.Failed(context);
        }
    }
}
