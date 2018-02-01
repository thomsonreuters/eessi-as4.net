using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the attachments of an AS4 message must be compressed.
    /// </summary>
    [Description("This step compresses the attachments of an AS4 Message if compression is enabled in the sending PMode.")]
    [Info("Compress attachments")]
    public class CompressAttachmentsStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private MessagingContext _messagingContext;

        /// <summary>
        /// Compress the <see cref="AS4Message" /> if required
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (!messagingContext.SendingPMode.MessagePackaging.UseAS4Compression)
            {
                return ReturnSameMessagingContext(messagingContext);
            }

            _messagingContext = messagingContext;
            TryCompressAS4Message(messagingContext.AS4Message);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static StepResult ReturnSameMessagingContext(MessagingContext messagingContext)
        {
            Logger.Debug($"Sending PMode {messagingContext.SendingPMode.Id} Compression is disabled");
            return StepResult.Success(messagingContext);
        }

        private void TryCompressAS4Message(AS4Message message)
        {
            try
            {
                Logger.Info(
                    $"{_messagingContext.EbmsMessageId} Compress AS4 Message Attachments with GZip Compression");

                message.CompressAttachments();
            }
            catch (SystemException exception)
            {
                throw ThrowAS4CompressingException(exception);
            }
        }

        private static Exception ThrowAS4CompressingException(Exception innerException)
        {
            const string description = "Attachments cannot be compressed";
            Logger.Error(description);

            return new InvalidDataException(description, innerException);
        }
    }
}