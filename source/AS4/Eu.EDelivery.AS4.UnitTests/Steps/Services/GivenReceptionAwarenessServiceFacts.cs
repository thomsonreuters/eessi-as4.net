using System;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    public class GivenReceptionAwarenessServiceFacts
    {
        private static TResult ExerciseService<TResult>(
            IDatastoreRepository repository,
            Func<ReceptionAwarenessService, TResult> act)
        {
            // Act
            return act(CreateServideWith(repository));
        }

        private static void ExerciseService(IDatastoreRepository repository, Action<ReceptionAwarenessService> act)
        {
            // Act
            act(CreateServideWith(repository));
        }

        private static ReceptionAwarenessService CreateServideWith(IDatastoreRepository repository)
        {
            return new ReceptionAwarenessService(repository);
        }

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

                stub.Setup(r => r.GetOutMessageData(It.IsAny<string>(), It.IsAny<Expression<Func<OutMessage, Operation>>>()))
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

                var awareness = new AS4.Entities.ReceptionAwareness
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
            public void CompletesReferecedMessage()
            {
                // Arrange
                var mockStore = new Mock<IDatastoreRepository>();
                var actual = new AS4.Entities.ReceptionAwareness { InternalMessageId = "message-id" };
                actual.SetStatus(ReceptionStatus.Busy);

                mockStore.Setup(
                            s =>
                                s.UpdateReceptionAwareness(
                                    It.IsAny<string>(),
                                    It.IsAny<Action<AS4.Entities.ReceptionAwareness>>()))
                        .Callback(
                             (string id, Action<AS4.Entities.ReceptionAwareness> updatedEntity) =>
                             {
                                 Assert.Equal(actual.InternalMessageId, id);
                                 updatedEntity(actual);
                             });

                // Act
                ExerciseService(mockStore.Object, s => s.MarkReferencedMessageAsComplete(actual));

                // Assert
                Assert.Equal(ReceptionStatus.Completed, ReceptionStatusUtils.Parse(actual.Status));
            }
        }

        public class UpdateForResend
        {
            [Fact]
            public void OperationToBeSentOfReferencedOutMessage()
            {
                // Arrange
                var mockRepository = new Mock<IDatastoreRepository>();
                var awareness = new AS4.Entities.ReceptionAwareness { InternalMessageId = "not empty message-id" };
                var actual = new OutMessage(Guid.NewGuid().ToString());
                actual.SetOperation(Operation.Sent);

                mockRepository.Setup(r => r.UpdateOutMessage(It.IsAny<string>(), It.IsAny<Action<OutMessage>>()))
                              .Callback(
                                  (string id, Action<OutMessage> updateEntity) =>
                                  {
                                      Assert.Equal(awareness.InternalMessageId, id);
                                      updateEntity(actual);
                                  });

                // Act
                ExerciseService(mockRepository.Object, s => s.MarkReferencedMessageForResend(awareness));

                // Assert
                Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(actual.Operation));
            }

            [Fact]
            public void StatusPendingOfReceptionAwarenessEntry()
            {
                // Arrange
                var mockRepository = new Mock<IDatastoreRepository>();
                var awareness = new AS4.Entities.ReceptionAwareness
                {
                    InternalMessageId = "not empty message-id"
                };

                awareness.SetStatus(ReceptionStatus.Busy);

                mockRepository.Setup(
                                  r =>
                                      r.UpdateReceptionAwareness(
                                          It.IsAny<string>(),
                                          It.IsAny<Action<AS4.Entities.ReceptionAwareness>>()))
                              .Callback(
                                  (string id, Action<AS4.Entities.ReceptionAwareness> updateEntry) =>
                                  {
                                      Assert.Equal(awareness.InternalMessageId, id);
                                      updateEntry(awareness);
                                  });

                // Act
                ExerciseService(mockRepository.Object, s => s.MarkReferencedMessageForResend(awareness));

                // Assert
                Assert.Equal(ReceptionStatus.Pending, ReceptionStatusUtils.Parse(awareness.Status));
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
                selectArgument.SetStatus(status);

                stubRepository.Setup(r => r.GetOutMessageData(It.IsAny<string>(), It.IsAny<Expression<Func<OutMessage, bool>>>()))
                              .Returns(
                                  (string id, Func<OutMessage, bool> selection) =>
                                      selection(selectArgument));

                var awareness = new AS4.Entities.ReceptionAwareness { InternalMessageId = messageId };

                // Act
                bool actual = ExerciseService(stubRepository.Object, s => s.IsMessageAlreadyAnswered(awareness));

                // Assert
                Assert.Equal(expected, actual);
                stubRepository.Verify(
                    expression: r => r.GetOutMessageData(It.IsAny<string>(), It.IsAny<Expression<Func<OutMessage, bool>>>()),
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
                var actual = new AS4.Entities.ReceptionAwareness();
                actual.SetStatus(ReceptionStatus.Busy);

                mockRepository.Setup(
                                  r =>
                                      r.UpdateReceptionAwareness(
                                          It.IsAny<string>(),
                                          It.IsAny<Action<AS4.Entities.ReceptionAwareness>>()))
                              .Callback(
                                  (string id, Action<AS4.Entities.ReceptionAwareness> updateEntry) =>
                                  {
                                      Assert.Equal(actual.InternalMessageId, id);
                                      updateEntry(actual);
                                  });

                // Act
                ExerciseService(mockRepository.Object, s => s.ResetReferencedMessage(actual));

                // Assert
                Assert.Equal(ReceptionStatus.Pending, ReceptionStatusUtils.Parse(actual.Status));
            }
        }
    }
}















