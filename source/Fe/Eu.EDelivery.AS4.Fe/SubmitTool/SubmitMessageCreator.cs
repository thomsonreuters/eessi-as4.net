using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Monitor;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    ///     Implementation of ISubmitMessageCreator
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.ISubmitMessageCreator" />
    public class SubmitMessageCreator : ISubmitMessageCreator
    {
        private readonly IClient client;
        private readonly IEnumerable<IMessageHandler> messageHandlers;
        private readonly IOptions<SubmitToolOptions> options;
        private readonly IEnumerable<IPayloadHandler> payloadHandlers;
        private readonly IPmodeService pmodeService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SubmitMessageCreator" /> class.
        /// </summary>
        /// <param name="pmodeService">The pmode service.</param>
        /// <param name="payloadHandlers">The payload handlers.</param>
        /// <param name="messageHandlers">The message handlers.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="client">The SignalR client.</param>
        public SubmitMessageCreator(IPmodeService pmodeService, IEnumerable<IPayloadHandler> payloadHandlers, IEnumerable<IMessageHandler> messageHandlers, IOptions<SubmitToolOptions> options, IClient client)
        {
            this.pmodeService = pmodeService;
            this.payloadHandlers = payloadHandlers;
            this.messageHandlers = messageHandlers;
            this.options = options;
            this.client = client;
        }

        /// <summary>
        ///     Submit one or more message(s)
        /// </summary>
        /// <param name="submitInfo">The submit information.</param>
        /// <returns></returns>
        /// <exception cref="BusinessException">
        ///     Missing to location
        ///     or
        ///     Missing payload location
        ///     or
        ///     Invalid number of submit messages value. Only a value between 1 &amp; 999 is allowed.
        ///     or
        ///     Could not find pmode
        /// </exception>
        public async Task CreateSubmitMessages(MessagePayload submitInfo)
        {
            try
            {
                if (submitInfo.NumberOfSubmitMessages <= 0 || submitInfo.NumberOfSubmitMessages > 999) throw new BusinessException("Invalid number of submit messages value. Only a value between 1 & 999 is allowed.");

                client.SendInfo($"Looking up PMode {submitInfo.SendingPmode}");
                var sendingPmode = await pmodeService.GetSendingByName(submitInfo.SendingPmode);
                if (sendingPmode == null) throw new BusinessException("Could not find PMode");
                client.SendPmode(AS4XmlSerializer.ToString(sendingPmode.Pmode));

                await CreateSubmitMessageObjects(submitInfo, sendingPmode.Pmode, options.Value.PayloadHttpAddress, options.Value.ToHttpAddress);
            }
            catch (Exception ex)
            {
                client.SendError(ex.Message);
                throw;
            }
        }

        private async Task CreateSubmitMessageObjects(MessagePayload submitInfo, SendingProcessingMode sendingPmode, string payloadDestination, string messageDestination)
        {
            var payloads = await CreatePayloads(submitInfo, payloadDestination);

            for (var i = 0; i < submitInfo.NumberOfSubmitMessages; i++)
            {
                client.SendInfo($"Submitting message {i + 1} of {submitInfo.NumberOfSubmitMessages} to {messageDestination}");
                var submitMessage = BuildMessage(submitInfo, sendingPmode, payloads);
                await SubmitMessage(submitMessage, messageDestination);
            }
        }

        private async Task<List<FilePayload>> CreatePayloads(MessagePayload submitInfo, string payloadDestination)
        {
            var payloads = new List<FilePayload>();

            foreach (var payloadInfo in submitInfo.Files)
            {
                var messagePayload = new FilePayload
                {
                    MimeType = payloadInfo.ContentType,
                    Location = await ProcessFile(payloadInfo.Data, payloadInfo.FileName, payloadDestination),
                    FileName = payloadInfo.FileName
                };

                payloads.Add(messagePayload);
            }
            return payloads;
        }

        private SubmitMessage BuildMessage(MessagePayload submitInfo, SendingProcessingMode sendingPmode, List<FilePayload> payloads)
        {
            var submitMessage = new SubmitMessage {MessageInfo = {MessageId = $"{Guid.NewGuid()}@{Environment.MachineName}"}};
            submitMessage.Collaboration.AgreementRef.PModeId = sendingPmode.Id;

            submitMessage.Payloads = payloads.Select(x => x.ToPayload(CreatePayloadId(submitInfo, x.FileName, submitMessage.MessageInfo.MessageId))).ToArray();
            return submitMessage;
        }

        private string CreatePayloadId(MessagePayload submitInfo, string fileName, string messageId)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            return submitInfo.NumberOfSubmitMessages > 1 ? $"{messageId}.{name}" : name;
        }

        private async Task<string> ProcessFile(Stream stream, string fileName, string toLocation)
        {
            client.SendInfo($"Submitting payload \"{fileName}\" to {toLocation}");
            var handler = payloadHandlers.FirstOrDefault(x => x.CanHandle(toLocation));
            if (handler == null) throw new Exception($"No payload handler found for {toLocation}");
            var result = await handler.Handle(toLocation, fileName, stream);
            client.SendInfo($"\"{fileName}\" has id {result}");
            return result;
        }

        private async Task SubmitMessage(SubmitMessage message, string toLocation)
        {
            client.SendAs4Message(AS4XmlSerializer.ToString(message), message.MessageInfo.MessageId);
            var handler = messageHandlers.First(x => x.CanHandle(toLocation));
            if (handler == null) throw new Exception($"No message handler found for {toLocation}");
            await handler.Handle(message, toLocation);
        }
    }
}
