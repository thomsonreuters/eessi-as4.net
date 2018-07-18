using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the attachments of an AS4 message must be compressed.
    /// </summary>
    [Info("Compress AS4 Message attachments if necessary")]
    [Description("This step compresses the attachments of an AS4 Message if compression is enabled in the sending PMode.")]
    public class CompressAttachmentsStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Compress the <see cref="AS4Message" /> if required
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CompressAttachmentsStep)} requires an AS4Message to compress it attachments but hasn't got one");
            }

            if (messagingContext.SendingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CompressAttachmentsStep)} requires a Sending Processing Mode to use during the compression but hasn't got one");
            }

            if (messagingContext.SendingPMode.MessagePackaging?.UseAS4Compression == false)
            {
                Logger.Debug(
                    $"No compression will happen because the SendingPMode {messagingContext.SendingPMode.Id} " + 
                    "MessagePackaging.UseAS4Compression is disabled");

                return StepResult.Success(messagingContext);
            }

            CompressAS4Message(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static void CompressAS4Message(MessagingContext context)
        {
            try
            {
                Logger.Info(
                    $"{context.LogTag} Compress AS4Message attachments with GZip compression");

                context.AS4Message.CompressAttachments();
            }
            catch (SystemException exception)
            {
                const string description = "(Receive) Attachments cannot be compressed because of an exception";
                Logger.Error(description);

                throw new InvalidDataException(description, exception);
            }
        }
    }
}