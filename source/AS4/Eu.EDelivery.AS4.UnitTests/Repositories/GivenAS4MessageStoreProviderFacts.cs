using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Repositories;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class GivenAS4MessageStoreProviderFacts
    {
        [Fact]
        public async Task SpyPersisterGetsCalled_IfLoadsBody()
        {
            await TestProviderWithAcceptedPersister(
                sut => sut.LoadMessagesBody("ignored location"),
                spy => spy.LoadMessagesBody(It.IsAny<string>()));
        }

        [Fact]
        public async Task SpyPersisterGetsCalled_IfSaveBody()
        {
            await TestProviderWithAcceptedPersister(
                sut => sut.SaveAS4MessageAsync("ignored location", null, CancellationToken.None),
                spy => spy.SaveAS4MessageAsync(It.IsAny<string>(), null, CancellationToken.None));
        }

        [Fact]
        public async Task SpyPersisterGetsCalled_IfUpdateBody()
        {
            await TestProviderWithAcceptedPersister(
                sut => sut.UpdateAS4MessageAsync("ignored location", null, CancellationToken.None),
                spy => spy.UpdateAS4MessageAsync(It.IsAny<string>(), null, CancellationToken.None));
        }

        private static async Task TestProviderWithAcceptedPersister(
            Func<MessageBodyStore, Task> act,
            Expression<Action<IAS4MessageBodyStore>> assertion)
        {
            // Arrange
            var spyPersister = Mock.Of<IAS4MessageBodyStore>();
            var sut = new MessageBodyStore();
            sut.Accept(location => true, spyPersister);

            // Act
            await act(sut);

            // Assert
            Mock.Get(spyPersister).Verify(assertion, Times.Once);
        }
    }
}
