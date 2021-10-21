using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.Journal;
using log4net;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Decompress the incoming Payloads
    /// </summary>
    [Info("Decompress attachments")]
    [Description("If necessary, decompresses the attachments that are present in the received message.")]
    public class DecompressAttachmentsStep : IStep
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
                Logger.Info("(Receive) AS4Message is decompressed correctly");

                JournalLogEntry entry =
                    JournalLogEntry.CreateFrom(
                        messagingContext.AS4Message,
                        $"Decompressed {messagingContext.AS4Message.Attachments.Count()} with GZip compression");

                return await StepResult
                    .Success(messagingContext)
                    .WithJournalAsync(entry);
            }
            catch (Exception exception) 
            when (exception is ArgumentException
                  || exception is ObjectDisposedException
                  || exception is InvalidDataException)
            {
                var description = "Decompression failed due to an exception";

                if (messagingContext.AS4Message.IsEncrypted)
                {
                    Logger.Error(
                        "Decompression failed because the incoming attachments are still encrypted. "
                        + "Make sure that you specify <Decryption/> information in the <Security/> element of the "
                        + "ReceivingPMode so the attachments are first decrypted before decompressed");

                    description = "Decompression failed because the incoming attachments are still encrypted";
                }

                messagingContext.ErrorResult = new ErrorResult(
                    description, 
                    ErrorAlias.DecompressionFailure);

                return StepResult.Failed(messagingContext);
            }
        }
    }
}