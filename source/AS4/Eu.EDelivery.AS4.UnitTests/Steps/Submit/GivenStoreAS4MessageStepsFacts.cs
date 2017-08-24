using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="StoreAS4MessageStep" />
    /// </summary>
    public class GivenStoreAS4MessageStepsFacts : GivenDatastoreFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        [Fact]
        public async Task MessageGetsSavedWithOperationToBeProcessed()
        {
            // Arrange
            string id = Guid.NewGuid().ToString();

            var sut = new StoreAS4MessageStep(GetDataStoreContext, _messageBodyStore);

            // Act
            await sut.ExecuteAsync(
                new MessagingContext(AS4Message.Create(new FilledUserMessage(id)), MessagingContextMode.Submit),
                CancellationToken.None);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                id,
                async m =>
                {
                    Assert.Equal(Operation.ToBeProcessed, m.Operation);
                    Assert.True(await _messageBodyStore.LoadMessageBodyAsync(m.MessageLocation) != Stream.Null);
                });
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }
    }
}