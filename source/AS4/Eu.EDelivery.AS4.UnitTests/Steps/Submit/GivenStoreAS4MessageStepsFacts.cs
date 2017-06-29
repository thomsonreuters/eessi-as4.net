using System;
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
        [Fact]
        public async Task MessageGetsSavedWithOperationToBeProcessed()
        {
            // Arrange
            string id = Guid.NewGuid().ToString(), 
                expected = Guid.NewGuid().ToString();

            var sut = new StoreAS4MessageStep(GetDataStoreContext, new StubMessageBodyStore(expected));

            // Act
            await sut.ExecuteAsync(
                new MessagingContext(AS4Message.Create(new FilledUserMessage(id)), MessagingContextMode.Submit),
                CancellationToken.None);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                id,
                m =>
                {
                    Assert.Equal(Operation.ToBeProcessed, m.Operation);
                    Assert.Equal(expected, m.MessageLocation);
                });
        }
    }
}