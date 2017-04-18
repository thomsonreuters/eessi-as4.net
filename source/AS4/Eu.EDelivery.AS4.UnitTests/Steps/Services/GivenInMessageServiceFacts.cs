using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
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
           TestFindMessageDuplicates((messageIds, service) => service.FindDuplicateUserMessageIds(messageIds));
        }

        [Fact]
        public void TestFindUserMessageDuplicates()
        {
            TestFindMessageDuplicates((messageIds, service) => service.FindDuplicateSignalMessageIds(messageIds));
        }

        private static void TestFindMessageDuplicates(Func<IEnumerable<string>, InMessageService, IDictionary<string, bool>> actAction)
        {
            // Arrange
            IEnumerable<string> expectedMessageIds = new[] { "known-messsage-id", "unknown-message-id" };
            Mock<IDatastoreRepository> mockedRepository = CreateMockedRepositoryThatHas(expectedMessageIds.ElementAt(0));
            var sut = new InMessageService(mockedRepository.Object);

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
                .Setup(r => r.SelectInMessageIdsIn(It.IsAny<IEnumerable<string>>()))
                .Returns(new[] { expectedMessageId });

            return mockedRepository;
        }
    }
}