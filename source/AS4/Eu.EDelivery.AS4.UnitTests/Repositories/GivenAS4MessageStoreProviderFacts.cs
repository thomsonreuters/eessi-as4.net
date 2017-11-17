using System;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Repositories;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class GivenAS4MessageStoreProviderFacts
    {

        [Fact]
        public void SpyPersisterGetsCalled_IfSaveBody()
        {
            TestProviderWithAcceptedPersister(
               sut => sut.SaveAS4Message("ignored location", null),
               spy => spy.SaveAS4Message(It.IsAny<string>(), null));
        }

        [Fact]
        public void SpyPersisterGetsCalled_IfUpdateBody()
        {
            TestProviderWithAcceptedPersister(
                sut => sut.UpdateAS4Message("ignored location", null),
                spy => spy.UpdateAS4Message(It.IsAny<string>(), null));
        }

        private static void TestProviderWithAcceptedPersister(
            Action<MessageBodyStore> act,
            Expression<Action<IAS4MessageBodyStore>> assertion)
        {
            // Arrange
            var spyPersister = Mock.Of<IAS4MessageBodyStore>();
            var sut = new MessageBodyStore();
            sut.Accept(location => true, spyPersister);

            // Act
            act(sut);

            // Assert
            Mock.Get(spyPersister).Verify(assertion, Times.Once);
        }
    }
}
