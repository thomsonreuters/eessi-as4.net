using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.Journal;
using log4net;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the attachments of an AS4 message must be compressed.
    /// </summary>
    [Info("Compress AS4 Message attachments if necessary")]
    [Description("This step compresses the attachments of an AS4 Message if compression is enabled in the sending PMode.")]
    public class CompressAttachmentsStep : IStep
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        /// <summary>
        /// Compress the <see cref="AS4Message" /> if required
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
                    $"{nameof(CompressAttachmentsStep)} requires an AS4Message to compress it attachments but no AS4Message is present in the MessagingContext");
            }

            if (messagingContext.SendingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CompressAttachmentsStep)} requires a Sending Processing Mode to use during the compression but no Sending Processing Mode is present in the MessagingContext");
            }

            if (messagingContext.SendingPMode.MessagePackaging?.UseAS4Compression == false)
            {
                Logger.Trace($"No compression will happen because the SendingPMode {Config.Encode(messagingContext.SendingPMode.Id)} MessagePackaging.UseAS4Compression is disabled");
                return StepResult.Success(messagingContext);
            }

            try
            {
                Logger.Info($"(Outbound)[{Config.Encode(messagingContext.AS4Message.GetPrimaryMessageId())}] Compress AS4Message attachments with GZip compression");
                messagingContext.AS4Message.CompressAttachments();
            }
            catch (SystemException exception)
            {
                const string description = "Attachments cannot be compressed because of an exception";
                Logger.Error(Config.Encode(description));

                throw new InvalidDataException(description, exception);
            }

            JournalLogEntry journal = 
                JournalLogEntry.CreateFrom(
                    messagingContext.AS4Message,
                    $"Compressed {messagingContext.AS4Message.Attachments.Count()} attachments with GZip compression");

            return await StepResult
                .Success(messagingContext)
                .WithJournalAsync(journal);
        }
    }
}