using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="StoreAS4MessageStep" />
    /// </summary>
    public class GivenStoreAS4MessageStepsFacts : GivenDatastoreFacts
    {
        private readonly StoreAS4MessageStep _module;

        public GivenStoreAS4MessageStepsFacts()
        {            
            _module = new StoreAS4MessageStep(StubMessageBodyStore.Default);
        }

        /// <summary>
        /// Testing if the module succeeds
        /// </summary>
        public class GivenStoreAs4MessageStepsSucceeds : GivenStoreAS4MessageStepsFacts
        {
            [Fact]
            public async Task ThenTransmitMessageSucceedsAsync()
            {
                // Act
                StepResult result = await _module.ExecuteAsync(AS4UserMessage(), CancellationToken.None);

                // Assert
                Assert.NotNull(result);
            }
        }

        /// <summary>
        /// Testing if the module fails
        /// </summary>
        public class GivenStoreAs4MessageStepsFails : GivenStoreAS4MessageStepsFacts
        {
            [Fact]
            public async Task StoreMessageFails_IfInvalidBodyPersister()
            {
                // Arrange
                var sut = new StoreAS4MessageStep(null);

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(() => sut.ExecuteAsync(AS4UserMessage(), CancellationToken.None));
            }

            [Fact]
            public async Task ThenTransmitMessageFailsWithNullAS4MessageAsync()
            {
                // Act / Assert
                await Assert.ThrowsAsync<NullReferenceException>(
                    () => _module.ExecuteAsync(null, CancellationToken.None));
            }
        }

        protected static MessagingContext AS4UserMessage()
        {
            return new MessagingContext(AS4Message.Create(new UserMessage("message-id")));
        }
    }
}