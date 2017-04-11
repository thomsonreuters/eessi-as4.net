using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Repositories
{
    /// <summary>
    /// Testing the <see cref="DatastoreRepository" />
    /// </summary>
    public class GivenDatastoreRepositoryFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenDatastoreRepositoryFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public void ThenInMessageExistsSucceeded(string sharedId)
            {
                // Arrange
                InsertInMessage(sharedId, Operation.NotApplicable);

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
            public void ThenGetOutMessageSucceeded(string sharedId)
            {
                // Arrange
                InsertOutMessage(sharedId, Operation.NotApplicable);

                using (DatastoreContext context = GetDataStoreContext())
                {
                    var repository = new DatastoreRepository(context);

                    // Act
                    OutMessage resultMessage = repository.GetOutMessageById(sharedId);

                    // Assert
                    Assert.NotNull(resultMessage);
                }
            }

            [Theory]
            [InlineData("shared-id")]
            public void ThenInsertInMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                InsertInMessage(sharedId, Operation.NotApplicable);

                // Assert
                AssertInMessage(sharedId, Assert.NotNull);
            }

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

            [Theory]
            [InlineData("share-id")]
            public async Task ThenUpdateInMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                InsertInMessage(sharedId, Operation.ToBeDelivered);

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

            private void InsertInMessage(string ebmsMessageId, Operation operation)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    context.InMessages.Add(new InMessage(){EbmsMessageId = ebmsMessageId, Operation = operation});
                    context.SaveChanges();
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

            private void AssertInException(string messageId, Action<InException> assertAction)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InException inException =
                        context.InExceptions.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));
                    assertAction(inException);
                }
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

            private void InsertOutMessage( string ebmsMessageId, Operation operation )
            {
                using (var context = new DatastoreContext(Options))
                {
                    context.OutMessages.Add(new OutMessage(){EbmsMessageId = ebmsMessageId, Operation = operation});
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
    }
}