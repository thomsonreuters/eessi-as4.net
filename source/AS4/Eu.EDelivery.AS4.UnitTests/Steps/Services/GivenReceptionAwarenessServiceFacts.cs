using System;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    public class GivenReceptionAwarenessServiceFacts
    {
        public class MessageNeedsToBeResendFacts
        {
            [Fact]
            public void MessageNeedsToBeResend()
            {
                TestMessageNeedsToBeResend(r => { }, expected: true);
            }

            [Fact]
            public void MessageDoesntNeedsToBeResend_IfStatusIsCompleted()
            {
                TestMessageNeedsToBeResend(r => r.SetStatus(ReceptionStatus.Completed), expected: false);
            }

            [Fact]
            public void MessageDoesntNeedsToBeResend_IfCurrentIsGreaterThanTotalInterval()
            {
                TestMessageNeedsToBeResend(
                    r =>
                    {
                        r.CurrentRetryCount = 1000;
                        r.TotalRetryCount = -1000;
                    }, expected: false);
            }

            [Fact]
            public void MessageDoesntNeedsToBeResent_IfNextDeadLineWillBeOutOfRange()
            {
                TestMessageNeedsToBeResend(
                    r =>
                    {
                        r.RetryInterval = "00:00:05";
                        r.LastSendTime = DateTimeOffset.Now;
                    }, expected: false);
            }

            private static IDatastoreRepository GetStubRepositoryWithOutMessageOperation(Operation operation)
            {
                var stub = new Mock<IDatastoreRepository>();

                stub.Setup(r => r.GetOutMessageData(It.IsAny<long>(), It.IsAny<Expression<Func<OutMessage, Operation>>>()))
                    .Returns(operation);

                return stub.Object;
            }

            private static void TestMessageNeedsToBeResend(
                Action<AS4.Entities.ReceptionAwareness> arrangeAwareness,
                bool expected,
                Operation referencedOperation = Operation.ToBeSent)
            {
                // Arrange
                IDatastoreRepository stubRepository = GetStubRepositoryWithOutMessageOperation(referencedOperation);

                var awareness = new AS4.Entities.ReceptionAwareness(1, "message-id")
                {
                    CurrentRetryCount = 1,
                    TotalRetryCount = 5,
                    RetryInterval = "00:00:05",
                    LastSendTime = DateTimeOffset.MinValue,
                };
                awareness.SetStatus(ReceptionStatus.Pending);

                arrangeAwareness(awareness);

                // Act
                bool actual = ExerciseService(stubRepository, r => r.MessageNeedsToBeResend(awareness));

                // Assert
                Assert.Equal(expected, actual);
            }
        }

        public class ModifyStatus
        {
            [Fact]
            public void CompletesReferencedMessage()
            {
                // Arrange
                var mockStore = new Mock<IDatastoreRepository>();
                var actual = new AS4.Entities.ReceptionAwareness(1, "message-id");
                actual.SetStatus(ReceptionStatus.Busy);

                mockStore.Setup(
                            s =>
                                s.UpdateReceptionAwareness(
                                    It.IsAny<long>(),
                                    It.IsAny<Action<AS4.Entities.ReceptionAwareness>>()))
                        .Callback(
                             (long id, Action<AS4.Entities.ReceptionAwareness> updatedEntity) =>
                             {
                                 Assert.Equal(actual.Id, id);
                                 updatedEntity(actual);
                             });

                // Act
                ExerciseService(mockStore.Object, s => s.MarkReferencedMessageAsComplete(actual));

                // Assert
                Assert.Equal(ReceptionStatus.Completed, actual.Status.ToEnum<ReceptionStatus>());
            }
        }

        public class UpdateForResend
        {
            [Fact]
            public void OperationToBeSentOfReferencedOutMessage()
            {
                // Arrange
                var mockRepository = new Mock<IDatastoreRepository>();
                var outMessage = new OutMessage(Guid.NewGuid().ToString());
                outMessage.InitializeIdFromDatabase(1);
                outMessage.SetOperation(Operation.Sent);

                var awareness = new AS4.Entities.ReceptionAwareness(outMessage.Id, "not empty message-id");
                awareness.InitializeIdFromDatabase(2);

                mockRepository.Setup(r => r.UpdateOutMessage(It.IsAny<long>(), It.IsAny<Action<OutMessage>>()))
                              .Callback(
                                  (long id, Action<OutMessage> updateEntity) =>
                                  {
                                      Assert.Equal(awareness.RefToOutMessageId, id);
                                      updateEntity(outMessage);
                                  });

                // Act
                ExerciseService(mockRepository.Object, s => s.MarkReferencedMessageForResend(awareness));

                // Assert
                Assert.Equal(Operation.ToBeSent, outMessage.Operation.ToEnum<Operation>());
            }

            [Fact]
            public void StatusPendingOfReceptionAwarenessEntry()
            {
                // Arrange
                var mockRepository = new Mock<IDatastoreRepository>();
                var awareness = new AS4.Entities.ReceptionAwareness(1, "not-empty-message-id");
                
                awareness.SetStatus(ReceptionStatus.Busy);

                mockRepository.Setup(
                                  r =>
                                      r.UpdateReceptionAwareness(
                                          It.IsAny<long>(),
                                          It.IsAny<Action<AS4.Entities.ReceptionAwareness>>()))
                              .Callback(
                                  (long id, Action<AS4.Entities.ReceptionAwareness> updateEntry) =>
                                  {
                                      Assert.Equal(awareness.Id, id);
                                      updateEntry(awareness);
                                  });

                // Act
                ExerciseService(mockRepository.Object, s => s.MarkReferencedMessageForResend(awareness));

                // Assert
                Assert.Equal(ReceptionStatus.Pending, awareness.Status.ToEnum<ReceptionStatus>());
            }
        }

        public class IsMessageAlreadyAnsweredFacts
        {
            [Theory]
            [InlineData(OutStatus.Ack, true)]
            [InlineData(OutStatus.Nack, true)]
            [InlineData(OutStatus.Exception, false)]
            [InlineData(OutStatus.Created, false)]
            public void TestAlreadyAnsweredMessage(OutStatus status, bool expected)
            {
                // Arranges
                var stubRepository = new Mock<IDatastoreRepository>();

                const string messageId = "message id";

                var selectArgument = new OutMessage(ebmsMessageId: messageId);
                selectArgument.InitializeIdFromDatabase(1);
                selectArgument.SetStatus(status);

                stubRepository.Setup(r => r.GetOutMessageData(It.IsAny<long>(), It.IsAny<Expression<Func<OutMessage, bool>>>()))
                              .Returns(
                                  (long id, Expression<Func<OutMessage, bool>> selection) =>
                                  {
                                      var f = selection.Compile();
                                      return f(selectArgument);
                                  });

                var awareness = new AS4.Entities.ReceptionAwareness(selectArgument.Id, selectArgument.EbmsMessageId);

                // Act
                bool actual = ExerciseService(stubRepository.Object, s => s.IsMessageAlreadyAnswered(awareness));

                // Assert
                Assert.Equal(expected, actual);
                stubRepository.Verify(
                    expression: r => r.GetOutMessageData(It.IsAny<long>(), It.IsAny<Expression<Func<OutMessage, bool>>>()),
                    times: Times.Once);
            }
        }

        public class ResetMessage
        {
            [Fact]
            public void UpdatePendingStatusOfReferencedMessage()
            {
                // Arrange
                var mockRepository = new Mock<IDatastoreRepository>();
                var actual = new AS4.Entities.ReceptionAwareness(1, "some-message-id");
                actual.SetStatus(ReceptionStatus.Busy);

                mockRepository.Setup(
                                  r =>
                                      r.UpdateReceptionAwareness(
                                          It.IsAny<long>(),
                                          It.IsAny<Action<AS4.Entities.ReceptionAwareness>>()))
                              .Callback(
                                  (long id, Action<AS4.Entities.ReceptionAwareness> updateEntry) =>
                                  {
                                      Assert.Equal(actual.Id, id);
                                      updateEntry(actual);
                                  });

                // Act
                ExerciseService(mockRepository.Object, s => s.ResetReferencedMessage(actual));

                // Assert
                Assert.Equal(ReceptionStatus.Pending, actual.Status.ToEnum<ReceptionStatus>());
            }
        }

        private static TResult ExerciseService<TResult>(
            IDatastoreRepository repository,
            Func<ReceptionAwarenessService, TResult> act)
        {
            // Act
            return act(CreateServiceWith(repository));
        }

        private static void ExerciseService(IDatastoreRepository repository, Action<ReceptionAwarenessService> act)
        {
            // Act
            act(CreateServiceWith(repository));
        }

        private static ReceptionAwarenessService CreateServiceWith(IDatastoreRepository repository)
        {
            return new ReceptionAwarenessService(repository);
        }
    }
}















