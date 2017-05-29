using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Repositories
{
    /// <summary>
    /// Testing the <see cref="DatastoreRepository" />
    /// </summary>
    public class GivenDatastoreRepositoryFacts : GivenDatastoreFacts
    {

        public class OutMessages : GivenDatastoreRepositoryFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public void ThenGetOutMessageSucceeded(string sharedId)
            {
                // Arrange
                const Operation expected = Operation.Delivered;
                InsertOutMessage(sharedId, expected);

                using (DatastoreContext context = GetDataStoreContext())
                {
                    var repository = new DatastoreRepository(context);

                    // Act
                    Operation actual = repository.FirstOrDefaultOutMessage(sharedId, m => m.Operation);

                    // Assert
                    Assert.Equal(expected, actual);
                }
            }

            [Theory]
            [InlineData("shared-id")]
            public async Task ThenInsertOutMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                var outMessage = new OutMessage { EbmsMessageId = sharedId };

                // Act
                using (DatastoreContext context = GetDataStoreContext())
                {
                    new DatastoreRepository(context).InsertOutMessage(outMessage);

                    await context.SaveChangesAsync();
                }

                // Assert
                AssertOutMessage(outMessage.EbmsMessageId, Assert.NotNull);
            }

            [Theory]
            [InlineData("shared-id")]
            public void ThenUpdateOutMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                InsertOutMessage(sharedId, Operation.ToBeSent);

                // Act
                using (DatastoreContext context = GetDataStoreContext())
                {
                    new DatastoreRepository(context).UpdateOutMessage(
                       sharedId,
                       m => m.Operation = Operation.Sent);

                    context.SaveChanges();
                }

                // Assert
                AssertOutMessage(sharedId, m => Assert.Equal(Operation.Sent, m.Operation));
            }

            private void InsertOutMessage(string ebmsMessageId, Operation operation = Operation.NotApplicable)
            {
                using (var context = new DatastoreContext(Options))
                {
                    context.OutMessages.Add(new OutMessage() { EbmsMessageId = ebmsMessageId, Operation = operation });
                    context.SaveChanges();
                }
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (var contex = new DatastoreContext(Options))
                {
                    OutMessage outMessage = contex.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(outMessage);
                }
            }
        }

        public class InExceptions : GivenDatastoreRepositoryFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public async Task ThenInsertInExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                var inException = new InException { EbmsRefToMessageId = sharedId };

                // Act
                using (DatastoreContext context = GetDataStoreContext())
                {
                    new DatastoreRepository(context).InsertInException(inException);

                    await context.SaveChangesAsync();
                }

                AssertInException(inException.EbmsRefToMessageId, Assert.NotNull);
            }

            private void AssertInException(string messageId, Action<InException> assertAction)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InException inException =
                        context.InExceptions.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));
                    assertAction(inException);

                }
            }
        }

        public class OutExceptions : GivenDatastoreRepositoryFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public async Task ThenInsertOutExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                var outException = new OutException { EbmsRefToMessageId = sharedId };

                // Act
                using (DatastoreContext context = GetDataStoreContext())
                {
                    new DatastoreRepository(context).InsertOutException(outException);

                    await context.SaveChangesAsync();
                }

                // Assert
                AssertOutException(outException.EbmsRefToMessageId, Assert.NotNull);
            }

            private void AssertOutException(string messageId, Action<OutException> assertAction)
            {
                using (var contex = new DatastoreContext(Options))
                {
                    OutException outException =
                        contex.OutExceptions.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));
                    assertAction(outException);
                }
            }
        }

        public class InMessages : GivenDatastoreRepositoryFacts
        {
            [Fact]
            public void SelectsOnlyMessageLocation()
            {
                // Arrange
                const string messageId = "single-id";
                const string expected = "message-location";
                InsertInMessage(messageId, m => m.MessageLocation = expected);

                // Act
                string actual = ExerciseRepository(sut => sut.FirstOrDefaultInMessage(messageId, m => m.MessageLocation));

                // Assert
                Assert.Equal(expected, actual);
            }

            [Fact]
            public void GetsMessageIdsForFoundUserMessages()
            {
                TestFoundInMessagesFor(id => InsertInMessageWithOperation(id), repository => repository.SelectExistingInMessageIds);
            }

            [Fact]
            public void GetsMmessagesIdsForFoundSignalMessages()
            {
                TestFoundInMessagesFor(InsertRefInMessage, repository => repository.SelectExistingRefInMessageIds);
            }

            private void TestFoundInMessagesFor(Action<string> insertion, Func<DatastoreRepository, Func<IEnumerable<string>, IEnumerable<string>>> sutAction)
            {
                // Arrange
                const string expectedId = "message-id";
                insertion(expectedId);

                using (DatastoreContext context = GetDataStoreContext())
                {
                    var repository = new DatastoreRepository(context);
                    var expectedMessageIds = new[] { expectedId };

                    // Act
                    IEnumerable<string> actualMessageIds = sutAction(repository)(expectedMessageIds);

                    // Assert
                    Assert.Equal(expectedMessageIds, actualMessageIds);
                }
            }

            [Theory]
            [InlineData("shared-id")]
            public void ThenInMessageExistsSucceeded(string sharedId)
            {
                // Arrange
                InsertInMessageWithOperation(sharedId);

                using (DatastoreContext context = GetDataStoreContext())
                {
                    var repository = new DatastoreRepository(context);

                    // Act
                    bool result = repository.InMessageExists(m => m.EbmsMessageId == sharedId);

                    // Assert
                    Assert.True(result);
                }
            }

            [Theory]
            [InlineData("shared-id")]
            public void ThenInsertInMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                InsertInMessageWithOperation(sharedId);

                // Assert
                AssertInMessage(sharedId, Assert.NotNull);
            }

            [Theory]
            [InlineData("share-id")]
            public async Task ThenUpdateInMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                InsertInMessageWithOperation(sharedId, Operation.ToBeDelivered);

                // Act
                using (DatastoreContext context = GetDataStoreContext())
                {
                    new DatastoreRepository(context).UpdateInMessage(
                        sharedId,
                        m => m.Operation = Operation.Delivered);

                    await context.SaveChangesAsync();
                }

                // Assert
                AssertInMessage(sharedId, m => Assert.Equal(Operation.Delivered, m.Operation));
            }

            private void InsertInMessageWithOperation(string ebmsMessageId, Operation operation = Operation.NotApplicable)
            {
                InsertInMessage(ebmsMessageId, m => m.Operation = operation);
            }

            private void InsertInMessage(string messageId, Action<InMessage> arrangeMessage)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    var message = new InMessage {EbmsMessageId = messageId};
                    arrangeMessage(message);

                    context.InMessages.Add(message);
                    context.SaveChanges();
                }
            }

            private void InsertRefInMessage(string refToEbmsMessageId)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    context.InMessages.Add(new InMessage {EbmsRefToMessageId = refToEbmsMessageId});
                    context.SaveChanges();
                }
            }

            private TResult ExerciseRepository<TResult>(Func<DatastoreRepository, TResult> act)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    var sut = new DatastoreRepository(context);

                    // Act
                    return act(sut);
                }
            }

            private void AssertInMessage(string messageId, Action<InMessage> assertAction)
            {
                using (var context = new DatastoreContext(Options))
                {
                    InMessage inMessage = context.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(inMessage);
                }
            }
        }
    }
}