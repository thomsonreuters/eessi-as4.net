using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    /// <summary>
    /// Testing <see cref="InMessageService" />
    /// </summary>
    public class GivenInMessageServiceFacts
    {
        public class InsertAS4Message : GivenDatastoreFacts
        {
            private readonly InMemoryMessageBodyStore _bodyStore = new InMemoryMessageBodyStore();

            [Fact]
            public async Task Service_Uses_Deserialized_AS4Message_Instead_Of_Deserializing_Incoming_Stream()
            {
                // Arrange
                var ctx = new MessagingContext(
                    new ReceivedMessage(Stream.Null, Constants.ContentTypes.Soap),
                    MessagingContextMode.Receive);

                string ebmsMessageId = $"receipt-{Guid.NewGuid()}";
                AS4Message receipt = AS4Message.Create(new Receipt(ebmsMessageId));
                ctx.ModifyContext(receipt);

                using (DatastoreContext dbCtx = GetDataStoreContext())
                {
                    var sut = new InMessageService(
                        StubConfig.Default,
                        new DatastoreRepository(dbCtx));

                    // Act
                    await sut.InsertAS4MessageAsync(ctx, MessageExchangePattern.Push, _bodyStore);
                    dbCtx.SaveChanges();
                }

                // Assert
                GetDataStoreContext.AssertInMessage(ebmsMessageId, Assert.NotNull);
            }

            protected override void Disposing()
            {
                _bodyStore.Dispose();
            }
        }

        public class UpdateAS4Message
        {
            [Fact]
            public void FailsToUpdateMessage_IfNoMessageLocationCanBeFound()
            {
                // Arrange
                var notPopulatedRepository = Mock.Of<IDatastoreRepository>();
                var sut = new InMessageService(config: null, repository: notPopulatedRepository);

                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

                // Act / Assert
                Assert.ThrowsAny<InvalidDataException>(
                    () => sut.UpdateAS4MessageForMessageHandling(context, null));
            }
        }

        public class DetermineDuplicates
        {
            [Fact]
            public void TestFindSignalMessageDuplicates()
            {
                TestFindMessageDuplicates(
                    (messageIds, service) => service.DetermineDuplicateSignalMessageIds(messageIds));
            }

            [Fact]
            public void TestFindUserMessageDuplicates()
            {
                TestFindMessageDuplicates(
                    (messageIds, service) => service.DetermineDuplicateUserMessageIds(messageIds));
            }

            private static void TestFindMessageDuplicates(Func<IEnumerable<string>, InMessageService, IDictionary<string, bool>> actAction)
            {
                // Arrange
                IEnumerable<string> expectedMessageIds = new[] { "known-messsage-id", "unknown-message-id" };
                Mock<IDatastoreRepository> mockedRepository = CreateMockedRepositoryThatHas(expectedMessageIds.ElementAt(0));

                var sut = new InMessageService(StubConfig.Default, mockedRepository.Object);

                // Act
                IDictionary<string, bool> actualDuplicates = actAction(expectedMessageIds, sut);

                // Assert
                Assert.True(actualDuplicates.ElementAt(0).Value);
                Assert.False(actualDuplicates.ElementAt(1).Value);
            }

            private static Mock<IDatastoreRepository> CreateMockedRepositoryThatHas(string expectedMessageId)
            {
                var mockedRepository = new Mock<IDatastoreRepository>();

                mockedRepository
                    .Setup(r => r.SelectExistingInMessageIds(It.IsAny<IEnumerable<string>>()))
                    .Returns(new[] { expectedMessageId });

                mockedRepository
                    .Setup(r => r.SelectExistingRefInMessageIds(It.IsAny<IEnumerable<string>>()))
                    .Returns(new[] { expectedMessageId });

                return mockedRepository;
            }
        }
    }
}