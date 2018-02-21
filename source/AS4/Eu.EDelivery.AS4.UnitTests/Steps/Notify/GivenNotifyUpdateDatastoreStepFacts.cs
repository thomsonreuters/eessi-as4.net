using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="NotifyUpdateDatastoreStep" />
    /// </summary>
    public class GivenNotifyUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task Then_Update_InMessage_Succeeds()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertInMessage(CreateInMessage(ebmsMessageId));

            // Act
            await ExerciseUpdateDatastoreEntity<InMessage>(ebmsMessageId);

            // Assert
            GetDataStoreContext.AssertInMessage(
                ebmsMessageId,
                m =>
                {
                    Assert.Equal(Operation.Notified, OperationUtils.Parse(m.Operation));
                    Assert.Equal(InStatus.Notified, InStatusUtils.Parse(m.Status));
                });
        }

        private static InMessage CreateInMessage(string id)
        {
            var inMessage = new InMessage(id);
            inMessage.SetOperation(Operation.Notifying);
            inMessage.SetStatus(InStatus.Delivered);

            return inMessage;
        }

        [Fact]
        public async Task Then_Update_OutMessage_Succeeds()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertOutMessage(CreateOutMessage(ebmsMessageId));

            // Act
            await ExerciseUpdateDatastoreEntity<OutMessage>(ebmsMessageId);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                ebmsMessageId,
                m =>
                {
                    Assert.Equal(Operation.Notified, OperationUtils.Parse(m.Operation));
                    Assert.Equal(OutStatus.Notified, OutStatusUtils.Parse(m.Status));
                });
        }

        private static OutMessage CreateOutMessage(string sharedId)
        {
            var outMessage = new OutMessage(sharedId);
            outMessage.SetStatus(OutStatus.Ack);
            outMessage.SetOperation(Operation.Notifying);

            return outMessage;
        }

        [Fact]
        public async Task Then_Update_InException_Succeeds()
        {
            // Arrange
            string refToMessageId = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertInException(CreateInException(refToMessageId));

            // Act
            await ExerciseUpdateDatastoreEntity<InException>(refToMessageId);

            // Assert
            GetDataStoreContext.AssertInException(
                CreateInException(refToMessageId).EbmsRefToMessageId,
                ex => Assert.Equal(Operation.Notified, OperationUtils.Parse(ex.Operation)));
        }

        private static InException CreateInException(string id)
        {
            var exception = new InException(id, errorMessage: "");
            exception.SetOperation(Operation.ToBeNotified);

            return exception;
        }

        [Fact]
        public async Task Then_Update_OutException_Succeeds()
        {
            // Arrange
            string refToMessageId = Guid.NewGuid().ToString();
            OutException outException = CreateOutException(refToMessageId);
            GetDataStoreContext.InsertOutException(outException);

            // Act
            await ExerciseUpdateDatastoreEntity<OutException>(refToMessageId);

            // Assert
            GetDataStoreContext.AssertOutException(
                outException.EbmsRefToMessageId,
                ex => Assert.Equal(Operation.Notified, OperationUtils.Parse(ex.Operation)));
        }

        private static OutException CreateOutException(string refId)
        {
            var exception = new OutException(refId, errorMessage: "");
            exception.SetOperation(Operation.ToBeNotified);

            return exception;
        }

        private static async Task ExerciseUpdateDatastoreEntity<T>(string ebmsMessageId)
        {
            NotifyMessageEnvelope notifyMessage = CreateNotifyMessage(ebmsMessageId, typeof(T));

            // Act
            var sut = new NotifyUpdateDatastoreStep();
            await sut.ExecuteAsync(new MessagingContext(notifyMessage), CancellationToken.None);
        }

        private static NotifyMessageEnvelope CreateNotifyMessage(string id, Type type)
        {
            return new NotifyMessageEnvelope(
                messageInfo: new MessageInfo {MessageId = id, RefToMessageId = id},
                statusCode: Status.Delivered,
                notifyMessage: null,
                contentType: string.Empty,
                entityType: type);
        }
    }
}