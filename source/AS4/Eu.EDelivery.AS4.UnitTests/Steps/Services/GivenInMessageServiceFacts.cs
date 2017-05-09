using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    /// <summary>
    /// Testing <see cref="InMessageService" />
    /// </summary>
    public class GivenInMessageServiceFacts
    {

        [Fact]
        public void TestFindSignalMessageDuplicates()
        {
            TestFindMessageDuplicates((messageIds, service) => service.DetermineDuplicateSignalMessageIds(messageIds));
        }

        [Fact]
        public void TestFindUserMessageDuplicates()
        {
            TestFindMessageDuplicates((messageIds, service) => service.DetermineDuplicateUserMessageIds(messageIds));
        }

        private static void TestFindMessageDuplicates(Func<IEnumerable<string>, InMessageService, IDictionary<string, bool>> actAction)
        {
            // Arrange
            IEnumerable<string> expectedMessageIds = new[] { "known-messsage-id", "unknown-message-id" };
            Mock<IDatastoreRepository> mockedRepository = CreateMockedRepositoryThatHas(expectedMessageIds.ElementAt(0));

            var sut = new InMessageService(mockedRepository.Object, CreateMockedAS4MessageBodyPersister().Object);

            // Act
            IDictionary<string, bool> actualDuplicates = actAction(expectedMessageIds, sut);

            // Assert
            Assert.True(actualDuplicates.ElementAt(0).Value);
            Assert.False(actualDuplicates.ElementAt(1).Value);
        }

        private static Mock<IAS4MessageBodyPersister> CreateMockedAS4MessageBodyPersister()
        {
            var inMessageBodyPersister = new Mock<IAS4MessageBodyPersister>();
            inMessageBodyPersister.Setup(m => m.SaveAS4MessageAsync(It.IsAny<AS4Message>(), It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

            return inMessageBodyPersister;
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