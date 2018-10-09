using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Decompress the incoming Payloads
    /// </summary>
    [Info("Decompress attachments")]
    [Description("If necessary, decompresses the attachments that are present in the received message.")]
    public class DecompressAttachmentsStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Decompress any Attachments
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
                    $"{nameof(DecompressAttachmentsStep)} requires a AS4Message but no AS4Message is present in the MessagingContext");
            }

            if (messagingContext.ReceivedMessageMustBeForwarded)
            {
                Logger.Debug("No decompression will happen because the incoming AS4Message must be forwarded unchanged");
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.AS4Message.HasAttachments == false)
            {
                Logger.Debug("No decompression will happen because the AS4Message hasn't got any attachments to decompress");
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.AS4Message.IsEncrypted)
            {
                Logger.Warn("Incoming attachmets are still encrypted will fail to decompress correctly");
            }

            try
            {
                messagingContext.AS4Message.DecompressAttachments();
                return await StepResult.SuccessAsync(messagingContext);
            }
            catch (Exception exception) 
            when (exception is ArgumentException
                  || exception is ObjectDisposedException
                  || exception is InvalidDataException)
            {
                if (messagingContext.AS4Message.IsEncrypted)
                {
                    Logger.Error(
                        "Decompression failed because the incoming attachments are still encrypted. "
                        + "Make sure that you specify <Encryption/> information in the <Security/> element of the SendingPMode "
                        + "so the attachments are first decrypted before decompressed");
                }

                messagingContext.ErrorResult = new ErrorResult(exception.Message, ErrorAlias.DecompressionFailure);
                return StepResult.Failed(messagingContext);
            }
        }
    }
}