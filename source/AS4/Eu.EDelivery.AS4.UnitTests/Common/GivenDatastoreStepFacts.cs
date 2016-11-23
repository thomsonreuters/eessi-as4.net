using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.UnitTests.Builders;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class GivenDatastoreStepFacts : GivenDatastoreFacts
    {
        protected IStep Step;
        protected string ReceiptMessageId, ErrorMessageId, SignalMessageId;

        public GivenDatastoreStepFacts()
        {
            this.ReceiptMessageId = Guid.NewGuid().ToString();
            this.ErrorMessageId = Guid.NewGuid().ToString();
            this.SignalMessageId = Guid.NewGuid().ToString();

            SeedDataStore(this.Options);
        }

        protected Receipt GetReceipt()
        {
            return new Receipt(this.ReceiptMessageId)
            {
                RefToMessageId = this.ReceiptMessageId
            };
        }

        protected Error GetError()
        {
            return new Error(this.ErrorMessageId)
            {
                RefToMessageId = this.ErrorMessageId
            };
        }

        protected SignalMessage GetSignalMessage()
        {
            return new SignalMessage(this.SignalMessageId)
            {
                RefToMessageId = this.SignalMessageId
            };
        }

        protected void SeedDataStore(DbContextOptions<DatastoreContext> options)
        {
            using (var context = new DatastoreContext(options))
            {
                var receipt = new OutMessage { EbmsMessageId = GetReceipt().MessageId };
                var error = new OutMessage { EbmsMessageId = GetError().MessageId };
                var signalMessage = new OutMessage { EbmsMessageId = GetSignalMessage().MessageId };

                context.OutMessages.Add(receipt);
                context.OutMessages.Add(error);
                context.OutMessages.Add(signalMessage);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsAsync()
        {
            // Before
            if (this.Step == null) return;
            // Arrange
            var message = new AS4Message();
            var internalMessage = new InternalMessage(message);
            // Act
            StepResult result = await this.Step
                .ExecuteAsync(internalMessage, CancellationToken.None);
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ThenExecuteStepUpdatesAsErrorAsync()
        {
            // Before
            if (this.Step == null) return;
            
            // Arrange
            SignalMessage errorMessage = GetError();

            InternalMessage internalMessage = new InternalMessageBuilder(errorMessage.RefToMessageId)
                .WithSignalMessage(errorMessage).Build();
            internalMessage.AS4Message.SendingPMode = new SendingProcessingMode();
            internalMessage.AS4Message.ReceivingPMode = new ReceivingProcessingMode();

            // Act
            await this.Step.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            await AssertOutMessages(errorMessage, this.Options, OutStatus.Nack);
            await AssertInMessage(errorMessage, this.Options);
        }

        [Fact]
        public async Task ThenExecuteStepUpdatesAsReceiptAsync()
        {
            // Before
            if (this.Step == null) return;

            // Arrange
            SignalMessage receiptMessage = GetReceipt();

            InternalMessage internalMessage = new InternalMessageBuilder(receiptMessage.RefToMessageId)
                .WithSignalMessage(receiptMessage).Build();
            internalMessage.AS4Message.SendingPMode = new SendingProcessingMode();
            internalMessage.AS4Message.ReceivingPMode = new ReceivingProcessingMode();

            receiptMessage.RefToMessageId = internalMessage.AS4Message.PrimaryUserMessage.MessageId;

            // Act
            await this.Step.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            await AssertOutMessages(receiptMessage, this.Options, OutStatus.Ack);
            await AssertInMessage(receiptMessage, this.Options);
        }

        protected async Task AssertOutMessages(
            MessageUnit signalMessage, DbContextOptions<DatastoreContext> options, OutStatus status)
        {
            using (var context = new DatastoreContext(options))
            {
                OutMessage outMessage = await context.OutMessages.FirstOrDefaultAsync(
                    m => m.EbmsMessageId.Equals(signalMessage.MessageId));
                Assert.NotNull(outMessage);
                Assert.Equal(status, outMessage.Status);
            }
        }

        protected async Task AssertInMessage(MessageUnit signalMessage, DbContextOptions<DatastoreContext> options)
        {
            using (var context = new DatastoreContext(options))
            {
                InMessage inMessage = await context.InMessages.FirstOrDefaultAsync(
                    m => m.EbmsMessageId.Equals(signalMessage.MessageId));
                Assert.NotNull(inMessage);
                Assert.Equal(InStatus.Received, inMessage.Status);
            }
        }
    }
}
